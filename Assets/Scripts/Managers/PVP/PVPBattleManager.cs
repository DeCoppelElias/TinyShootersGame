using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using TMPro;
using UnityEngine.Experimental.Rendering.Universal;

public class PVPBattleManager : MonoBehaviour
{
    public static PVPBattleManager Instance { get; private set; }

    [Header("Players & State")]
    [SerializeField] private Dictionary<System.Guid, Player> playerDict = new Dictionary<System.Guid, Player>();
    [SerializeField] private List<System.Guid> alivePlayers = new List<System.Guid>();
    private Dictionary<System.Guid, int> playerScores = new Dictionary<System.Guid, int>();

    [Header("Match")]
    [SerializeField] private int matches = 3;
    [SerializeField] private int currentMatch = 0;
    [SerializeField] private int matchCooldown = 5;

    [Header("Prefabs / UI")]
    [SerializeField] private GameObject playerNamePrefab;
    [SerializeField] private PlayerStats pvpBaseStats;

    public UnityEvent<PVPStage> OnBattleStarted;
    public UnityEvent OnBattleEnded;

    private void Awake()
    {
        Instance = this;

        OnBattleStarted = new UnityEvent<PVPStage>();
        OnBattleEnded = new UnityEvent();
    }

    private void Start()
    {
        Player[] players = FindObjectsOfType<Player>();
        LobbyPlayer[] lobbyPlayers = FindObjectsOfType<LobbyPlayer>();

        // Sort players by their internal playerIndex so we keep deterministic order
        List<Player> gamePlayers = players.OrderBy(p => p.playerIndex).ToList();
        List<LobbyPlayer> lobbySorted = lobbyPlayers.OrderBy(lp => lp.playerIndex).ToList();

        // If exactly two lobby players exist, preserve your previous special-case behavior:
        // link lowest-index lobby player with the first game player and highest with second
        if (lobbyPlayers.Length == 2 && gamePlayers.Count >= 2)
        {
            LobbyPlayer lowest = FindLowestIndexLobbyPlayer(lobbyPlayers);
            LobbyPlayer highest = FindHighestIndexLobbyPlayer(lobbyPlayers);

            // pick the first two game players (this matches original code's player1/player2 assignment)
            Player player1 = gamePlayers[0];
            Player player2 = gamePlayers[1];

            LinkLobbyPlayerInput(player1, lowest);
            LinkLobbyPlayerInput(player2, highest);

            player1.playerIndex = lowest.playerIndex;
            player2.playerIndex = highest.playerIndex;

            player1.gameObject.name = $"Player {player1.playerIndex}";
            player2.gameObject.name = $"Player {player2.playerIndex}";

            // register canvases for both players (keeps original behavior)
            Canvas playerCanvas1 = player1.GetComponentInChildren<Canvas>();
            UIPlayerScaler.Instance.Register(player1.playerIndex, playerCanvas1);
            Canvas playerCanvas2 = player2.GetComponentInChildren<Canvas>();
            UIPlayerScaler.Instance.Register(player2.playerIndex, playerCanvas2);
        }
        else
        {
            // General mapping: link in-order lobby players to in-order game players
            // If there are more game players than lobby players, only link up to min count.
            int pairCount = Mathf.Min(gamePlayers.Count, lobbySorted.Count);

            for (int i = 0; i < pairCount; i++)
            {
                LinkLobbyPlayerInput(gamePlayers[i], lobbySorted[i]);
                gamePlayers[i].playerIndex = lobbySorted[i].playerIndex;
                gamePlayers[i].gameObject.name = $"Player {gamePlayers[i].playerIndex}";

                Canvas playerCanvas = gamePlayers[i].GetComponentInChildren<Canvas>();
                UIPlayerScaler.Instance.Register(gamePlayers[i].playerIndex, playerCanvas);
            }

            // For any remaining game players (without lobby assignment), still set a deterministic name/index
            // (they keep their existing playerIndex, or we assign sequential index if it's zero)
            for (int i = pairCount; i < gamePlayers.Count; i++)
            {
                if (gamePlayers[i].playerIndex == 0)
                    gamePlayers[i].playerIndex = i + 1; // fallback index
                gamePlayers[i].gameObject.name = $"Player {gamePlayers[i].playerIndex}";

                Canvas playerCanvas = gamePlayers[i].GetComponentInChildren<Canvas>();
                UIPlayerScaler.Instance.Register(gamePlayers[i].playerIndex, playerCanvas);
            }
        }

        // Register all players in dictionaries & setup base stats, listeners, UI names etc.
        foreach (Player player in FindObjectsOfType<Player>()) // re-fetch to include any whose input was linked above
        {
            if (!playerDict.ContainsKey(player.EntityID))
            {
                playerDict.Add(player.EntityID, player);
            }

            // Give player PVP base stats (same behavior as before)
            player.baseStats = pvpBaseStats;
            player.Reset();

            // Prevent input switching (same as before)
            var pInput = player.GetComponent<PlayerInput>();
            if (pInput != null)
                pInput.neverAutoSwitchControlSchemes = true;

            // Hook death callback to our PlayerDied method (preserves original behaviour)
            player.onDeath.AddListener(() => PlayerDied(player.gameObject));

            // Instantiate name label under player transform (same as before)
            if (playerNamePrefab != null)
            {
                GameObject playerNameGO = Instantiate(playerNamePrefab, player.transform);
                TextMeshPro playerNameText = playerNameGO.GetComponent<TextMeshPro>();
                if (playerNameText != null) playerNameText.text = player.gameObject.name;
            }

            // Initialize score entry if not present
            if (!playerScores.ContainsKey(player.EntityID))
                playerScores.Add(player.EntityID, 0);
        }

        // Begin the first match
        StartMatch();
    }

    private PlayerInput LinkLobbyPlayerInput(Player player, LobbyPlayer lobbyPlayer)
    {
        if (player == null || lobbyPlayer == null) return null;

        PlayerInput gamePlayerInput = player.GetComponent<PlayerInput>();
        PlayerInput lobbyPlayerInput = lobbyPlayer.GetComponentInChildren<PlayerInput>();

        if (lobbyPlayerInput == null || gamePlayerInput == null)
            return null;

        // Set hierarchy and UI routing like before
        lobbyPlayerInput.transform.SetParent(player.transform);
        lobbyPlayerInput.uiInputModule = gamePlayerInput.uiInputModule;
        lobbyPlayerInput.camera = gamePlayerInput.camera;

        player.playerIndex = lobbyPlayer.playerIndex;
        player.GetComponent<PlayerController>().SetPlayerInput(lobbyPlayerInput);
        var uiInputModule = player.GetComponentInChildren<InputSystemUIInputModule>();
        if (uiInputModule != null) uiInputModule.actionsAsset = lobbyPlayerInput.actions;
        var playerUIManager = player.GetComponentInChildren<PlayerUIManager>();
        if (playerUIManager != null) playerUIManager.SetCurrentControllingPlayer(lobbyPlayerInput);
        player.Color = lobbyPlayer.color;

        // Destroy the original scene PlayerInput to avoid double inputs
        Destroy(gamePlayerInput);

        return lobbyPlayerInput;
    }

    private void StartMatch()
    {
        PVPStage stage = LoadRandomStage();
        if (stage == null)
        {
            Debug.LogError("[PVPBattleManager] No stage loaded. Aborting StartMatch.");
            return;
        }

        if (Camera.main != null)
            Camera.main.transform.position = stage.GetCameraLocation();

        ResetPlayers();

        List<Vector3> orderedLocalPlayerPositions = stage.GetPlayerSpawnLocations().OrderBy(v => v.x).ToList();
        List<Player> orderedPlayers = playerDict.Values.OrderBy(p => p.playerIndex).ToList();
        for (int i = 0; i < orderedPlayers.Count; i++)
        {
            if (i < orderedLocalPlayerPositions.Count)
            {
                orderedPlayers[i].transform.position = orderedLocalPlayerPositions[i];
            }
            else Debug.LogError("Not enough spawn locations for players!");
        }

        Dictionary<string, int> scoreDict = new Dictionary<string, int>();
        foreach (KeyValuePair<System.Guid, int> pair in playerScores)
        {
            if (playerDict.ContainsKey(pair.Key))
                scoreDict.Add(playerDict[pair.Key].name, pair.Value);
        }
        SharedUIManager.Instance.GetUIElement<PVPStatusUI>().UpdateScores(scoreDict);

        OnBattleStarted?.Invoke(stage);
    }

    private PVPStage LoadRandomStage()
    {
        TextAsset[] stageFiles = Resources.LoadAll<TextAsset>("PVPStages");

        if (stageFiles == null || stageFiles.Length == 0)
        {
            Debug.LogError("[PVPBattleManager] No stages found in Resources/PVPStages!");
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, stageFiles.Length);
        TextAsset chosenStageFile = stageFiles[randomIndex];
        PVPStage stage = JsonUtility.FromJson<PVPStage>(chosenStageFile.text);

        if (stage == null)
        {
            Debug.LogError($"[PVPBattleManager] Failed to parse JSON for {chosenStageFile.name}");
            return null;
        }

        Debug.Log($"[PVPBattleManager] Loaded random stage: {chosenStageFile.name}");
        return stage;
    }

    private void ResetPlayers()
    {
        alivePlayers.Clear();
        foreach (Player player in playerDict.Values)
        {
            EnablePlayer(player, true);
            player.Revive();
            if (!alivePlayers.Contains(player.EntityID))
                alivePlayers.Add(player.EntityID);
        }
    }

    private void EnablePlayer(Player player, bool enable)
    {
        SpriteRenderer[] spriteRenderers = player.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer spr in spriteRenderers) spr.enabled = enable;

        TextMeshPro[] texts = player.GetComponentsInChildren<TextMeshPro>();
        foreach (TextMeshPro text in texts) text.enabled = enable;

        ShadowCaster2D[] shadowCasters = player.GetComponentsInChildren<ShadowCaster2D>();
        foreach (ShadowCaster2D shadowCaster in shadowCasters) shadowCaster.enabled = enable;

        PlayerController playerController = player.GetComponent<PlayerController>();
        playerController.collectInput = enable;
    }

    private void PlayerDied(GameObject playerGO)
    {
        Player player = playerGO.GetComponent<Player>();
        if (player == null) return;

        alivePlayers.Remove(player.EntityID);
        EnablePlayer(player, false);

        // If one player remains, end match
        if (alivePlayers.Count == 1)
        {
            MatchEnd(alivePlayers[0]);
        }
    }

    private void MatchEnd(System.Guid winnerID)
    {
        if (alivePlayers.Count != 1) return;

        Debug.Log("Match Done");

        if (playerScores.ContainsKey(winnerID)) playerScores[winnerID] += 1;
        else playerScores.Add(winnerID, 1);

        currentMatch += 1;
        SharedUIManager.Instance.GetUIElement<PVPStatusUI>().PerformCountdown(matchCooldown);

        // Update score UI
        Dictionary<string, int> scoreDict = new Dictionary<string, int>();
        foreach (KeyValuePair<System.Guid, int> pair in playerScores)
        {
            if (playerDict.ContainsKey(pair.Key))
                scoreDict.Add(playerDict[pair.Key].name, pair.Value);
        }
        SharedUIManager.Instance.GetUIElement<PVPStatusUI>().UpdateScores(scoreDict);

        StartCoroutine(PerformAfterDelay(matchCooldown + 1.5f, () =>
        {
            if (currentMatch >= matches) EndPVP();
            else
            {
                StartMatch();

                // After respawn, show powerup/upgrade UI for every player (preserves original loop)
                foreach (Player player in playerDict.Values)
                {
                    PlayerUIManager playerUIManager = player.GetComponentInChildren<PlayerUIManager>();
                    if (playerUIManager == null) continue;

                    playerUIManager.Enable<PowerupUI>();
                    StartCoroutine(PerformAfterDelay(0.1f, playerUIManager.Enable<UpgradeUI>));
                }
            }
        }));

        OnBattleEnded?.Invoke();
    }

    private void EndPVP()
    {
        ResetPlayers();

        if (playerScores.Count == 0) return;

        System.Guid maxKey = playerScores.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
        Player player = playerDict.Values.FirstOrDefault(p => p.EntityID == maxKey);
        if (player != null)
            SharedUIManager.Instance.Enable<PVPEndMatchUI, PVPEndMatchUIData>(new PVPEndMatchUIData(player.gameObject.name));
    }

    public void Restart()
    {
        LobbyPlayer[] lobbyPlayers = FindObjectsOfType<LobbyPlayer>();
        List<Player> players = new List<Player>(this.playerDict.Values);

        List<Player> orderedPlayers = players.OrderBy(p => p.playerIndex).ToList();
        List<LobbyPlayer> orderedLobbyPlayers = lobbyPlayers.OrderBy(lp => lp.playerIndex).ToList();

        // Re-parent player's input to corresponding lobby players if possible (preserve behaviour)
        int count = Mathf.Min(orderedPlayers.Count, orderedLobbyPlayers.Count);
        for (int i = 0; i < count; i++)
        {
            var playerInput = orderedPlayers[i].GetComponentInChildren<PlayerInput>();
            if (playerInput != null)
                playerInput.transform.SetParent(orderedLobbyPlayers[i].transform);
        }

        GameStateManager.Instance.ToRunning();
        SceneManager.LoadScene("PVPBattle");
    }

    public void QuitToMainMenu()
    {
        LobbyPlayer[] lobbyPlayers = FindObjectsOfType<LobbyPlayer>();
        foreach (LobbyPlayer lobbyPlayer in lobbyPlayers) Destroy(lobbyPlayer.gameObject);

        List<Player> players = new List<Player>(this.playerDict.Values);
        foreach (Player player in players)
        {
            var controller = player.GetComponent<PlayerController>();
            if (controller != null)
                controller.collectInput = false;
        }

        GameStateManager.Instance.QuitToMainMenu();
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

    [System.Serializable]
    private class ListWrapper
    {
        public List<Vector3> list;
    }
}
