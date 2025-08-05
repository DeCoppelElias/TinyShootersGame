using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    private GeneralUIManager uiManager;
    private void Start()
    {
        uiManager = GameObject.Find("UIManager").GetComponent<GeneralUIManager>();
    }
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Escape))
        {
            uiManager.SwitchPauseUI();
        }*/
    }
}
