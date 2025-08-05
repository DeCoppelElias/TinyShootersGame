using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class PlayerUIManager : MonoBehaviour
{
    private Canvas playerCanvas;
    private MultiplayerEventSystem playerEventSystem;
    private Player player;

    private GameObject upgradeUI;
    private GameObject powerupUI;
    [SerializeField] private GameObject upgradeButtonPrefab;

    private GameObject abilityUI;
    private GameObject dashAbilityUI;
    private bool dashAbilityEnabled = true;
    private GameObject reflectAbilityUI;
    private bool reflectAbilityEnabled = true;
    private GameObject classAbilityUI;
    private bool classAbilityEnabled = true;
    private bool classAbilityInitialised = false;

    private PlayerInput playerInput;

    private string currentControlScheme;
    public GameObject FirstSelected { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponentInParent<Player>();
        playerCanvas = GetComponent<Canvas>();

        playerEventSystem = player.GetComponentInChildren<MultiplayerEventSystem>();
        LinkToPlayer();

        upgradeUI = playerCanvas.transform.Find("UIParent").Find("UpgradeUI").gameObject;
        upgradeUI.GetComponent<UIElement>().onDisable.AddListener(() =>
        {
            UIVisibilityManager.Instance.RegisterUIHidden();
            RemoveFirstSelected();
        });
        powerupUI = playerCanvas.transform.Find("UIParent").Find("PowerupUI").gameObject;
        powerupUI.GetComponent<UIElement>().onDisable.AddListener(() =>
        {
            UIVisibilityManager.Instance.RegisterUIHidden();
            RemoveFirstSelected();
        });

        abilityUI = playerCanvas.transform.Find("UIParent").Find("AbilityUI").gameObject;
        dashAbilityUI = abilityUI.transform.Find("DashAbility").gameObject;
        reflectAbilityUI = abilityUI.transform.Find("ReflectAbility").gameObject;
        classAbilityUI = abilityUI.transform.Find("ClassAbility").gameObject;
        if (!classAbilityInitialised) classAbilityUI.SetActive(false);

        playerInput = player.GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (playerInput.currentControlScheme != currentControlScheme)
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

    private void LinkToPlayer()
    {
        DashAbility dashAbility = player.GetComponent<DashAbility>();
        if (dashAbility != null)
        {
            dashAbility.onReady.AddListener(EnableDashAbility);
            dashAbility.onPerformed.AddListener(DisableDashAbility);
        }

        ReflectShieldAbility reflectShieldAbility = player.GetComponent<ReflectShieldAbility>();
        if (reflectShieldAbility != null)
        {
            reflectShieldAbility.onReady.AddListener(EnableReflectAbility);
            reflectShieldAbility.onPerformed.AddListener(DisableReflectAbility);
        }

        AbilityBehaviour abilityBehaviour = player.GetComponent<AbilityBehaviour>();
        if (abilityBehaviour != null)
        {
            abilityBehaviour.onReady.AddListener(() => EnableClassAbility(abilityBehaviour));
            abilityBehaviour.onPerformed.AddListener(() => DisableClassAbility(abilityBehaviour));
            abilityBehaviour.onLinkAbility.AddListener(() => SetClassAbilityUI(abilityBehaviour));
        }
    }

    public void EnableUpgradeUI()
    {
        UIElement upgradeUIElement = this.upgradeUI.GetComponent<UIElement>();
        EnableUI(upgradeUIElement);
    }

    public void DisableUpgradeUI()
    {
        UIElement upgradeUIElement = this.upgradeUI.GetComponent<UIElement>();
        DisableUI(upgradeUIElement);
    }

    public void EnablePowerupUI()
    {
        UIElement powerupUIElement = this.powerupUI.GetComponent<UIElement>();
        EnableUI(powerupUIElement);
    }

    public void DisablePowerupUI()
    {
        UIElement powerupUIElement = this.powerupUI.GetComponent<UIElement>();
        DisableUI(powerupUIElement);
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

    public void EnableAbilityUI(bool enable)
    {
        abilityUI.SetActive(enable);
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

    public void SetClassAbilityUI(AbilityBehaviour abilityBehaviour)
    {
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
        if (Gamepad.current != null && Gamepad.current.enabled && playerInput.currentControlScheme == "Gamepad")
        {
            Debug.Log($"Setting first selected {FirstSelected}");
            playerEventSystem.SetSelectedGameObject(obj);
        }
    }

    private void RemoveFirstSelected()
    {
        FirstSelected = null;
        playerEventSystem.SetSelectedGameObject(null);
    }
}
