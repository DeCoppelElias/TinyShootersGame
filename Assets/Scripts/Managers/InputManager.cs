using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    private SharedUIManager uiManager;
    private void Start()
    {
        uiManager = GameObject.Find("UIManager").GetComponent<SharedUIManager>();
    }
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Escape))
        {
            uiManager.SwitchPauseUI();
        }*/
    }
}
