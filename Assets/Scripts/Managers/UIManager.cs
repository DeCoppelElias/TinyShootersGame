using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private GameStateManager gameStateManager;

    private GameObject canvas;
    private GameObject pauseUI;

    private GameObject upgradeUI;
    [SerializeField] private GameObject upgradeButtonPrefab;

    private GameObject abilityUI;
    private GameObject dashAbilityUI;
    private bool dashAbilityEnabled = true;
    private GameObject reflectAbilityUI;
    private bool reflectAbilityEnabled = true;
    private GameObject classAbilityUI;
    private bool classAbilityEnabled = true;
    private bool classAbilityInitialised = false;

    private GameObject waveUI;
    private Text waveCountdownText;
    private Text waveText;

    private GameObject gameOverUI;
    private GameObject winUI;

    private EventSystem eventSystem;
    private PlayerInput playerInput;

    private GameObject firstSelected = null;
    private string currentControlScheme;

    private Player player;

    // Start is called before the first frame update
    void Start()
    {
        gameStateManager = GameObject.Find("GameStateManager").GetComponent<GameStateManager>();
        canvas = GameObject.Find("Canvas");

        upgradeUI = GameObject.Find("UpgradeUI");
        upgradeUI.SetActive(false);

        pauseUI = GameObject.Find("PauseUI");
        pauseUI.SetActive(false);

        abilityUI = GameObject.Find("AbilityUI");
        dashAbilityUI = GameObject.Find("DashAbility");
        reflectAbilityUI = GameObject.Find("ReflectAbility");
        classAbilityUI = GameObject.Find("ClassAbility");
        if (!classAbilityInitialised) classAbilityUI.SetActive(false);

        waveUI = GameObject.Find("WaveUI");
        waveCountdownText = waveUI.transform.Find("Countdown").GetComponent<Text>();
        waveText = waveUI.transform.Find("Title").GetComponent<Text>();
        waveUI.SetActive(false);

        gameOverUI = GameObject.Find("GameOverUI");
        gameOverUI.SetActive(false);

        winUI = GameObject.Find("WinUI");
        winUI.SetActive(false);

        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        playerInput = GameObject.Find("Player").GetComponent<PlayerInput>();

        player = GameObject.Find("Player").GetComponent<Player>();
    }

    public void Pause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TogglePauseUI();
        }
    }

    private void Update()
    {
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();

        if (playerInput.currentControlScheme != currentControlScheme)
        {
            currentControlScheme = playerInput.currentControlScheme;
            OnControlsChanged();
        }
    }

    public void EnablePauseUI()
    {
        if (upgradeUI.activeSelf) return;

        LowerMusicVolume();

        gameStateManager.ToPaused();
        pauseUI.SetActive(true);
        SetFirstSelectedIfGamepad(pauseUI.transform.Find("ResumeButton").gameObject);
    }

    public void DisablePauseUI()
    {
        ReturnMusicVolume();

        gameStateManager.ToRunning();
        pauseUI.SetActive(false);
        RemoveFirstSelected();
    }

    public void TogglePauseUI()
    {
        if (pauseUI.activeSelf)
        {
            DisablePauseUI();
        }
        else
        {
            EnablePauseUI();
        }
    }

    public void EnableUpgradeUI()
    {
        LowerMusicVolume();

        Class playerClass = player.playerClass;
        if (playerClass.upgrades.Count == 0) return;

        gameStateManager.ToPaused();

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
            Class currentPlayerClass = playerClass.upgrades[i];

            Text text = buttonTransform.GetComponentInChildren<Text>();
            text.text = currentPlayerClass.className;

            Image image = buttonTransform.Find("Sprite").GetComponent<Image>();
            image.sprite = currentPlayerClass.blueSprite;

            Button button = buttonTransform.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { 
                player.ApplyClass(currentPlayerClass);
                DisableUpgradeUI();
            });
        }

        upgradeUI.SetActive(true);
        SetFirstSelectedIfGamepad(buttons.GetChild(0).gameObject);
    }

    public void DisableUpgradeUI()
    {
        ReturnMusicVolume();

        gameStateManager.ToRunning();
        upgradeUI.SetActive(false);
        RemoveFirstSelected();
    }

    public void DisableDashAbility()
    {
        if (!dashAbilityEnabled) return;
        dashAbilityEnabled = false;

        DashAbility dashAbility = player.GetComponent<DashAbility>();
        int cooldown = dashAbility.GetDashCooldown();

        Image image = dashAbilityUI.GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.1f);

        Text text = dashAbilityUI.GetComponentInChildren<Text>();
        text.text = cooldown.ToString();

        StartCoroutine(ReduceCountEverySecond(text));
    }

    public void EnableDashAbility()
    {
        dashAbilityEnabled = true;

        Image image = dashAbilityUI.GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.7f);

        Text text = dashAbilityUI.GetComponentInChildren<Text>();
        text.text = "";
    }

    public void DisableReflectAbility()
    {
        if (!reflectAbilityEnabled) return;
        reflectAbilityEnabled = false;

        ReflectShieldAbility reflectAbility = player.GetComponent<ReflectShieldAbility>();
        int cooldown = reflectAbility.GetReflectShieldCooldown();

        Image image = reflectAbilityUI.GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.1f);

        Text text = reflectAbilityUI.GetComponentInChildren<Text>();
        text.text = cooldown.ToString();

        StartCoroutine(ReduceCountEverySecond(text));
    }

    public void EnableReflectAbility()
    {
        reflectAbilityEnabled = true;

        Image image = reflectAbilityUI.GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.7f);

        Text text = reflectAbilityUI.GetComponentInChildren<Text>();
        text.text = "";
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
            ColorUtility.TryParseHtmlString("#FFFFFF", out Color newColor);
            waveText.color = newColor;
            waveText.text = "Next wave starts in: ";
        }

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
        firstSelected = obj;

        // Only select first if player is using a gamepad
        if (Gamepad.current != null && Gamepad.current.enabled && playerInput.currentControlScheme == "Gamepad") eventSystem.SetSelectedGameObject(obj);
    }

    private void RemoveFirstSelected()
    {
        firstSelected = null;
        eventSystem.SetSelectedGameObject(null);
    }

    public void OnControlsChanged()
    {
        if (Gamepad.current != null && Gamepad.current.enabled && playerInput.currentControlScheme == "Gamepad")
        {
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void SetClassAbilityUI(AbilityBehaviour abilityBehaviour)
    {
        if (classAbilityUI == null) classAbilityUI = GameObject.Find("ClassAbility");
        classAbilityUI.SetActive(true);
        classAbilityInitialised = true;
    }

    public void DisableClassAbility(AbilityBehaviour abilityBehaviour)
    {
        if (!classAbilityUI.activeSelf) return;
        if (!classAbilityEnabled) return;
        classAbilityEnabled = false;

        int cooldown = abilityBehaviour.ability.cooldown;

        Image image = classAbilityUI.GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.1f);

        Text text = classAbilityUI.GetComponentInChildren<Text>();
        text.text = cooldown.ToString();

        StartCoroutine(ReduceCountEverySecond(text));
    }

    public void EnableClassAbility(AbilityBehaviour abilityBehaviour)
    {
        if (!classAbilityUI.activeSelf) return;
        classAbilityEnabled = true;

        Image image = classAbilityUI.GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.7f);

        Text text = classAbilityUI.GetComponentInChildren<Text>();
        text.text = "";
    }
    
    public void EnableWinUI(bool beatBestTime, float bestTime, bool beatHighScore, float highScore)
    {
        LowerMusicVolume();

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
        LowerMusicVolume();

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

    private void LowerMusicVolume()
    {
        GameObject.Find("AudioManager").transform.Find("MusicAudioSource").GetComponent<AudioSource>().volume *= 0.5f;
    }

    private void ReturnMusicVolume()
    {
        GameObject.Find("AudioManager").transform.Find("MusicAudioSource").GetComponent<AudioSource>().volume *= 2;
    }
}
