using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodParticle : MonoBehaviour
{
    private enum ParticleState { Inactive, Idle, Fadeout}
    private ParticleState particleState = ParticleState.Inactive;

    private float idleDuration;
    private float idleStartTime;
    private float fadeoutDuration;
    private float fadoutStartTime;

    private SpriteRenderer spriteRenderer;

    // Update is called once per frame
    void Update()
    {
        if (particleState == ParticleState.Inactive) return;
        else if (particleState == ParticleState.Idle)
        {
            if (Time.time - idleStartTime > idleDuration)
            {
                particleState = ParticleState.Fadeout;
                fadoutStartTime = Time.time;
            }
        }
        else if (particleState == ParticleState.Fadeout)
        {
            // Setting transparancy
            float p = Mathf.Clamp((Time.time - fadoutStartTime) / fadeoutDuration, 0, 1);
            float a = 0.3f - (p * 0.3f);
            Color currentColor = spriteRenderer.color;
            currentColor.a = a;
            spriteRenderer.color = currentColor;

            if (p == 1)
            {
                Destroy(this.gameObject);
            }
        }
    }

    public void Initialise(float idleDuration, float fadeoutDuration, Color color)
    {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;
        Color currentColor = spriteRenderer.color;
        currentColor.a = 0.3f;
        spriteRenderer.color = currentColor;

        this.idleStartTime = Time.time;
        this.idleDuration = idleDuration;
        this.fadeoutDuration = fadeoutDuration;

        particleState = ParticleState.Idle;
    }
}
