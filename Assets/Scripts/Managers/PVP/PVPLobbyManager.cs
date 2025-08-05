using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PVPLobbyManager : MonoBehaviour
{
    private PlayerInput player1;
    private GameObject playerJoinedUI1;
    private Text readyText1;
    private GameObject ready1;

    private PlayerInput player2;
    private GameObject playerJoinedUI2;
    private Text readyText2;
    private GameObject ready2;

    private int lastSwitched = 2;

    private GameObject eventSystem;

    void Start()
    {
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;

        playerJoinedUI1 = GameObject.Find("Player1").transform.Find("PlayerJoinedUI").gameObject;
        ready1 = playerJoinedUI1.transform.Find("Ready").gameObject;
        readyText1 = playerJoinedUI1.transform.Find("ReadyExplanation").GetComponent<Text>();
        playerJoinedUI2 = GameObject.Find("Player2").transform.Find("PlayerJoinedUI").gameObject;
        ready2 = playerJoinedUI2.transform.Find("Ready").gameObject;
        readyText2 = playerJoinedUI2.transform.Find("ReadyExplanation").GetComponent<Text>();

        playerJoinedUI1.SetActive(false);
        ready1.SetActive(false);
        playerJoinedUI2.SetActive(false);
        ready2.SetActive(false);

        eventSystem = GameObject.Find("EventSystem");
    }

    void OnPlayerJoined(PlayerInput input)
    {
        if (player1 == null)
        {
            player1 = input;
            input.gameObject.name = "Player 1";
            Debug.Log("Player 1 joined!");
        }
        else if (player2 == null)
        {
            player2 = input;
            input.gameObject.name = "Player 2";
            Debug.Log("Player 2 joined!");
        }
        else 
        { 
            if (lastSwitched == 1)
            {
                Destroy(player2.gameObject);
                player2 = input;
                input.gameObject.name = "Player 2";
                lastSwitched = 2;
                Debug.Log("Player 2 replaced!");
            }
            else if (lastSwitched == 2)
            {
                Destroy(player1.gameObject);
                player1 = input;
                input.gameObject.name = "Player 1";
                lastSwitched = 1;
                Debug.Log("Player 1 replaced!");
            }
        }

        input.gameObject.AddComponent<PlayerReady>();
        input.GetComponent<PlayerController>().collectInput = false;
        input.neverAutoSwitchControlSchemes = true;
        input.GetComponentInChildren<Canvas>().enabled = false;

        if (input.gameObject.name == "Player 1")
        {
            playerJoinedUI1.SetActive(true);
            input.transform.position = new Vector3(-5, 0, 0);

            if (input.currentControlScheme == "Gamepad") readyText1.text = "Press SELECT to ready up!";
            else readyText1.text = "Press X to ready up!";
        }
        if (input.gameObject.name == "Player 2")
        {
            playerJoinedUI2.SetActive(true);
            input.transform.position = new Vector3(5, 0, 0);

            if (input.currentControlScheme == "Gamepad") readyText2.text = "Press SELECT to ready up!";
            else readyText2.text = "Press X to ready up!";
        }
    }

    public void TryStartGame(PlayerInput playerInput)
    {
        if (playerInput.gameObject.name == "Player 1")
        {
            ready1.SetActive(true);
        }
        else if (playerInput.gameObject.name == "Player 2")
        {
            ready2.SetActive(true);
        }

        if (player1 != null && player2 != null && player1.GetComponent<PlayerReady>().IsReady && player2.GetComponent<PlayerReady>().IsReady)
        {
            Destroy(player1.gameObject.GetComponent<PlayerReady>());
            DontDestroyOnLoad(player1);
            player1.GetComponent<PlayerController>().collectInput = true;
            player1.GetComponentInChildren<Canvas>().enabled = true;

            Destroy(player2.gameObject.GetComponent<PlayerReady>());
            DontDestroyOnLoad(player2);
            player2.GetComponent<PlayerController>().collectInput = true;
            player2.GetComponentInChildren<Canvas>().enabled = true;

            DontDestroyOnLoad(eventSystem);

            SceneManager.LoadScene("PVPBattle");
        }
    }

    public void QuitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
