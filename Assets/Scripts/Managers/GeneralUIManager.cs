using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class GeneralUIManager : MonoBehaviour
{
    public static GeneralUIManager Instance { get; private set; }

    [SerializeField] private Canvas canvas;

    private GameObject pauseUI;
    private GameObject scoreUI;

    private GameObject waveUI;
    private Text waveCountdownText;
    private Text waveText;

    private GameObject gameOverUI;
    private GameObject winUI;

    [SerializeField] private Player player;
    private PlayerInput playerInput;
    private MultiplayerEventSystem playerEventSystem;
    private string currentControlScheme;

    public GameObject FirstSelected { get; private set; }


    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput>();
            playerEventSystem = player.GetComponentInChildren<MultiplayerEventSystem>();
        }
        LinkToPlayers();

        pauseUI = canvas.transform.Find("PauseUI").gameObject;
        pauseUI.GetComponent<UIElement>().onDisable.AddListener(() =>
        {
            UIVisibilityManager.Instance.RegisterUIHidden();
            RemoveFirstSelected();
        });
        scoreUI = canvas.transform.Find("ScoreUI").gameObject;

        waveUI = canvas.transform.Find("WaveUI").gameObject;
        waveCountdownText = waveUI.transform.Find("Countdown").GetComponent<Text>();
        waveText = waveUI.transform.Find("Title").GetComponent<Text>();
        waveUI.SetActive(false);

        gameOverUI = canvas.transform.Find("GameOverUI").gameObject;
        gameOverUI.SetActive(false);

        winUI = canvas.transform.Find("WinUI").gameObject;
        winUI.SetActive(false);
    }

    private void Update()
    {
        if (player != null && playerInput.currentControlScheme != currentControlScheme)
        {
            currentControlScheme = playerInput.currentControlScheme;
            OnControlsChanged();
        }
    }

    private void OnControlsChanged()
    {
        Debug.Log($"Controls Changed to {playerInput.currentControlScheme}");
        if (Gamepad.current != null && Gamepad.current.enabled && playerInput.currentControlScheme == "Gamepad")
        {
            Debug.Log($"Setting first selected: {FirstSelected}");
            playerEventSystem.SetSelectedGameObject(FirstSelected);
        }
        else
        {
            playerEventSystem.SetSelectedGameObject(null);
        }
    }

    private void LinkToPlayers()
    {
        PlayerController[] playerControllers = FindObjectsOfType<PlayerController>();
        foreach (PlayerController playerController in playerControllers)
        {
            playerController.onPause.AddListener(TogglePauseUI);
        }
    }

    public void EnableAllPowerupUI()
    {
        PlayerUIManager[] playerUIManagers = FindObjectsOfType<PlayerUIManager>();
        foreach (PlayerUIManager playerUIManager in playerUIManagers)
        {
            playerUIManager.EnablePowerupUI();
        }
    }

    public void EnableAllUpgradeUI()
    {
        PlayerUIManager[] playerUIManagers = FindObjectsOfType<PlayerUIManager>();
        foreach (PlayerUIManager playerUIManager in playerUIManagers)
        {
            playerUIManager.EnableUpgradeUI();
        }
    }

    public void EnablePauseUI()
    {
        UIElement pauseUIElement = this.pauseUI.GetComponent<UIElement>();
        EnableUI(pauseUIElement);
    }

    public void DisablePauseUI()
    {
        UIElement pauseUIElement = this.pauseUI.GetComponent<UIElement>();
        DisableUI(pauseUIElement);
    }

    public void TogglePauseUI()
    {
        UIElement pauseUIElement = this.pauseUI.GetComponent<UIElement>();
        if (pauseUIElement.Enabled())
        {
            DisablePauseUI();
        }
        else
        {
            EnablePauseUI();
        }
    }

    public void DisableUI(UIElement uiElement)
    {
        uiElement.Disable();
    }

    public void EnableUI(UIElement uiElement)
    {
        UIVisibilityManager.Instance.RegisterUIShown();

        // First enable then set first selected as first selected object might be created in enable call.
        uiElement.Enable();
        SetFirstSelectedIfGamepad(uiElement.GetFirstSelected());
    }

    public void PerformWaveCountdown(int countdown, bool boss)
    {
        if (waveUI.activeSelf) return;

        if (boss)
        {
            ColorUtility.TryParseHtmlString("#FA5C5C", out Color newColor);
            waveText.color = newColor;
            waveText.text = "BOSS wave starts in: ";
        }
        else
        {
            ColorUtility.TryParseHtmlString("#E6E6E6", out Color newColor);
            waveText.color = newColor;
            waveText.text = "Next wave starts in: ";
        }

        waveUI.SetActive(true);
        waveCountdownText.text = countdown.ToString();

        StartCoroutine(ReduceCountEverySecond(waveCountdownText));
    }

    public void PerformCountdown(string text, Color color, int countdown)
    {
        if (waveUI.activeSelf) return;

        waveText.color = color;
        waveText.text = text;

        waveUI.SetActive(true);
        waveCountdownText.text = countdown.ToString();

        StartCoroutine(ReduceCountEverySecond(waveCountdownText));
    }

    public void EnableLevelCompletedText(int room)
    {
        if (waveUI.activeSelf) return;

        waveUI.SetActive(true);
        waveText.text = "You beat level " + room + "!";
    }

    public void DisableWaveUI()
    {
        waveCountdownText.text = "";
        waveText.text = "";
        ColorUtility.TryParseHtmlString("#FFFFFF", out Color newColor);
        waveText.color = newColor;

        waveUI.SetActive(false);
    }

    private IEnumerator ReduceCountEverySecond(Text text)
    {
        yield return new WaitForSeconds(1);
        if (text.text != "")
        {
            int cooldown = int.Parse(text.text);
            if (cooldown > 0)
            {
                text.text = (cooldown - 1).ToString();
                StartCoroutine(ReduceCountEverySecond(text));
            }
        }
    }

    private void SetFirstSelectedIfGamepad(GameObject obj)
    {
        FirstSelected = obj;

        // Only select first if player is using a gamepad
        if (Gamepad.current != null && Gamepad.current.enabled && playerInput.currentControlScheme == "Gamepad") EventSystem.current.SetSelectedGameObject(obj);
    }

    private void RemoveFirstSelected()
    {
        FirstSelected = null;
        EventSystem.current.SetSelectedGameObject(null);
    }
    
    public void EnableWinUI(bool beatBestTime, float bestTime, bool beatHighScore, float highScore)
    {
        UIVisibilityManager.Instance.RegisterUIShown();

        this.winUI.SetActive(true);
        GameObject timeGameObject = this.winUI.transform.Find("Scores").Find("Time").gameObject;
        timeGameObject.SetActive(false);
        GameObject scoreGameObject = this.winUI.transform.Find("Scores").Find("Score").gameObject;
        scoreGameObject.SetActive(false);
        GameObject newBestTimeGameObject = this.winUI.transform.Find("Scores").Find("NewBestTime").gameObject;
        newBestTimeGameObject.SetActive(false);
        GameObject newHighScoreGameObject = this.winUI.transform.Find("Scores").Find("NewHighScore").gameObject;
        newHighScoreGameObject.SetActive(false);

        StartCoroutine(PerformAfterRealDelay(1, () =>
        {
            timeGameObject.SetActive(true);

            Text timeText = timeGameObject.GetComponent<Text>();
            TimeSpan time = TimeSpan.FromSeconds(bestTime);
            timeText.text = $"Time: {time:hh\\:mm\\:ss}";

            if (beatBestTime)
            {
                newBestTimeGameObject.gameObject.SetActive(true);
            }
        }));

        StartCoroutine(PerformAfterRealDelay(2, () =>
        {
            scoreGameObject.SetActive(true);

            Text scoreText = scoreGameObject.GetComponent<Text>();
            scoreText.text = $"Score: {highScore}";

            if (beatHighScore)
            {
                newHighScoreGameObject.SetActive(true);
            }
        }));

        SetFirstSelectedIfGamepad(winUI.GetComponentsInChildren<Button>()[0].gameObject);
    }

    public void EnableGameOverUI(bool beatHighScore, float highScore)
    {
        UIVisibilityManager.Instance.RegisterUIShown();

        this.gameOverUI.SetActive(true);
        GameObject scoreGameObject = this.gameOverUI.transform.Find("Scores").Find("Score").gameObject;
        scoreGameObject.SetActive(false);
        GameObject newHighScoreGameObject = this.gameOverUI.transform.Find("Scores").Find("NewHighScore").gameObject;
        newHighScoreGameObject.SetActive(false);

        StartCoroutine(PerformAfterRealDelay(1, () =>
        {
            scoreGameObject.SetActive(true);

            Text scoreText = scoreGameObject.GetComponent<Text>();
            scoreText.text = $"Score: {highScore}";

            if (beatHighScore)
            {
                newHighScoreGameObject.SetActive(true);
            }
        }));

        SetFirstSelectedIfGamepad(winUI.GetComponentsInChildren<Button>()[0].gameObject);
    }
    
    private IEnumerator PerformAfterRealDelay(float delay, Action action)
    {
        yield return new WaitForSecondsRealtime(delay);

        action();
    }

    public void EnableScoreUI(bool enable)
    {
        scoreUI.SetActive(enable);
    }
}
