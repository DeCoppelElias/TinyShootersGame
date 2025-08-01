using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }

    [SerializeField] private int particleLimit = 100;
    public enum ParticleType { Damage, Blood, BulletExplosion, Trail}

    [SerializeField] public GameObject scoreTextPrefab;

    [SerializeField] private List<ParticlePool> particlePools = new List<ParticlePool>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        foreach (ParticlePool particlePool in particlePools)
        {
            particlePool.InitializePool(particleLimit, Mathf.RoundToInt(0.1f * particleLimit));
        }
    }

    public void CreateParticle(ParticleType particleType, Vector3 position, Quaternion rotation, Vector3 scale, Color color, int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            ParticlePool particlePool = GetParticlePool(particleType);
            Particle particle = particlePool.GetParticle();

            particle.Initialise(position, rotation, scale, color);
            particle.AssignOnComplete(() => particlePool.ReturnParticle(particle));
            particle.Play();
        }
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
