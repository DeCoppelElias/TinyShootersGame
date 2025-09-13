using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : UIElement
{
    [SerializeField] private Player player;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button powerupButton;

    protected override void Start()
    {
        base.Start();

        upgradeButton.onClick.AddListener(player.GetComponentInChildren<PlayerUIManager>().Enable<UpgradeUI>);
        powerupButton.onClick.AddListener(player.GetComponentInChildren<PlayerUIManager>().Enable<PowerupUI>);
    }
    protected override void DisableActions()
    {
        this.gameObject.SetActive(false);
    }

    protected override void EnableActions()
    {
        this.gameObject.SetActive(true);
    }

    public override bool Enabled()
    {
        return this.gameObject.activeSelf;
    }

    protected override void InstantDisableActions()
    {
        this.gameObject.SetActive(false);
    }
}
