using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;

public class PVPBattleManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> playerGOs = new List<GameObject>();
    [SerializeField] private List<System.Guid> alivePlayers = new List<System.Guid>();
     private Dictionary<System.Guid,int> playerScores = new Dictionary<System.Guid, int>();

    [SerializeField] private List<Vector3> levels;
    [SerializeField] private List<ListWrapper> localPlayerSpawnPositions;
    [SerializeField] private List<ListWrapper> localEnemySpawnPositions;

    [SerializeField] private int matches = 3;
    [SerializeField] private int currentMatch = 0;
    [SerializeField] private int matchCooldown = 5;

    [SerializeField] private GameObject endMatchUI;
    [SerializeField] private Text winnerText;
    [SerializeField] private GameObject playerNamePrefab;

    [SerializeField] private PlayerStats pvpBaseStats;


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

            // Link player canvas to Camera
            player.GetComponentInChildren<Canvas>().worldCamera = Camera.main;

            // Link Match end to player death
            player.onDeath.AddListener(() => PlayerDied(playerGO));

            // Give name
            GameObject playerNameGO = Instantiate(playerNamePrefab, playerGO.transform);
            TextMeshPro playerNameText = playerNameGO.GetComponent<TextMeshPro>();
            playerNameText.text = $"Player {i}";

            i++;
        }

        // Link Event System
        GameObject eventSystemGO = GameObject.Find("EventSystem");
        MultiplayerEventSystem multiplayerEventSystem = eventSystemGO.GetComponent<MultiplayerEventSystem>();
        GameObject canvasGO = GameObject.Find("Canvas");
        multiplayerEventSystem.playerRoot = canvasGO;

        // UI Setup
        endMatchUI.SetActive(false);
        Player player1 = players[0];
        GameObject playerUIGO1 = player1.GetComponentInChildren<PlayerUIManager>().transform.Find("UIParent").gameObject;
        RectTransform transform1 = playerUIGO1.GetComponent<RectTransform>();
        transform1.localScale = new Vector3(0.75f, 0.75f, 1);
        transform1.localPosition = new Vector3(-500, 0, 0);

        Player player2 = players[1];
        GameObject playerUIGO2 = player2.GetComponentInChildren<PlayerUIManager>().transform.Find("UIParent").gameObject;
        RectTransform transform2 = playerUIGO2.GetComponent<RectTransform>();
        transform2.localScale = new Vector3(0.75f, 0.75f, 1);
        transform2.localPosition = new Vector3(500, 0, 0);

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
            playerGO.transform.Find("Sprite").gameObject.SetActive(true);
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
        playerGO.transform.Find("Sprite").gameObject.SetActive(false);

        if (alivePlayers.Count <= 1) MatchEnd(alivePlayers[0]);
    }

    private void MatchEnd(System.Guid winnerID)
    {
        if (alivePlayers.Count > 1) return;

        Debug.Log("Match Done");

        if (playerScores.ContainsKey(winnerID)) playerScores[winnerID] += 1;
        else playerScores.Add(winnerID, 1);

        currentMatch += 1;
        ColorUtility.TryParseHtmlString("#E6E6E6", out Color newColor);
        GeneralUIManager.Instance.PerformCountdown("Next match starts in: ", newColor, matchCooldown);

        StartCoroutine(PerformAfterDelay(matchCooldown, () =>
        {
            GeneralUIManager.Instance.DisableWaveUI();

            if (currentMatch >= matches) EndPVP();
            else
            {
                StartMatch();

                foreach (GameObject playerGO in playerGOs)
                {
                    PlayerUIManager playerUIManager = playerGO.GetComponentInChildren<PlayerUIManager>();
                    playerUIManager.EnablePowerupUI();
                    StartCoroutine(PerformAfterDelay(0.1f, playerUIManager.EnableUpgradeUI));
                }
            }
        }));
    }

    private void EndPVP()
    {
        UIVisibilityManager.Instance.RegisterUIShown();
        endMatchUI.SetActive(true);

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
        foreach (GameObject playerGO in playerGOs)
        {
            Player player = playerGO.GetComponent<Player>();

            if (player.EntityID == maxKey)
            {
                winnerText.text = $"Player {i}";
            }

            i++;
        }
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

        endMatchUI.SetActive(false);
        UIVisibilityManager.Instance.RegisterUIHidden();

        StartMatch();
    }

    // Update is called once per frame
    void Update()
    {
        
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
