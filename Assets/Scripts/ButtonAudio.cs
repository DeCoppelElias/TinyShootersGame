using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonAudio : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    private PlayerInput playerInput;
    public bool canPlaySound = false;

    private void OnEnable()
    {
        StartCoroutine(EnableNavigateSound());
    }
    private IEnumerator EnableNavigateSound()
    {
        canPlaySound = false;
        yield return new WaitForSecondsRealtime(0.1f);
        canPlaySound = true;
    }

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(PlayClickSound);
    }
    public void OnSelect(BaseEventData eventData)
    {
        if (canPlaySound && AudioManager.Instance != null) AudioManager.Instance.PlayNavigateSound();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (canPlaySound && AudioManager.Instance != null) AudioManager.Instance.PlayHoverSound();
    }

    private void PlayClickSound()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSound();
    }
}
