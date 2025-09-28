using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct PVPEndMatchUIData
{
    public string winningPlayerName;

    public PVPEndMatchUIData(string winningPlayerName)
    {
        this.winningPlayerName = winningPlayerName;
    }
}

public class PVPEndMatchUI : UIElement<PVPEndMatchUIData>
{
    [SerializeField] private Text winningPlayerText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    protected override void Start()
    {
        base.Start();

        restartButton.onClick.AddListener(PVPBattleManager.Instance.Restart);
        mainMenuButton.onClick.AddListener(GameStateManager.Instance.QuitToMainMenu);

        InstantDisable();
    }

    public override bool Enabled()
    {
        return this.gameObject.activeSelf;
    }

    protected override void DisableActions()
    {
        this.gameObject.SetActive(false);
    }

    protected override void EnableActions()
    {
        EnableActions(new PVPEndMatchUIData("ERROR"));
    }

    protected override void EnableActions(PVPEndMatchUIData data)
    {

        this.gameObject.SetActive(true);
    }

    protected override void InstantDisableActions()
    {
        this.gameObject.SetActive(false);
    }
}
