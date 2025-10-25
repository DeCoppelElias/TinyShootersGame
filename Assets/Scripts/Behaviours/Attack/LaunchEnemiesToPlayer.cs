using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Enemy), typeof(Rigidbody2D))]
public class LaunchEnemiesToPlayer : MonoBehaviour
{
    [SerializeField] private int spawnedEnemies = 0;
    [SerializeField] private int maxSpawnedEnemies = 5;

    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private List<float> probabilities;

    [SerializeField] private float launchCooldown = 5;
    [SerializeField] private float launchOffset = 2;
    [SerializeField] private float launchForce = 100;

    private float startTimer;
    private float launchTimer;

    private Enemy owner;
    private float colliderRadius;

    // Start is called before the first frame update
    void Start()
    {
        startTimer = Time.time;
        owner = GetComponent<Enemy>();
        Collider2D collider = GetComponent<Collider2D>();
        colliderRadius = collider.bounds.size.x / 2f;
    }

    // Update is called once per frame
    void Update()
    {
        Player player = owner.GetTargetPlayer();
        if (player == null) return;

        if (Time.time - startTimer > launchOffset && Time.time - launchTimer > launchCooldown && spawnedEnemies < maxSpawnedEnemies)
        {
            launchTimer = Time.time;

            Vector3 launchDirection = (player.transform.position - this.transform.position).normalized;
            Vector3 spawnPoint = this.transform.position + launchDirection * colliderRadius;

            GameObject enemyPrefab = PickRandomEnemyType();
            GameObject enemyGO = Instantiate(enemyPrefab, spawnPoint, Quaternion.identity, this.owner.transform.parent);
            Enemy enemy = enemyGO.GetComponent<Enemy>();
            enemy.onDeathScore = 0;

            Rigidbody2D enemyRB = enemy.GetComponent<Rigidbody2D>();
            enemyRB.AddForce(launchForce * launchDirection, ForceMode2D.Impulse);

            enemy.GetComponent<Enemy>().onDeath.AddListener(() => spawnedEnemies--);
            spawnedEnemies++;
        }
    }

    private GameObject PickRandomEnemyType()
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
