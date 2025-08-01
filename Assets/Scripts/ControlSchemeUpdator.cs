using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlSchemeUpdator : MonoBehaviour
{
    private PlayerInput playerInput;
    private string currentControlScheme;
    private UIManager uiManager;

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
    }

    private void Update()
    {
        if (playerInput.currentControlScheme != currentControlScheme)
        {
            currentControlScheme = playerInput.currentControlScheme;
            uiManager.OnControlsChanged();
        }
    }
}
