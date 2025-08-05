using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialUIManager : MonoBehaviour
{
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

    private GameObject scoreUI;

    private GameObject firstSelected = null;
    private string currentControlScheme;

    private Player player;
    private PlayerInput playerInput;

    // Start is called before the first frame update
    void Start()
    {
        pauseUI = GameObject.Find("PauseUI");
        pauseUI.SetActive(false);

        upgradeUI = GameObject.Find("UpgradeUI");
        upgradeUI.SetActive(false);

        abilityUI = GameObject.Find("AbilityUI");
        dashAbilityUI = GameObject.Find("DashAbility");
        reflectAbilityUI = GameObject.Find("ReflectAbility");
        classAbilityUI = GameObject.Find("ClassAbility");
        if (!classAbilityInitialised) classAbilityUI.SetActive(false);
        abilityUI.SetActive(false);

        scoreUI = GameObject.Find("ScoreUI");
        scoreUI.SetActive(false);

        player = GameObject.Find("Player").GetComponent<Player>();
        playerInput = player.GetComponent<PlayerInput>();
    }

    public void EnableAbilityUI()
    {
        abilityUI.SetActive(true);
    }

    public void EnableScoreUI()
    {
        scoreUI.SetActive(true);
    }

    public void EnableUpgradeUI()
    {
        LowerMusicVolume();

        PlayerClass playerClass = player.playerClass;
        if (playerClass.upgrades.Count == 0) return;

        GameStateManager.Instance.ToPaused();

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
                DisableUpgradeUI();
            });
        }

        upgradeUI.SetActive(true);
        SetFirstSelectedIfGamepad(buttons.GetChild(0).gameObject);
    }

    public void DisableUpgradeUI()
    {
        ReturnMusicVolume();

        GameStateManager.Instance.ToRunning();
        upgradeUI.SetActive(false);
        RemoveFirstSelected();
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

    public void DisableDashAbility()
    {
        if (!dashAbilityEnabled) return;
        dashAbilityEnabled = false;

        DashAbility dashAbility = player.GetComponent<DashAbility>();
        int cooldown = Mathf.RoundToInt(dashAbility.GetDashCooldown());

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

        GameStateManager.Instance.ToPaused();
        pauseUI.SetActive(true);
        SetFirstSelectedIfGamepad(pauseUI.transform.Find("ResumeButton").gameObject);
    }

    public void DisablePauseUI()
    {
        ReturnMusicVolume();

        GameStateManager.Instance.ToRunning();
        pauseUI.SetActive(false);
        RemoveFirstSelected();
    }
    private void LowerMusicVolume()
    {
        GameObject.Find("AudioManager").transform.Find("MusicAudioSource").GetComponent<AudioSource>().volume *= 0.5f;
    }

    private void ReturnMusicVolume()
    {
        GameObject.Find("AudioManager").transform.Find("MusicAudioSource").GetComponent<AudioSource>().volume *= 2;
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

    private void SetFirstSelectedIfGamepad(GameObject obj)
    {
        firstSelected = obj;

        // Only select first if player is using a gamepad
        if (Gamepad.current != null && Gamepad.current.enabled && playerInput.currentControlScheme == "Gamepad") EventSystem.current.SetSelectedGameObject(obj);
    }

    private void RemoveFirstSelected()
    {
        firstSelected = null;
        EventSystem.current.SetSelectedGameObject(null);
    }
}
