using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;

public class UITransition : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    public void FadeIn()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        DisableButtons();

        canvasGroup.DOFade(1f, 0.5f)
            .SetUpdate(true)
            .OnComplete(() => {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                EnableButtons();
            });
    }

    public void FadeOut()
    {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        DisableButtons();

        canvasGroup.DOFade(0f, 0.5f)
            .SetUpdate(true);
    }

    public bool Enabled()
    {
        return canvasGroup.interactable;
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
}
