using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;

public class UIFadeTransition : UITransition
{
    [SerializeField] private CanvasGroup canvasGroup;

    private enum TransitionState { Idle, FadeIn, FadeOut}
    private TransitionState transitionState = TransitionState.Idle;

    public override void Transition()
    {
        FadeIn();
    }

    public override void ReverseTransition()
    {
        FadeOut();
    }

    private void FadeIn()
    {
        if (transitionState != TransitionState.Idle) return;
        transitionState = TransitionState.FadeIn;

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        DisableButtons();

        canvasGroup.DOFade(1f, 0.5f)
            .SetUpdate(true)
            .OnComplete(() => 
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                EnableButtons();
                transitionState = TransitionState.Idle;
            });
    }

    private void FadeOut()
    {
        if (transitionState != TransitionState.Idle) return;
        transitionState = TransitionState.FadeOut;

        canvasGroup.alpha = 1;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        DisableButtons();

        canvasGroup.DOFade(0f, 0.5f)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                transitionState = TransitionState.Idle;
            });
    }

    public override bool IsEnabled()
    {
        return canvasGroup.alpha == 1;
    }
    public override bool IsDisabled()
    {
        return canvasGroup.alpha == 0;
    }

    private void DisableButtons()
    {
        Button[] buttons = canvasGroup.transform.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            var nav = button.navigation;
            nav.mode = Navigation.Mode.None;
            button.navigation = nav;
        }
    }

    private void EnableButtons()
    {
        Button[] buttons = canvasGroup.transform.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            var nav = button.navigation;
            nav.mode = Navigation.Mode.Explicit;
            button.navigation = nav;
        }
    }

    public override void InstantReverseTransition()
    {
        if (transitionState != TransitionState.Idle) return;

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        DisableButtons();
    }
}
