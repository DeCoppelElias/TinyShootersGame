using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] private int particleLimit = 100;
    public enum ParticleType { Damage, Blood, BulletExplosion}

    [SerializeField] public GameObject scoreTextPrefab;

    [SerializeField] private List<ParticlePool> particlePools = new List<ParticlePool>();

    private void Start()
    {
        foreach (ParticlePool particlePool in particlePools)
        {
            particlePool.InitializePool(particleLimit, Mathf.RoundToInt(0.1f * particleLimit));
        }
    }

    public GameObject CreateParticle(ParticleType particleType, Vector3 position, Color color)
    {
        if (particleType == ParticleType.Damage)
        {
            return CreateDamageParticle(position, color);
        }
        else if (particleType == ParticleType.Blood)
        {
            return CreateBloodParticle(position, color);
        }
        else if (particleType == ParticleType.BulletExplosion)
        {
            return CreateBulletExplosion(position, color);
        }
        return null;
    }

    private GameObject CreateDamageParticle(Vector3 position, Color color)
    {
        ParticlePool particlePool = GetParticlePool(ParticleType.Damage);
        DamageParticle particle = (DamageParticle)particlePool.GetParticle();
        if (particle == null) return null;

        particle.Initialise(position, color);
        particle.AssignOnComplete(() => particlePool.ReturnParticle(particle));
        particle.Play();

        return particle.gameObject;
    }

    private GameObject CreateBloodParticle(Vector3 position, Color color)
    {
        ParticlePool particlePool = GetParticlePool(ParticleType.Blood);
        BloodParticle particle = (BloodParticle)particlePool.GetParticle();
        if (particle == null) return null;

        particle.Initialise(position, color);
        particle.AssignOnComplete(() => particlePool.ReturnParticle(particle));
        particle.Play();

        return particle.gameObject;
    }

    private GameObject CreateBulletExplosion(Vector3 position, Color color)
    {
        ParticlePool particlePool = GetParticlePool(ParticleType.BulletExplosion);
        BulletExplosionParticle particle = (BulletExplosionParticle)particlePool.GetParticle();
        if (particle == null) return null;

        particle.Initialise(position, color);
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
