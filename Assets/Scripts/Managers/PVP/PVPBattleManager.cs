using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;

public class PVPBattleManager : MonoBehaviour
{
    public static PVPBattleManager Instance { get; private set; }

    [SerializeField] private List<GameObject> playerGOs = new List<GameObject>();
    [SerializeField] private List<System.Guid> alivePlayers = new List<System.Guid>();
     private Dictionary<System.Guid,int> playerScores = new Dictionary<System.Guid, int>();

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

        int i = 1;
        foreach (Player player in players)
        {
            GameObject playerGO = player.gameObject;
            playerGOs.Add(playerGO);

            // Give player PVP base stats
            player.baseStats = pvpBaseStats;
            player.Reset();

            // Make sure players do not switch controls
            player.GetComponent<PlayerInput>().neverAutoSwitchControlSchemes = true;

            // Link Match end to player death
            player.onDeath.AddListener(() => PlayerDied(playerGO));

            // Give name
            GameObject playerNameGO = Instantiate(playerNamePrefab, playerGO.transform);
            TextMeshPro playerNameText = playerNameGO.GetComponent<TextMeshPro>();
            playerNameText.text = $"Player {i}";

            i++;
        }

        StartMatch();
    }

    private void StartMatch()
    {
        // Choose Level and set player and camera positions
        int r = Random.Range(0, levels.Count);
        Vector3 levelPosition = levels[r];
        Debug.Log($"Choose level: {levelPosition}");
        Camera.main.transform.position = levelPosition + new Vector3(0, 0, -10);

        int i = 0;
        alivePlayers.Clear();
        foreach (GameObject playerGO in playerGOs)
        {
            // Enable player
            playerGO.SetActive(true);
            Player player = playerGO.GetComponent<Player>();
            alivePlayers.Add(player.EntityID);

            // Revive player
            player.Revive();

            // Player position
            player.transform.position = localPlayerSpawnPositions[r].list[i] + levelPosition;

            i++;
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

        StartCoroutine(PerformAfterDelay(matchCooldown, () =>
        {
            if (currentMatch >= matches) EndPVP();
            else
            {
                StartMatch();

                foreach (GameObject playerGO in playerGOs)
                {
                    PlayerUIManager playerUIManager = playerGO.GetComponentInChildren<PlayerUIManager>();
                    playerUIManager.Enable<PowerupUI>();
                    StartCoroutine(PerformAfterDelay(0.1f, playerUIManager.Enable<UpgradeUI>));
                }
            }
        }));
    }

    private void EndPVP()
    {
        System.Guid maxKey;
        int maxValue = 0;
        foreach (System.Guid key in playerScores.Keys)
        {
            if (playerScores[key] > maxValue)
            {
                maxValue = playerScores[key];
                maxKey = key;
            }
        }

        int i = 1;
        string winnerText = "";
        foreach (GameObject playerGO in playerGOs)
        {
            Player player = playerGO.GetComponent<Player>();

            if (player.EntityID == maxKey)
            {
                winnerText = $"Player {i}";
            }

            i++;
        }

        SharedUIManager.Instance.Enable<PVPEndMatchUI, PVPEndMatchUIData>(new PVPEndMatchUIData(winnerText));
    }

    public void Restart()
    {
        // Reset matches
        currentMatch = 0;

        // Reset scores
        playerScores.Clear();

        // Reset Players
        foreach (GameObject playerGO in playerGOs)
        {
            Player player = playerGO.GetComponent<Player>();
            player.Reset();
        }

        SharedUIManager.Instance.Disable<PVPEndMatchUI>();

        StartMatch();
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
}
