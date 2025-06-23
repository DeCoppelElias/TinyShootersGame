using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    private float duration;
    private float startTime;
    private TextMeshPro textMeshPro;
    private bool start = false;

    // Update is called once per frame
    void Update()
    {
        if (!start) return;

        // Setting transparancy
        float p = Mathf.Clamp((Time.time - startTime) / duration, 0, 1);
        float a = 0.5f - (p / 2);
        Color currentColor = textMeshPro.color;
        currentColor.a = a;
        textMeshPro.color = currentColor;

        // Setting size
        // transform.localScale = new Vector3(1-p, 1-p, 1-p);

        if (p == 1)
        {
            Destroy(this.gameObject);
        }
    }

    public void Initialise(float duration, float score)
    {
        this.textMeshPro = GetComponent<TextMeshPro>();
        this.textMeshPro.text = score.ToString();
        this.textMeshPro.renderer.sortingLayerName = "Debug";
        this.startTime = Time.time;
        this.duration = duration;
        this.start = true;
    }
}
