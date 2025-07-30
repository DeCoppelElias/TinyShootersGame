using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(UITransition))]
public class UpgradeUI : UIElement
{
    [SerializeField] private GameObject classUpgradeButtonPrefab;
    private UITransition uiTransition;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        uiTransition = GetComponent<UITransition>();
    }

    public override void Enable()
    {
        List<PlayerClass> upgrades = player.GetUpgrades();
        if (upgrades.Count == 0)
        {
            uiManager.DisableUI(this);
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
                uiManager.DisableUI(this);
            });
        }

        uiManager.SetFirstSelectedIfGamepad(buttons.GetChild(0).gameObject);
        uiTransition.FadeIn();

        return;
    }

    public override UIElement Disable()
    {
        uiTransition.FadeOut();
        return this;
    }
}
