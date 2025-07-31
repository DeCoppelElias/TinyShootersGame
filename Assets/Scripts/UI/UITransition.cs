using UnityEngine;
using DG.Tweening;

public class UITransition : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    public void FadeIn()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        canvasGroup.DOFade(1f, 0.5f)
            .SetUpdate(true)
            .OnComplete(() => {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            });
    }

    public void FadeOut()
    {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        canvasGroup.DOFade(0f, 0.5f)
            .SetUpdate(true)
            .OnComplete(() => {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            });
    }

    public bool Enabled()
    {
        return canvasGroup.interactable;
    }
}
