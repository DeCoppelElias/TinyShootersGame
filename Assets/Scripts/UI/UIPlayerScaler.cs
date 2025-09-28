using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIPlayerScaler : MonoBehaviour
{
    private readonly Dictionary<int, Canvas> playerCanvases = new Dictionary<int, Canvas>();

    private void Start()
    {
        PlayerInput[] initialPlayers = FindObjectsOfType<PlayerInput>();
        foreach (PlayerInput player in initialPlayers) Register(player);
    }

    public void Register(PlayerInput playerInput)
    {
        Canvas playerCanvas = playerInput.GetComponentInChildren<Canvas>();

        playerCanvases[playerInput.playerIndex] = playerCanvas;
        ApplyLayout();
    }

    public void Unregister(PlayerInput playerInput)
    {
        if (playerCanvases.ContainsKey(playerInput.playerIndex))
        {
            playerCanvases.Remove(playerInput.playerIndex);
            ApplyLayout();
        }
    }

    private void ApplyLayout()
    {
        List<Canvas> orderedCanvases = playerCanvases
            .OrderBy(pair => -pair.Key)
            .Select(pair => pair.Value)
            .ToList();

        if (orderedCanvases.Count == 1)
        {
            Stretch(orderedCanvases[0], new Vector2(0, 0), new Vector2(1, 1));
            return;
        }
        else if (orderedCanvases.Count == 2)
        {
            Stretch(orderedCanvases[0], new Vector2(0f, 0f), new Vector2(0.5f, 1f)); 
            Stretch(orderedCanvases[1], new Vector2(0.5f, 0f), new Vector2(1f, 1f));
        }
        else
        {
            throw new System.Exception($"Player count {orderedCanvases.Count} is not supported by the UI scaling system");
        }
    }

    private static void Stretch(Canvas canvas, Vector2 min, Vector2 max)
    {
        var rt = (RectTransform)canvas.transform.Find("UIParent");
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
    }
}
