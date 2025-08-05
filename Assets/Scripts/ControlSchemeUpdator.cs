using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ControlSchemeUpdator : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    private string currentControlScheme;

    private void Update()
    {
        if (playerInput.currentControlScheme != currentControlScheme)
        {
            currentControlScheme = playerInput.currentControlScheme;
            OnControlsChanged();
        }
    }

    public void OnControlsChanged()
    {
        if (Gamepad.current != null && Gamepad.current.enabled && playerInput.currentControlScheme == "Gamepad")
        {
            EventSystem.current.SetSelectedGameObject(GeneralUIManager.Instance.FirstSelected);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
