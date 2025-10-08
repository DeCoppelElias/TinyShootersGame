using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.Users;
using System.Linq;

public class PVPBattleManager : MonoBehaviour
{
    public static PVPBattleManager Instance { get; private set; }

    [SerializeField] private Dictionary<System.Guid, Player> playerDict = new Dictionary<System.Guid, Player>();
    [SerializeField] private List<System.Guid> alivePlayers = new List<System.Guid>();
     private Dictionary<System.Guid, int> playerScores = new Dictionary<System.Guid, int>();

    [SerializeField] private List<Vector3> levels;
    [SerializeField] private List<ListWrapper> localPlayerSpawnPositions;
    [SerializeField] private List<ListWrapper> localEnemySpawnPositions;

    [SerializeField] private int matches = 3;
    [SerializeField] private int currentMatch = 0;
    [SerializeField] private int matchCooldown = 5;

    [SerializeField] private GameObject playerNamePrefab;

    [SerializeField] private PlayerStats pvpBaseStats;


    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Player[] players = FindObjectsOfType<Player>();
        LobbyPlayer[] lobbyPlayers = FindObjectsOfType<LobbyPlayer>();

        Player player1 = players[0];
        Player player2 = players[1];

        player1.playerIndex = 1;
        player2.playerIndex = 2;

        if (lobbyPlayers.Length == 2)
        {
            LobbyPlayer lowestLobbyPlayer = FindLowestIndexLobbyPlayer(lobbyPlayers);
            LobbyPlayer highestLobbyPlayer = FindHighestIndexLobbyPlayer(lobbyPlayers);

            LinkLobbyPlayerInput(player1, lowestLobbyPlayer);
            LinkLobbyPlayerInput(player2, highestLobbyPlayer);
        }

        player1.gameObject.name = $"Player {player1.playerIndex}";
        player2.gameObject.name = $"Player {player2.playerIndex}";

        Canvas playerCanvas1 = player1.GetComponentInChildren<Canvas>();
        UIPlayerScaler.Instance.Register(player1.playerIndex, playerCanvas1);
        Canvas playerCanvas2 = player2.GetComponentInChildren<Canvas>();
        UIPlayerScaler.Instance.Register(player2.playerIndex, playerCanvas2);

        int i = 1;
        foreach (Player player in players)
        {
            this.playerDict.Add(player.EntityID, player);

            // Give player PVP base stats
            player.baseStats = pvpBaseStats;
            player.Reset();

            // Make sure players do not switch controls
            player.GetComponent<PlayerInput>().neverAutoSwitchControlSchemes = true;

            // Link Match end to player death
            player.onDeath.AddListener(() => PlayerDied(player.gameObject));

            // Give name
            GameObject playerNameGO = Instantiate(playerNamePrefab, player.transform);
            TextMeshPro playerNameText = playerNameGO.GetComponent<TextMeshPro>();
            playerNameText.text = player.gameObject.name;

            // Add score
            playerScores.Add(player.EntityID, 0);

            i++;
        }

        StartMatch();
    }

    private PlayerInput LinkLobbyPlayerInput(Player player, LobbyPlayer lobbyPlayer)
    {
        PlayerInput gamePlayerInput = player.GetComponent<PlayerInput>();
        PlayerInput lobbyPlayerInput = lobbyPlayer.GetComponentInChildren<PlayerInput>();

        lobbyPlayerInput.transform.SetParent(player.transform);
        lobbyPlayerInput.uiInputModule = gamePlayerInput.uiInputModule;
        lobbyPlayerInput.camera = gamePlayerInput.camera;

        player.playerIndex = lobbyPlayer.playerIndex;
        player.GetComponent<PlayerController>().SetPlayerInput(lobbyPlayerInput);
        player.GetComponentInChildren<InputSystemUIInputModule>().actionsAsset = lobbyPlayerInput.actions;
        player.GetComponentInChildren<PlayerUIManager>().SetCurrentControllingPlayer(lobbyPlayerInput);
        player.Color = lobbyPlayer.color;

        Destroy(gamePlayerInput);
        return lobbyPlayerInput;
    }

    private void StartMatch()
    {
        // Choose Level and set player and camera positions
        int r = Random.Range(0, levels.Count);
        Vector3 levelPosition = levels[r];
        Debug.Log($"Choose level: {levelPosition}");
        Camera.main.transform.position = levelPosition + new Vector3(0, 0, -10);

        ResetPlayers();

        List<Player> orderedPlayers = playerDict.Values.OrderBy(obj => obj.playerIndex).ToList();
        List<Vector3> playerPositions = localPlayerSpawnPositions[r].list.OrderBy(pos => pos.x).ToList();

        for (int i = 0; i < orderedPlayers.Count; i++) orderedPlayers[i].transform.position = playerPositions[i] + levelPosition;

        // Update score UI
        Dictionary<string, int> scoreDict = new Dictionary<string, int>();
        foreach (KeyValuePair<System.Guid, int> pair in playerScores)
        {
            scoreDict.Add(playerDict[pair.Key].name, pair.Value);
        }
        SharedUIManager.Instance.GetUIElement<PVPStatusUI>().UpdateScores(scoreDict);
    }

    private void ResetPlayers()
    {
        alivePlayers.Clear();
        foreach (Player player in playerDict.Values)
        {
            player.gameObject.SetActive(true);
            player.Revive();
            alivePlayers.Add(player.EntityID);
        }
    }

    private void PlayerDied(GameObject playerGO)
    {
        Player player = playerGO.GetComponent<Player>();
        alivePlayers.Remove(player.EntityID);
        playerGO.SetActive(false);

        if (alivePlayers.Count <= 1) MatchEnd(alivePlayers[0]);
    }

    private void MatchEnd(System.Guid winnerID)
    {
        if (alivePlayers.Count > 1) return;

        Debug.Log("Match Done");

        if (playerScores.ContainsKey(winnerID)) playerScores[winnerID] += 1;
        else playerScores.Add(winnerID, 1);

        currentMatch += 1;
        SharedUIManager.Instance.GetUIElement<PVPStatusUI>().PerformCountdown(matchCooldown);

        // Update score UI
        Dictionary<string, int> scoreDict = new Dictionary<string, int>();
        foreach (KeyValuePair<System.Guid, int> pair in playerScores)
        {
            scoreDict.Add(playerDict[pair.Key].name, pair.Value);
        }
        SharedUIManager.Instance.GetUIElement<PVPStatusUI>().UpdateScores(scoreDict);

        StartCoroutine(PerformAfterDelay(matchCooldown+1.5f, () =>
        {
            if (currentMatch >= matches) EndPVP();
            else
            {
                StartMatch();

                foreach (Player player in playerDict.Values)
                {
                    PlayerUIManager playerUIManager = player.GetComponentInChildren<PlayerUIManager>();
                    playerUIManager.Enable<PowerupUI>();
                    StartCoroutine(PerformAfterDelay(0.1f, playerUIManager.Enable<UpgradeUI>));
                }
            }
        }));
    }

    private void EndPVP()
    {
        ResetPlayers();

        System.Guid maxKey = playerScores.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
        Player player = playerDict.Values.FirstOrDefault(p => p.EntityID == maxKey);
        SharedUIManager.Instance.Enable<PVPEndMatchUI, PVPEndMatchUIData>(new PVPEndMatchUIData(player.gameObject.name));
    }

    public void Restart()
    {
        LobbyPlayer[] lobbyPlayers = FindObjectsOfType<LobbyPlayer>();
        List<Player> players = new List<Player>(this.playerDict.Values);

        players[0].GetComponentInChildren<PlayerInput>().transform.SetParent(lobbyPlayers[0].transform);
        players[1].GetComponentInChildren<PlayerInput>().transform.SetParent(lobbyPlayers[1].transform);

        GameStateManager.Instance.ToRunning();

        SceneManager.LoadScene("PVPBattle");
    }

    [System.Serializable]
    private class ListWrapper
    {
        public List<Vector3> list;
    }

    private IEnumerator PerformAfterDelay(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);

        action();
    }

    private LobbyPlayer FindLowestIndexLobbyPlayer(LobbyPlayer[] lobbyPlayers)
    {
        if (lobbyPlayers.Length == 0) return null;

        LobbyPlayer lowestPlayer = lobbyPlayers[0];
        foreach (LobbyPlayer lobbyPlayer in lobbyPlayers)
        {
            if (lobbyPlayer.playerIndex < lowestPlayer.playerIndex) lowestPlayer = lobbyPlayer;
        }

        return lowestPlayer;
    }
    private LobbyPlayer FindHighestIndexLobbyPlayer(LobbyPlayer[] lobbyPlayers)
    {
        if (lobbyPlayers.Length == 0) return null;

        LobbyPlayer highestPlayer = lobbyPlayers[0];
        foreach (LobbyPlayer lobbyPlayer in lobbyPlayers)
        {
            if (lobbyPlayer.playerIndex > highestPlayer.playerIndex) highestPlayer = lobbyPlayer;
        }

        return highestPlayer;
    }
}
