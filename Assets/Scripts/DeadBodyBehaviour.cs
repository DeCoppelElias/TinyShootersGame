using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadBodyBehaviour : MonoBehaviour
{
    private float startTime;
    [SerializeField] private float duration;
    private SpriteRenderer spriteRenderer;
    private bool start = false;

    // Update is called once per frame
    void Update()
    {
        if (!start) return;

        float p = Mathf.Clamp((Time.time - startTime) / duration, 0, 1);
        float a = 0.5f - (p / 2);
        Color currentColor = spriteRenderer.color;
        currentColor.a = a;
        spriteRenderer.color = currentColor;

        if (p == 1)
        {
            Destroy(this.gameObject);
        }
    }

    public void Initialise(float duration)
    {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
        this.startTime = Time.time;
        this.duration = duration;
        this.start = true;
    }
}
