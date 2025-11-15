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
    private GameObject firstSelected;

    public GameObject mainMenuFirstSelected;
    public GameObject levelMenuFirstSelected;

    public PlayerInput playerInput;

    private GameObject highScoreUI;
    private Text highScoreText;

    [SerializeField] private GameObject mainUI;
    [SerializeField] private GameObject levelUI;
    [SerializeField] private Transform levelsParent;

    private string currentControlScheme;

    private void Start()
    {
        firstSelected = mainMenuFirstSelected;

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

        for (int levelIndex = 1; levelIndex < levelsParent.childCount; levelIndex++)
        {
            Transform levelTransform = levelsParent.GetChild(levelIndex);
            string levelString = $"Level {levelIndex}";
            if (!PlayerPrefs.HasKey(levelString))
            {
                Button levelButton = levelTransform.GetComponent<Button>();
                levelButton.interactable = false;
            }
        }
        
        levelUI.SetActive(false);
    }

    private void Update()
    {
        if (playerInput.currentControlScheme != currentControlScheme)
        {
            UpdateFirstSelected();
            currentControlScheme = playerInput.currentControlScheme;
        }
    }

    public void ToLevelUI()
    {
        this.levelUI.SetActive(true);
        this.mainUI.SetActive(false);

        firstSelected = levelMenuFirstSelected;
        UpdateFirstSelected();
    }
    public void ToMainUI()
    {
        this.levelUI.SetActive(false);
        this.mainUI.SetActive(true);

        firstSelected = mainMenuFirstSelected;
        UpdateFirstSelected();
    }

    public void PlayGame(int level)
    {
        SceneTransitionManager.Instance.Level = level;
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

    public void UpdateFirstSelected()
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
