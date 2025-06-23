using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePixel : MonoBehaviour
{
    private float duration;
    private float startTime;
    private SpriteRenderer spriteRenderer;
    private bool start = false;
    private Vector3 direction;
    private float velocity;

    // Update is called once per frame
    void Update()
    {
        if (!start) return;

        // Setting transparancy
        float p = Mathf.Clamp((Time.time - startTime) / duration, 0, 1);
        float a = 0.5f - (p / 2);
        Color currentColor = spriteRenderer.color;
        currentColor.a = a;
        spriteRenderer.color = currentColor;

        // Setting size
        // transform.localScale = new Vector3(1-p, 1-p, 1-p);

        // Movement
        transform.position += velocity * Time.deltaTime * direction;

        if (p == 1)
        {
            Destroy(this.gameObject);
        }
    }

    public void Initialise(float duration, Vector2 direction, float velocity)
    {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
        this.startTime = Time.time;
        this.duration = duration;
        this.direction = direction;
        this.velocity = velocity;
        this.start = true;
    }
}
