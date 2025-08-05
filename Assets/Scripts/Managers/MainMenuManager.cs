using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject firstSelected;
    public PlayerInput playerInput;

    private GameObject bestTimeUI;
    private Text bestTimeText;

    private GameObject highScoreUI;
    private Text highScoreText;

    private string currentControlScheme;

    private void Start()
    {
        bestTimeUI = GameObject.Find("BestTime");
        bestTimeText = bestTimeUI.GetComponent<Text>();
        if (PlayerPrefs.HasKey("BestTime"))
        {
            float bestTime = PlayerPrefs.GetFloat("BestTime");
            TimeSpan time = TimeSpan.FromSeconds(bestTime);
            bestTimeText.text = $"Best Time: {time:hh\\:mm\\:ss}";
        }
        else
        {
            bestTimeUI.SetActive(false);
        }

        highScoreUI = GameObject.Find("HighScore");
        highScoreText = highScoreUI.GetComponent<Text>();
        if (PlayerPrefs.HasKey("HighScore"))
        {
            float highScore = PlayerPrefs.GetFloat("HighScore");
            highScoreText.text = $"Highscore: {highScore}";
        }
        else
        {
            highScoreUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInput.currentControlScheme != currentControlScheme)
        {
            ControlsChanged();
            currentControlScheme = playerInput.currentControlScheme;
        }
    }
    public void PlayGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void PlayPVP()
    {
        SceneManager.LoadScene("PVPLobby");
    }

    public void ToLeaderboard()
    {
        SceneManager.LoadScene("Leaderboard");
    }

    public void ToTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ControlsChanged()
    {
        if (!playerInput.enabled) return;

        if (Gamepad.current != null && Gamepad.current.enabled && playerInput.currentControlScheme == "Gamepad")
        {
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
