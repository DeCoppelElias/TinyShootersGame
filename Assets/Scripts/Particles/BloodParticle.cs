using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodParticle : Particle
{
    private enum ParticleState { Inactive, Idle, Fadeout}
    private ParticleState particleState = ParticleState.Inactive;

    private float idleDuration = 5;
    private float idleStartTime;
    private float fadeoutDuration = 1;
    private float fadoutStartTime;

    private List<SpriteRenderer> childSpriteRenderers = new List<SpriteRenderer>();

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

    public void Initialise(Vector3 position, Color color)
    {
        int childCount = this.transform.childCount;
        if (childCount == 0) return;

        this.transform.position = position;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = this.transform.GetChild(i);

            SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
            this.childSpriteRenderers.Add(spriteRenderer);
            spriteRenderer.color = color;
            Color currentColor = spriteRenderer.color;
            currentColor.a = 0.3f;
            spriteRenderer.color = currentColor;

            Vector2 offset = UnityEngine.Random.insideUnitCircle * 0.3f;
            float scale = UnityEngine.Random.Range(0.5f, 1.5f);
            float rotationZ = UnityEngine.Random.Range(0f, 360f);

            child.position = position + new Vector3(offset.x, offset.y, 0);
            child.localScale = child.transform.localScale * scale;
            child.rotation = Quaternion.Euler(0, 0, rotationZ);
        }
        
    }

    private void ChangeTransparency()
    {
        foreach (SpriteRenderer spriteRenderer in this.childSpriteRenderers)
        {
            float p = Mathf.Clamp((Time.time - fadoutStartTime) / fadeoutDuration, 0, 1);
            float a = 0.3f - (p * 0.3f);
            Color currentColor = spriteRenderer.color;
            currentColor.a = a;
            spriteRenderer.color = currentColor;
        }
    }

    public override void Play()
    {
        this.idleStartTime = Time.time;
        particleState = ParticleState.Idle;
    }
}
