using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PVPManager : MonoBehaviour
{
    private Player player1;
    private PlayerInput playerInput1;
    private Player player2;
    private PlayerInput playerInput2;

    private Canvas canvas1;
    private GameObject pauseUI1;
    private GameObject upgradeUI1;

    private Canvas canvas2;
    private GameObject pauseUI2;
    private GameObject upgradeUI2;

    private Canvas sharedCanvas;
    private GameObject notificationUI;
    private Text notificationText;
    private GameObject scoreUI;
    private Text scoreText;
    private GameObject debugUI;
    private Text debugText;

    private bool deviceLost = false;

    public GameObject upgradeButtonPrefab;

    private enum PVPState { Initialising, PVP, CountDown, Upgrading, Done}
    [SerializeField]
    private PVPState pvpState = PVPState.Initialising;

    private int currentRound = 1;
    [SerializeField]
    private int rounds = 2;
    private bool upgradesBetweenRounds = true;

    private float roundStart = 0;
    [SerializeField]
    private float roundDuration = 30;
    private float lastSpawn = 0;
    private float spawnCooldown = 20;
    private float currentSpawnCooldown = 20;
    private int spawnAmount = 2;
    private int currentSpawnAmount = 2;
    public List<GameObject> enemyPrefabs = new List<GameObject>();
    public List<Vector3> spawnPositions = new List<Vector3>();

    private int player1Score = 0;
    private int player2Score = 0;

    public Tilemap warningTilemap;
    public Tile warningTile;


    // Start is called before the first frame update
    void Start()
    {
        player1 = GameObject.Find("Player 1").GetComponent<Player>();
        playerInput1 = player1.GetComponent<PlayerInput>();
        player2 = GameObject.Find("Player 2").GetComponent<Player>();
        playerInput2 = player2.GetComponent<PlayerInput>();

        canvas1 = GameObject.Find("Canvas 1").GetComponent<Canvas>();
        pauseUI1 = canvas1.transform.Find("PauseUI").gameObject;
        pauseUI1.SetActive(false);
        upgradeUI1 = canvas1.transform.Find("UpgradeUI").gameObject;
        upgradeUI1.SetActive(false);

        canvas2 = GameObject.Find("Canvas 2").GetComponent<Canvas>();
        pauseUI2 = canvas2.transform.Find("PauseUI").gameObject;
        pauseUI2.SetActive(false);
        upgradeUI2 = canvas2.transform.Find("UpgradeUI").gameObject;
        upgradeUI2.SetActive(false);

        sharedCanvas = GameObject.Find("SharedCanvas").GetComponent<Canvas>();
        notificationUI = sharedCanvas.transform.Find("NotificationUI").gameObject;
        notificationText = notificationUI.transform.Find("Title").GetComponent<Text>();
        scoreUI = sharedCanvas.transform.Find("ScoreUI").gameObject;
        scoreText = scoreUI.transform.Find("Title").GetComponent<Text>();
        debugUI = sharedCanvas.transform.Find("DebugUI").gameObject;
        debugText = debugUI.transform.Find("Title").GetComponent<Text>();

        InputSystem.onDeviceChange += OnDeviceChange;

        Initialise();
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad)
        {
            if (change == InputDeviceChange.Added)
            {
                // Assign the newly connected gamepad to a player without a device
                if (playerInput1.devices.Count == 0)
                {
                    DisplayDebugMessage("Gamepad connected to player 1!", 2);
                    playerInput1.SwitchCurrentControlScheme(device);
                }
                else if (playerInput2.devices.Count == 0)
                {
                    DisplayDebugMessage("Gamepad connected to player 2!", 2);
                    playerInput2.SwitchCurrentControlScheme(device);
                }
            }
            else if (change == InputDeviceChange.Removed)
            {
                deviceLost = true;
                DisplayDebugMessage("Gamepad disconnected!", -1);

                if (pvpState == PVPState.PVP || pvpState == PVPState.CountDown)
                {
                    EnablePlayerMovement(false);
                    GameStateManager.Instance.ToPaused();
                }
            }
        }
    }

    private void Initialise()
    {
        pvpState = PVPState.Initialising;
        EnablePlayerMovement(false);
        DisplayNotification("Welcome to PVP, please plug in a controller in order to play together.", -1);
    }

    // Update is called once per frame
    void Update()
    {
        if (pvpState == PVPState.Initialising && playerInput1.devices.Count > 0 && playerInput2.devices.Count > 0)
        {
            if (pvpState == PVPState.Initialising)
            {
                StartRound();
            }
        }
        else if (deviceLost && playerInput1.devices.Count > 0 && playerInput2.devices.Count > 0)
        {
            deviceLost = false;
            DisplayDebugMessage("Gamepad reconnected!", 2);

            if (pvpState == PVPState.PVP || pvpState == PVPState.CountDown)
            {
                EnablePlayerMovement(true);
                GameStateManager.Instance.ToRunning();
            }
        }

        if (pvpState == PVPState.PVP && Time.time - roundStart > roundDuration && Time.time - lastSpawn > currentSpawnCooldown)
        {
            SpawnEnemies();
        }
    }

    private void SpawnEnemies()
    {
        lastSpawn = Time.time;
        DisplayNotification("Spawning Enemies!", 2);

        List<Vector3> currentSpawnPositions = this.spawnPositions.OrderBy(x => UnityEngine.Random.Range(0, this.spawnPositions.Count)).Take(currentSpawnAmount).ToList();
        foreach (Vector3 position in currentSpawnPositions)
        {
            int randomIndex = UnityEngine.Random.Range(0, this.enemyPrefabs.Count);
            GameObject prefab = this.enemyPrefabs[randomIndex];
            CreateEnemy(prefab, position);
        }

        if (currentSpawnAmount < this.spawnPositions.Count)
        {
            currentSpawnAmount += 1;
        }
        else if (currentSpawnCooldown > 1)
        {
            currentSpawnCooldown -= 1;
        }
    }

    private void CreateEnemy(GameObject prefab, Vector3 spawnLocation)
    {
        warningTilemap.SetTile(Vector3Int.FloorToInt(spawnLocation), warningTile);
        StartCoroutine(CreateEnemyAfterDelay(prefab, spawnLocation, 3));
    }

    private IEnumerator CreateEnemyAfterDelay(GameObject prefab, Vector3 spawnLocation, int delay)
    {
        yield return new WaitForSeconds(delay);

        if (pvpState == PVPState.PVP)
        {
            GameObject enemy = Instantiate(prefab, spawnLocation, Quaternion.identity);
            enemy.transform.SetParent(GameObject.Find("Enemies").transform);
        }

        warningTilemap.SetTile(Vector3Int.FloorToInt(spawnLocation), null);
    }

    public void PlayerDied(Player player)
    {
        // Adding score from previous round
        if (player == player1)
        {
            player2Score += 1;
        }
        else
        {
            player1Score += 1;
        }
        scoreText.text = player1Score + " - " + player2Score;

        if (currentRound == rounds)
        {
            EndPVP();
        }
        else
        {
            currentRound += 1;

            if (upgradesBetweenRounds)
            {
                ResetArena();
                UpgradePlayers();
            }
            else StartRound();
        }
    }

    private void UpgradePlayers()
    {
        pvpState = PVPState.Upgrading;
        DisplayNotification("Upgrade!", -1);

        EnablePlayerMovement(false);

        EnableUpgradeUI(player1);
        EnableUpgradeUI(player2);
    }

    public void EnableUpgradeUI(Player player)
    {
        PlayerClass playerClass = player.playerClass;
        if (playerClass.upgrades.Count == 0) return;

        (GameObject pauseUI, GameObject upgradeUI) = GetUI(player);
        Transform buttons = upgradeUI.transform.Find("Buttons");

        // First make sure that the amount of buttons and upgrades are the same
        if (buttons.childCount > playerClass.upgrades.Count)
        {
            for (int i = playerClass.upgrades.Count; i < buttons.childCount; i++)
            {
                GameObject child = buttons.GetChild(i).gameObject;
                child.transform.SetParent(null);
                if (child != null) Destroy(child);
            }
        }
        else
        {
            for (int i = buttons.childCount; i < playerClass.upgrades.Count; i++)
            {
                Instantiate(upgradeButtonPrefab, buttons);
            }
        }

        // Link each button to upgrading the player to that class
        for (int i = 0; i < buttons.childCount; i++)
        {
            Transform buttonTransform = buttons.GetChild(i);
            PlayerClass currentPlayerClass = playerClass.upgrades[i];

            Text text = buttonTransform.GetComponentInChildren<Text>();
            text.text = currentPlayerClass.className;

            Image image = buttonTransform.Find("Sprite").GetComponent<Image>();
            image.sprite = currentPlayerClass.UISprite;

            Button button = buttonTransform.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                player.ApplyClass(currentPlayerClass);
                DisableUpgradeUI(player);
            });
        }

        upgradeUI.SetActive(true);

        // Set first selected if gamepad
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput.currentControlScheme == "Gamepad")
        {
            MultiplayerEventSystem multiplayerEventSystem = playerInput.uiInputModule.GetComponent<MultiplayerEventSystem>();
            multiplayerEventSystem.SetSelectedGameObject(buttons.GetChild(0).gameObject);
        }
    }

    public void DisableUpgradeUI(Player player)
    {
        (GameObject pauseUI, GameObject upgradeUI) = GetUI(player);
        upgradeUI.SetActive(false);

        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput.currentControlScheme == "Gamepad")
        {
            MultiplayerEventSystem multiplayerEventSystem = playerInput.uiInputModule.GetComponent<MultiplayerEventSystem>();
            multiplayerEventSystem.SetSelectedGameObject(null);
        }

        if (upgradeUI1.activeSelf == false && upgradeUI2.activeSelf == false) StartRound();
    }

    private void ResetArena()
    {
        // Reset player position
        player1.transform.position = new Vector3(-5, 0, 0);
        player2.transform.position = new Vector3(5, 0, 0);

        // Reset player speed
        player1.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
        player2.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);

        // Reset player health
        player1.Health = player1.MaxHealth;
        player2.Health = player2.MaxHealth;

        // Clean up all enemies
        Transform enemiesParent = GameObject.Find("Enemies").transform;
        foreach (Transform child in enemiesParent)
        {
            Destroy(child.gameObject);
        }

        // Clean up all bullets
        Transform bulletsParent = GameObject.Find("Bullets").transform;
        foreach (Transform child in bulletsParent)
        {
            Destroy(child.gameObject);
        }

        // Reset enemy spawning parameters
        this.currentSpawnCooldown = spawnCooldown;
        this.currentSpawnAmount = spawnAmount;
    }

    private void StartRound()
    {
        EnablePlayerMovement(false);
        ResetArena();
        CountDownToAction(5, "Next round will start in ", () =>
        {
            DisplayNotification("Start!", 2);
            pvpState = PVPState.PVP;
            roundStart = Time.time;
            EnablePlayerMovement(true);
        });
    }

    private void EndPVP()
    {
        pvpState = PVPState.Done;
        EnablePlayerMovement(false);

        ResetArena();

        if (player1Score < player2Score)
        {
            player2.transform.position = new Vector3(0, 0, 0);
            Destroy(player1.gameObject);
            DisplayNotification("Player 2 won! Quitting to main menu...", 5);
        }
        else if (player1Score > player2Score)
        {
            player1.transform.position = new Vector3(0, 0, 0);
            Destroy(player2.gameObject);
            DisplayNotification("Player 1 won! Quitting to main menu...", 5);
        }
        else
        {
            player1.transform.position = new Vector3(-1, 0, 0);
            player2.transform.position = new Vector3(1, 0, 0);
            DisplayNotification("It's a tie! Quitting to main menu...", 5);
        }
        StartCoroutine(PerformAfterDelay(() => GameStateManager.Instance.QuitToMainMenu(), 5));
    }

    public void EnablePauseUI(Player player)
    {
        if (upgradeUI1.activeSelf || upgradeUI2.activeSelf) return;

        LowerMusicVolume();

        GameStateManager.Instance.ToPaused();

        (GameObject pauseUI, GameObject upgradeUI) = GetUI(player);
        pauseUI.SetActive(true);

        // Disable controls of other player
        PlayerInput otherPlayerInput = GetOtherInput(player);
        otherPlayerInput.enabled = false;

        // Set first selected if gamepad
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput.currentControlScheme == "Gamepad")
        {
            MultiplayerEventSystem multiplayerEventSystem = playerInput.uiInputModule.GetComponent<MultiplayerEventSystem>();
            multiplayerEventSystem.SetSelectedGameObject(pauseUI.transform.Find("ResumeButton").gameObject);
        }
    }

    public void DisablePauseUI(Player player)
    {
        ReturnMusicVolume();

        (GameObject pauseUI, GameObject upgradeUI) = GetUI(player);
        pauseUI.SetActive(false);
        GameStateManager.Instance.ToRunning();

        // Enable controls of other player
        PlayerInput otherPlayerInput = GetOtherInput(player);
        otherPlayerInput.enabled = true;

        // Set first selected if gamepad
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput.currentControlScheme == "Gamepad")
        {
            MultiplayerEventSystem multiplayerEventSystem = playerInput.uiInputModule.GetComponent<MultiplayerEventSystem>();
            multiplayerEventSystem.SetSelectedGameObject(null);
        }
    }

    public void TogglePauseUI(Player player)
    {
        (GameObject pauseUI, GameObject upgradeUI) = GetUI(player);
        if (pauseUI.activeSelf)
        {
            DisablePauseUI(player);
        }
        else
        {
            EnablePauseUI(player);
        }
    }

    public PlayerInput GetOtherInput(Player player)
    {
        if (player == player1)
        {
            return player2.GetComponent<PlayerInput>();
        }
        else
        {
            return player1.GetComponent<PlayerInput>();
        }
    }

    private (GameObject pauseUI, GameObject upgradeUI) GetUI(Player player)
    {
        if (player == player1)
        {
            return (pauseUI1, upgradeUI1);
        }
        else
        {
            return (pauseUI2, upgradeUI2);
        }
    }

    public void DisplayNotification(string notification, float removeAfter)
    {
        notificationText.text = notification;

        if (removeAfter == -1) return;

        StartCoroutine(RemoveTextAfter(notificationText, removeAfter));
    }

    public void DisplayDebugMessage(string message, float removeAfter)
    {
        debugText.text = message;

        if (removeAfter == -1) return;

        StartCoroutine(RemoveTextAfter(debugText, removeAfter));
    }

    private void CountDownToAction(int seconds, string pretext, Action action)
    {
        pvpState = PVPState.CountDown;
        DisplayNotification(pretext + seconds + ".", -1);
        StartCoroutine(CountDownToActionCoroutine(seconds, pretext, action));
    }

    private IEnumerator CountDownToActionCoroutine(int seconds, string pretext, Action action)
    {
        yield return new WaitForSeconds(1);

        if (pvpState == PVPState.CountDown)
        {
            seconds -= 1;
            if (seconds> 0)
            {
                DisplayNotification(pretext + seconds + ".", -1);
                StartCoroutine(CountDownToActionCoroutine(seconds, pretext, action));
            }
            else
            {
                action();
            }
        }
    }

    private IEnumerator RemoveTextAfter(Text text, float seconds)
    {
        yield return new WaitForSeconds(seconds);

        text.text = "";
    }

    private IEnumerator PerformAfterDelay(Action action, float seconds)
    {
        yield return new WaitForSeconds(seconds);

        action();
    }

    private void EnablePlayerMovement(bool enable)
    {
        if (player1 != null)
        {
            if (!enable) player1.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
            player1.GetComponent<PlayerController>().collectInput = enable;
            player1.GetComponent<PlayerMovement>().SetMoveDirection(Vector3.zero);
        }
        if (player2 != null)
        {
            if (!enable) player2.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
            player2.GetComponent<PlayerController>().collectInput = enable;
            player2.GetComponent<PlayerMovement>().SetMoveDirection(Vector3.zero);
        }
    }

    private void LowerMusicVolume()
    {
        GameObject.Find("AudioManager").transform.Find("MusicAudioSource").GetComponent<AudioSource>().volume *= 0.5f;
    }

    private void ReturnMusicVolume()
    {
        GameObject.Find("AudioManager").transform.Find("MusicAudioSource").GetComponent<AudioSource>().volume *= 2;
    }
}
