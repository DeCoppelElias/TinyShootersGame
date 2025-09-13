using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(UIFadeTransition))]
public class UpgradeUI : PlayerUIElement
{
    [SerializeField] private GameObject classUpgradeButtonPrefab;
    private UIFadeTransition uiTransition;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        this.pausesGame = true;
        uiTransition = GetComponent<UIFadeTransition>();
        InstantDisableActions();
    }

    protected override void EnableActions()
    {
        List<PlayerClass> upgrades = player.GetUpgrades();
        if (upgrades.Count == 0)
        {
            Disable();
            return;
        }

        Transform buttons = this.transform.Find("Buttons");
        // First make sure that the amount of buttons and upgrades are the same
        if (buttons.childCount > upgrades.Count)
        {
            for (int i = upgrades.Count; i < buttons.childCount; i++)
            {
                GameObject child = buttons.GetChild(i).gameObject;
                child.transform.SetParent(null);
                if (child != null) Destroy(child);
            }
        }
        else
        {
            for (int i = buttons.childCount; i < upgrades.Count; i++)
            {
                Instantiate(classUpgradeButtonPrefab, buttons);
            }
        }

        // Make navigation of buttons
        for (int i = 0; i < buttons.childCount; i++)
        {
            Button button = buttons.GetChild(i).GetComponent<Button>();
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.Explicit;

            // Assign left and right navigation to neighboring buttons
            if (i > 0)
                navigation.selectOnLeft = buttons.GetChild(i - 1).GetComponent<Selectable>();
            if (i < buttons.childCount - 1)
                navigation.selectOnRight = buttons.GetChild(i + 1).GetComponent<Selectable>();

            button.navigation = navigation;
        }

        // Link each button to upgrading the player to that class
        for (int i = 0; i < buttons.childCount; i++)
        {
            Transform buttonTransform = buttons.GetChild(i);
            PlayerClass currentPlayerClass = upgrades[i];

            Text classTitle = buttonTransform.Find("Banner").GetComponentInChildren<Text>();
            classTitle.text = currentPlayerClass.className;

            Text classDescription = buttonTransform.Find("ClassDescription").GetComponent<Text>();
            classDescription.text = currentPlayerClass.classDescription;

            Image image = buttonTransform.Find("Banner").GetComponentInChildren<Image>();
            image.sprite = currentPlayerClass.UISprite;

            Button button = buttonTransform.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                player.ApplyClass(currentPlayerClass);
                Disable();
            });
        }

        firstSelected = buttons.GetChild(0).gameObject;
        uiTransition.Transition();

        return;
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
