using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIVisibilityManager : MonoBehaviour
{
    public static UIVisibilityManager Instance { get; private set; }

    [SerializeField] private int activeUICount = 0;

    [SerializeField] private event UnityAction OnUIShown;
    [SerializeField] private event UnityAction OnUIHidden;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void RegisterUIShown()
    {
        activeUICount++;
        if (activeUICount == 1)
        {
            OnUIShown?.Invoke();
            PauseGameAndDecreaseAudio();
        }
    }

    public void RegisterUIHidden()
    {
        activeUICount = Mathf.Max(0, activeUICount - 1);
        if (activeUICount == 0)
        {
            OnUIHidden?.Invoke();
            ResumeGameAndAudio();
        }
    }

    private void PauseGameAndDecreaseAudio()
    {
        GameStateManager.Instance.ToPaused();
        AudioManager.Instance.ChangeMusicVolume(0.5f);
        Debug.Log("Game paused and audio decreased");
    }

    private void ResumeGameAndAudio()
    {
        GameStateManager.Instance.ToRunning();
        AudioManager.Instance.ChangeMusicVolume(2f);
        Debug.Log("Game resumed and audio restored");
    }
}
