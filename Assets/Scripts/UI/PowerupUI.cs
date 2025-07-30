using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(UITransition))]
public class PowerupUI : UIElement
{
    private PowerupManager powerupManager;
    private UITransition uiTransition;

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
        powerupManager = GameObject.Find("PowerupManager").GetComponent<PowerupManager>();
        uiTransition = GetComponent<UITransition>();

        buttons.Clear();
        Transform buttonTransform = this.transform.Find("Buttons");
        for (int i = 0; i < buttonTransform.childCount; i++)
        {
            Transform buttonChildTransform = buttonTransform.GetChild(i);
            this.buttons.Add(buttonChildTransform.GetComponent<Button>());
        }

        initialised = true;
    }

    public override void Enable()
    {
        if (!initialised) Init();

        List<Powerup> powerups = powerupManager.ChooseRandomPowerups(3);
        if (powerups.Count != 3)
        {
            uiManager.DisableUI(this);
            return;
        }

        Button button = this.buttons[0];
        // Link each button to upgrading the player to that class
        for (int i = 0; i < buttons.Count; i++)
        {
            button = this.buttons[i];
            Powerup currentPowerup = powerups[i];

            Text title = button.transform.Find("Title").GetComponent<Text>();
            title.text = currentPowerup.powerupName;

            Text description = button.transform.Find("Description").GetComponent<Text>();
            description.text = currentPowerup.GenerateUIDescription();

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                player.ApplyPowerup(currentPowerup);
                uiManager.DisableUI(this);
            });

            // Change color depending on rarity of powerup
            Image buttonImage = button.transform.GetComponent<Image>();
            if (currentPowerup.rarity == Powerup.Rarity.Uncommon) buttonImage.color = uncommonColor;
            else if (currentPowerup.rarity == Powerup.Rarity.Rare) buttonImage.color = rareColor;
            else buttonImage.color = commonColor;
        }

        uiManager.SetFirstSelectedIfGamepad(button.gameObject);
        uiTransition.FadeIn();
    }

    public override UIElement Disable()
    {
        uiTransition.FadeOut();
        return this;
    }
}
