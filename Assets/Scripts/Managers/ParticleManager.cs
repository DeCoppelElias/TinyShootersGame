using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] private int particleLimit = 100;
    public enum ParticleType { Damage, Blood}

    [SerializeField] private GameObject damageParticlePrefab;
    [SerializeField] private GameObject bloodParticlePrefab;
    [SerializeField] public GameObject scoreTextPrefab;
    [SerializeField] public Transform particleParent;

    private bool ParticleLimit()
    {
        return particleParent.childCount > particleLimit;
    }

    public GameObject CreateParticle(ParticleType particleType, Vector3 position, Color color)
    {
        if (ParticleLimit()) return null;

        if (particleType == ParticleType.Damage)
        {
            return CreateDamageParticle(position, color);
        }
        else if (particleType == ParticleType.Blood)
        {
            return CreateBloodParticle(position, color);
        }
        return null;
    }

    private GameObject CreateDamageParticle(Vector3 position, Color color)
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        float speed = Random.Range(1f, 3f);
        GameObject pixel = Instantiate(damageParticlePrefab, position, Quaternion.identity, particleParent);
        pixel.GetComponent<DamageParticle>().Initialise(0.5f, dir, speed, color);
        return pixel;
    }

    private GameObject CreateBloodParticle(Vector3 position, Color color)
    {
        Transform bloodParent = new GameObject("BloodPool").transform;
        bloodParent.parent = particleParent;
        int count = Random.Range(5, 10);

        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 0.3f;
            Vector3 spawnPos = position + new Vector3(offset.x, offset.y, 0);

            GameObject pixel = Instantiate(bloodParticlePrefab, spawnPos, Quaternion.identity, bloodParent);
            pixel.GetComponent<BloodParticle>().Initialise(5, 1, color);

            float scale = Random.Range(0.5f, 1.5f);
            pixel.transform.localScale = pixel.transform.localScale * scale;

            float rotationZ = Random.Range(0f, 360f);
            pixel.transform.rotation = Quaternion.Euler(0, 0, rotationZ);
        }

        
        return bloodParent.gameObject;
    }
}
