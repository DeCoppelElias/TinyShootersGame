using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePool : MonoBehaviour
{
    [SerializeField] private ParticleManager.ParticleType poolType;
    [SerializeField] private GameObject particlePrefab;
    private int particleLimit;
    [SerializeField] private int particleCount = 0;

    private Queue<Particle> availableParticles = new Queue<Particle>();
    private bool initialized = false;

    public void InitializePool(int particleLimit, int initialAmount)
    {
        this.particleLimit = particleLimit;
        this.particleCount = 0;

        for (int i = 0; i < initialAmount; i++)
        {
            SpawnParticle();
        }

        initialized = true;
    }

    public Particle GetParticle()
    {
        if (!initialized) return null;

        if (availableParticles.Count > 0)
        {
            Particle particle = availableParticles.Dequeue();
            particle.gameObject.SetActive(true);
            return particle;
        }
        else
        {
            return SpawnParticle();
        }
    }

    public void ReturnParticle(Particle particle)
    {
        if (particle == null || !initialized) return;

        availableParticles.Enqueue(particle);
        particle.gameObject.SetActive(false);
    }

    private Particle SpawnParticle()
    {
        if (particleCount > particleLimit) return null;

        GameObject particleGO = Instantiate(particlePrefab, new Vector3(0,0,0), Quaternion.identity, this.transform);
        Particle particle = particleGO.GetComponent<Particle>();

        availableParticles.Enqueue(particle);
        particleGO.SetActive(false);

        particleCount++;

        return particle;
    }

    public ParticleManager.ParticleType GetPoolType()
    {
        return this.poolType;
    }
}
