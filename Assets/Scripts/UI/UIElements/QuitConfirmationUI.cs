using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitConfirmationUI : UIElement
{
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    protected override void Start()
    {
        base.Start();

        noButton.onClick.AddListener(() =>
        {
            SharedUIManager.Instance.Enable<PauseWithConfirmationUI>();
            InstantDisable();
        });
        yesButton.onClick.AddListener(GameStateManager.Instance.QuitToMainMenu);

        InstantDisable();
    }

    public override bool IsDisabled()
    {
        return !this.gameObject.activeSelf;
    }

    public override bool IsEnabled()
    {
        return this.gameObject.activeSelf;
    }

    protected override void DisableActions()
    {
        this.gameObject.SetActive(false);
    }

    protected override void EnableActions()
    {
        firstSelected = noButton.gameObject;
        this.gameObject.SetActive(true);
    }

    protected override void InstantDisableActions()
    {
        this.gameObject.SetActive(false);
    }
}
