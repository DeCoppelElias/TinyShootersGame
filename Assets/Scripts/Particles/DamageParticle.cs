using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageParticle : Particle
{
    private float duration = 0.5f;
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
        float a = 1-p;
        Color currentColor = spriteRenderer.color;
        currentColor.a = a;
        spriteRenderer.color = currentColor;

        // Movement
        transform.position += velocity * Time.deltaTime * direction;

        if (p == 1)
        {
            Complete();
        }
    }

    public override void Initialise(Vector3 position, Quaternion rotation, Vector3 scale, Color color)
    {
        this.transform.position = position;

        this.spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;

        this.direction = UnityEngine.Random.insideUnitCircle.normalized;
        this.velocity = UnityEngine.Random.Range(1f, 3f);
    }

    public override void Play()
    {
        this.startTime = Time.time;
        this.start = true;
    }
}
