using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy), typeof(Rigidbody2D))]
public class LaunchEnemiesToPlayer : MonoBehaviour
{
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private List<float> probabilities;

    [SerializeField] private float launchCooldown = 5;
    [SerializeField] private float launchOffset = 2;
    [SerializeField] private float launchForce = 100;

    private float startTimer;
    private float launchTimer;

    private WaveManager waveManager;
    private Enemy owner;
    private float colliderRadius;

    // Start is called before the first frame update
    void Start()
    {
        startTimer = Time.time;
        waveManager = GameObject.Find("WaveManager").GetComponent<WaveManager>();
        owner = GetComponent<Enemy>();
        Collider2D collider = GetComponent<Collider2D>();
        colliderRadius = collider.bounds.size.x / 2f;
    }

    // Update is called once per frame
    void Update()
    {
        Player player = owner.GetTargetPlayer();
        if (player == null) return;

        if (Time.time - startTimer > launchOffset && Time.time - launchTimer > launchCooldown)
        {
            launchTimer = Time.time;

            Vector3 launchDirection = (player.transform.position - this.transform.position).normalized;
            Vector3 spawnPoint = this.transform.position + launchDirection * colliderRadius;

            GameObject prefab = pickRandomPrefab();
            GameObject enemy = waveManager.CreateEnemy(prefab, spawnPoint);
            Rigidbody2D enemyRB = enemy.GetComponent<Rigidbody2D>();
            enemyRB.AddForce(launchForce * launchDirection, ForceMode2D.Impulse);
        }
    }

    private GameObject pickRandomPrefab()
    {
        float r = Random.Range(0, 1f);
        for (int i = 0; i < this.probabilities.Count; i++)
        {
            if (r < this.probabilities[i])
            {
                return this.enemyPrefabs[i];
            }
            else
            {
                r -= this.probabilities[i];
            }
        }

        return null;
    }
}
