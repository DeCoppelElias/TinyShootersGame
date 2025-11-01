using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreEntry : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private Text _reasonText;
    [SerializeField] private Text _scoreText;
    [SerializeField] private Text _multiplierText;
    [SerializeField] private Image _progressBar;

    [SerializeField] private CanvasGroup _canvasGroup;

    private float _targetFill = 0;

    private void Start()
    {
        _progressBar.transform.localScale = new Vector3(0, 1, 1);
    }

    public void SetReason(string reason)
    {
        _reasonText.text = reason;
    }

    /// <summary>
    /// Update the displayed entry. timeRemaining can be used to show a progress bar if you have one.
    /// </summary>
    public void UpdateScore(int entryScore, int multiplier, float timeRemaining, float totalDuration)
    {
        _scoreText.text = entryScore.ToString();
        _multiplierText.text = "x" + multiplier.ToString();

        if (_progressBar != null && totalDuration > 0)
        {
            _targetFill = Mathf.Clamp01(timeRemaining / totalDuration);

            _progressBar.transform.localScale = new Vector3(Mathf.Lerp(_progressBar.transform.localScale.x, _targetFill, Time.deltaTime * 20f), 1, 1);
        }
    }

    public void PlayCommitAndDestroy()
    {
        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        float t = 0f;
        float start = _canvasGroup.alpha;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(start, 0f, t / fadeDuration);
            yield return null;
        }
        Destroy(gameObject);
    }
}
