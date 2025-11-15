using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PVPLobbyManager : MonoBehaviour
{
    [SerializeField] private List<LobbyPlayerSlot> playerSlots = new List<LobbyPlayerSlot>();
    [SerializeField] private string battleSceneName = "PVPBattle";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void OnEnable()
    {
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;
        PlayerReady.OnPlayerReady += HandlePlayerReady;
    }

    private void OnDisable()
    {
        if (PlayerInputManager.instance != null)
            PlayerInputManager.instance.onPlayerJoined -= OnPlayerJoined;
        PlayerReady.OnPlayerReady -= HandlePlayerReady;
    }

    private void Start()
    {
        foreach (var slot in playerSlots)
        {
            slot.playerJoinedUI?.SetActive(false);
            slot.readyIndicator?.SetActive(false);
        }
    }

    private void OnPlayerJoined(PlayerInput input)
    {
        input.neverAutoSwitchControlSchemes = true;
        var freeSlot = playerSlots.FirstOrDefault(s => s.LobbyPlayer == null);

        if (freeSlot != null)
        {
            AssignPlayerToSlot(input, freeSlot);
        }
        else
        {
            ReplaceLastPlayer(input);
        }
    }

    private void AssignPlayerToSlot(PlayerInput input, LobbyPlayerSlot slot)
    {
        slot.AssignPlayer(input.GetComponentInParent<LobbyPlayer>());

        Debug.Log($"{input.gameObject.name} joined using {input.currentControlScheme}");
    }

    private void ReplaceLastPlayer(PlayerInput newInput)
    {
        var oldestSlot = playerSlots
        .Where(s => s.PlayerInput != null)
        .OrderBy(s => s.JoinedTimestamp)
        .First();

        if (oldestSlot.LobbyPlayer != null)
            Destroy(oldestSlot.LobbyPlayer.gameObject);

        AssignPlayerToSlot(newInput, oldestSlot);
        Debug.Log($"{newInput.gameObject.name} replaced previous player in slot {oldestSlot.slotIndex}");
    }

    private void HandlePlayerReady(PlayerInput input)
    {
        var slot = playerSlots.FirstOrDefault(s => s.PlayerInput == input);
        if (slot == null)
            return;

        slot.readyIndicator.SetActive(true);

        if (playerSlots.All(s => s.PlayerAssigned() && s.PlayerInput != null && s.IsReady))
        {
            StartCoroutine(StartBattleAfterDelay());
        }
    }

    private IEnumerator StartBattleAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        foreach (var slot in playerSlots)
        {
            DontDestroyOnLoad(slot.LobbyPlayer.gameObject);
        }

        SceneManager.LoadScene(battleSceneName);
    }

    public void QuitToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    [System.Serializable]
    public class LobbyPlayerSlot
    {
        public int slotIndex;

        public LobbyPlayer LobbyPlayer { get; private set; }
        public float JoinedTimestamp { get; private set; }

        public Color playerColor;
        public GameObject playerJoinedUI;
        public GameObject readyIndicator;
        public Text readyText;
        public Image playerUIImage;

        public void AssignPlayer(LobbyPlayer lobbyPlayer)
        {
            LobbyPlayer = lobbyPlayer;
            JoinedTimestamp = Time.time;

            LobbyPlayer.playerIndex = slotIndex;
            LobbyPlayer.color = playerColor;

            lobbyPlayer.gameObject.name = $"Player {slotIndex}";

            playerUIImage.color = playerColor;
            readyText.text = GetReadyPrompt(PlayerInput.currentControlScheme);

            playerJoinedUI.SetActive(true);
            readyIndicator.SetActive(false);
        }

        private string GetReadyPrompt(string controlScheme)
        {
            if (controlScheme == "Gamepad") return "Press Select to ready up!";
            if (controlScheme == "Keyboard&Mouse") return "Press X to ready up!";
            else return "Press Ready button!";
        }

        public bool PlayerAssigned()
        {
            return this.LobbyPlayer != null;
        }

        public PlayerReady PlayerReady { get => LobbyPlayer.GetComponent<PlayerReady>(); }
        public PlayerInput PlayerInput { get => LobbyPlayer.GetComponentInChildren<PlayerInput>(); }

        public bool IsReady => PlayerReady.IsReady;
    }
}

