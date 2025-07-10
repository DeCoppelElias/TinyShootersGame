using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BulletExplosionParticle : Particle
{
    public override void Play()
    {
        float animationTime = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;
        StartCoroutine(CompleteAfterAnimation(animationTime));
    }

    public override void Initialise(Vector3 position, Quaternion rotation, Vector3 scale, Color color)
    {
        this.transform.position = position;
        this.GetComponent<SpriteRenderer>().color = color;
        ParticleSystem.MainModule main = GetComponentInChildren<ParticleSystem>().main;
        main.startColor = color;

        this.transform.localScale = scale;
    }

    IEnumerator CompleteAfterAnimation(float animationTime)
    {
        yield return new WaitForSeconds(animationTime);

        this.Complete();
    }
}
