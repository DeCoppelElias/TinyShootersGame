using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] private int particleLimit = 100;
    public enum ParticleType { Damage, Blood, BulletExplosion, Trail}

    [SerializeField] public GameObject scoreTextPrefab;

    [SerializeField] private List<ParticlePool> particlePools = new List<ParticlePool>();

    private void Start()
    {
        foreach (ParticlePool particlePool in particlePools)
        {
            particlePool.InitializePool(particleLimit, Mathf.RoundToInt(0.1f * particleLimit));
        }
    }

    public GameObject CreateParticle(ParticleType particleType, Vector3 position, Quaternion rotation, Color color)
    {
        ParticlePool particlePool = GetParticlePool(particleType);
        Particle particle = particlePool.GetParticle();
        if (particle == null) return null;

        particle.Initialise(position, rotation, color);
        particle.AssignOnComplete(() => particlePool.ReturnParticle(particle));
        particle.Play();

        return particle.gameObject;
    }

    private ParticlePool GetParticlePool(ParticleType particleType)
    {
        foreach (ParticlePool particlePool in particlePools)
        {
            if (particlePool.GetPoolType() == particleType)
            {
                return particlePool;
            }
        }

        throw new Exception($"Particle pool does not exist for type: {particleType}");
    }
}
