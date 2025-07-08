using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy movement class that will move randomly for the most part but will sometimes move towards player, for jumpscare effect.
/// </summary>
[RequireComponent(typeof(Enemy))]
public class PseudoRandomMovementBehaviour : MovementBehaviour
{
    private Rigidbody2D rb;
    private float lastMovement;
    [SerializeField] private float minimumMovementCooldown = 1;
    [SerializeField] private float maximumMovementCooldown = 4;
    [SerializeField] private int minimumForceApplied = 500;
    [SerializeField] private int maximumForceApplied = 1500;

    private float movementCooldown;

    private Enemy owner;


    protected override void Start()
    {
        base.Start();

        this.rb = GetComponent<Rigidbody2D>();
        this.movementCooldown = Random.Range(minimumMovementCooldown, maximumMovementCooldown);

        this.owner = GetComponent<Enemy>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (Time.time - this.lastMovement > movementCooldown)
        {
            float r = Random.Range(0, 1f);
            if (r < 0.75f)
            {
                MoveRandomly();
            }
            else
            {
                MoveTowardsPlayer();
            }

            this.lastMovement = Time.time;
            this.movementCooldown = Random.Range(this.minimumMovementCooldown, this.maximumMovementCooldown);
        }
    }

    private void MoveRandomly()
    {
        Vector3 direction = Random.onUnitSphere;

        float counter = 5;
        Vector3 endpoint = this.transform.position + direction * counter;
        while (this.pathFinder.IsObstacleInBetween(this.transform.position, endpoint) && counter <= 0)
        {
            direction = Random.onUnitSphere;
            endpoint = this.transform.position + direction * counter;
            counter -= 0.5f;
        }

        int randomForce = Random.Range(minimumForceApplied, maximumForceApplied);
        rb.AddForce(randomForce * this.moveSpeed * direction);

        
    }

    private void MoveTowardsPlayer()
    {
        int randomForce = Random.Range(minimumForceApplied, maximumForceApplied);
        Vector3 direction = (this.owner.GetTargetPlayer().transform.position - this.transform.position).normalized;
        rb.AddForce(randomForce * this.moveSpeed * direction);
    }
}
