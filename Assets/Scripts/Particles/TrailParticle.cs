using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TrailParticle : Particle
{
    private SpriteRenderer spriteRenderer;
    private enum ParticleState { Inactive, Idle, Fadeout }
    private ParticleState particleState = ParticleState.Inactive;

    private float idleDuration = 5;
    private float idleStartTime;
    private float fadeoutDuration = 1;
    private float fadoutStartTime;
    public override void Initialise(Vector3 position, Quaternion rotation, Color color)
    {
        this.transform.position = position;
        this.transform.rotation = rotation;
        this.spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;
    }

    public override void Play()
    {
        this.idleStartTime = Time.time;
        particleState = ParticleState.Idle;
    }

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
            ChangeTransparency();

            if (Time.time - fadoutStartTime > fadeoutDuration)
            {
                Complete();
            }
        }
    }

    private void ChangeTransparency()
    {
        float p = Mathf.Clamp((Time.time - fadoutStartTime) / fadeoutDuration, 0, 1);
        float a = 0.3f - (p * 0.3f);
        Color currentColor = spriteRenderer.color;
        currentColor.a = a;
        spriteRenderer.color = currentColor;
    }
}
