using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PVPLobbyManager : MonoBehaviour
{
    private List<PlayerInput> players = new List<PlayerInput>();
    private GameObject playerJoinedUI1;
    private GameObject ready1;
    private GameObject playerJoinedUI2;
    private GameObject ready2;

    void Start()
    {
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;

        playerJoinedUI1 = GameObject.Find("Player1").transform.Find("PlayerJoinedUI").gameObject;
        ready1 = playerJoinedUI1.transform.Find("Ready").gameObject;
        playerJoinedUI2 = GameObject.Find("Player2").transform.Find("PlayerJoinedUI").gameObject;
        ready2 = playerJoinedUI2.transform.Find("Ready").gameObject;

        playerJoinedUI1.SetActive(false);
        ready1.SetActive(false);
        playerJoinedUI2.SetActive(false);
        ready2.SetActive(false);
    }

    void OnPlayerJoined(PlayerInput input)
    {
        players.Add(input);
        Debug.Log("Player joined: " + input.playerIndex);

        input.gameObject.AddComponent<PlayerReady>();
        input.GetComponent<PlayerController>().collectInput = false;

        if (input.playerIndex == 0) playerJoinedUI1.SetActive(true);
        if (input.playerIndex == 1) playerJoinedUI2.SetActive(true);
    }

    public void TryStartGame()
    {
        foreach (var player in players)
        {
            var ready = player.GetComponent<PlayerReady>();
            if (ready != null && ready.IsReady)
            {
                if (player.playerIndex == 0)
                    ready1.SetActive(true);
                else if (player.playerIndex == 1)
                    ready2.SetActive(true);
            }
        }

        if (players.Count == 2 && players.All(p => p.GetComponent<PlayerReady>().IsReady))
        {
            foreach (var player in players)
            {
                Destroy(player.gameObject.GetComponent<PlayerReady>());
                DontDestroyOnLoad(player);
                player.GetComponent<PlayerController>().collectInput = true;
            }

            SceneManager.LoadScene("PVPBattle");
        }
    }
}
