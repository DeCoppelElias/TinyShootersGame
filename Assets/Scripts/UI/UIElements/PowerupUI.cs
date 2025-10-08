using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(UIFadeTransition))]
public class PowerupUI : PlayerUIElement
{
    private UIFadeTransition uiTransition;

    private List<Button> buttons = new List<Button>();

    [Header("Powerup Colors")]
    [SerializeField] private Color commonColor = new Color(93f / 255, 32f / 255, 93f / 255);
    [SerializeField] private Color uncommonColor = new Color(59f / 255, 93f / 255, 201f / 255);
    [SerializeField] private Color rareColor;

    private bool initialised = false;

    protected override void Start()
    {
        base.Start();
        if (!initialised) Init();
    }

    private void Init()
    {
        uiTransition = GetComponent<UIFadeTransition>();

        buttons.Clear();
        Transform buttonTransform = this.transform.Find("Buttons");
        for (int i = 0; i < buttonTransform.childCount; i++)
        {
            Transform buttonChildTransform = buttonTransform.GetChild(i);
            this.buttons.Add(buttonChildTransform.GetComponent<Button>());
        }

        this.pausesGame = true;

        initialised = true;
    }

    protected override void EnableActions()
    {
        if (!initialised) Init();
        if (PowerupManager.Instance == null) return;

        List<Powerup> powerups = PowerupManager.Instance.ChooseRandomPowerups(3);
        if (powerups.Count != 3)
        {
            Disable();
            return;
        }

        // Link each button to upgrading the player to that class
        for (int i = 0; i < buttons.Count; i++)
        {
            Button button = this.buttons[i];
            Powerup currentPowerup = powerups[i];

            Text rarity = button.transform.Find("Rarity").GetComponent<Text>();
            rarity.text = currentPowerup.rarity.ToString();
            Color baseColor;
            if (currentPowerup.rarity == Powerup.Rarity.Uncommon) baseColor = uncommonColor;
            else if (currentPowerup.rarity == Powerup.Rarity.Rare) baseColor = rareColor;
            else baseColor = commonColor;

            // Brighten the color for text
            Color.RGBToHSV(baseColor, out float h, out float s, out float v);
            v = Mathf.Clamp01(v + 0.5f);
            Color textColor = Color.HSVToRGB(h, s, v);
            rarity.color = textColor;

            Text title = button.transform.Find("Title").GetComponent<Text>();
            title.text = currentPowerup.powerupName;

            Text description = button.transform.Find("Description").GetComponent<Text>();
            description.text = currentPowerup.GenerateUIDescription();

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                Player.ApplyPowerup(currentPowerup);
                Disable();
            });

            // Change color depending on rarity of powerup
            Image buttonImage = button.transform.GetComponent<Image>();
            if (currentPowerup.rarity == Powerup.Rarity.Uncommon) buttonImage.color = uncommonColor;
            else if (currentPowerup.rarity == Powerup.Rarity.Rare) buttonImage.color = rareColor;
            else buttonImage.color = commonColor;
        }

        // Make navigation of buttons
        for (int i = 0; i < buttons.Count; i++)
        {
            Button button = buttons[i];
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.Explicit;

            // Assign left and right navigation to neighboring buttons
            if (i > 0)
                navigation.selectOnLeft = buttons[i - 1].GetComponent<Selectable>();
            if (i < buttons.Count - 1)
                navigation.selectOnRight = buttons[i + 1].GetComponent<Selectable>();

            button.navigation = navigation;
        }

        firstSelected = buttons[0].gameObject;
        uiTransition.Transition();
    }

    protected override void DisableActions()
    {
        uiTransition.ReverseTransition();
    }
    protected override void InstantDisableActions()
    {
        uiTransition.InstantReverseTransition();
    }

    public override bool Enabled()
    {
        return uiTransition.Enabled();
    }
}
