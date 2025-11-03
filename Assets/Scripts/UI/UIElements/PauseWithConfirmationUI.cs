using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseWithConfirmationUI : PauseUI
{
    protected override void SetupQuitAction()
    {
        quitButton.onClick.AddListener(() => 
        {
            SharedUIManager.Instance.Enable<QuitConfirmationUI>();
            InstantDisable();
        });
    }
}
