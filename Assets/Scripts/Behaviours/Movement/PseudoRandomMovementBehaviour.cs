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

    private float movementOffset = 0.2f;
    private float startTiming;

    [SerializeField] private float minimumMovementCooldown = 1;
    [SerializeField] private float maximumMovementCooldown = 4;
    [SerializeField] private int minimumForceApplied = 500;
    [SerializeField] private int maximumForceApplied = 1500;

    private float movementCooldown;

    private Enemy owner;

    [SerializeField] private bool canSeePlayer = true;
    [SerializeField] private float lastPlayerVisionCheck;
    [SerializeField] private float playerVisionCooldown = 0.5f;

    protected override void Start()
    {
        base.Start();

        this.rb = GetComponent<Rigidbody2D>();
        this.movementCooldown = Random.Range(minimumMovementCooldown, maximumMovementCooldown);

        this.owner = GetComponent<Enemy>();

        startTiming = Time.time;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        Player player = this.owner.GetTargetPlayer();
        if (player == null) MoveRandomly();

        if (Time.time - lastMovement > movementCooldown && Time.time - startTiming > movementOffset)
        {
            if (!canSeePlayer) MoveRandomly();
            else
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
            }

            this.lastMovement = Time.time;
            this.movementCooldown = Random.Range(this.minimumMovementCooldown, this.maximumMovementCooldown);
        }

        if (Time.time - lastPlayerVisionCheck > playerVisionCooldown)
        {
            lastPlayerVisionCheck = Time.time;
            canSeePlayer = !pathFinder.IsObstacleInBetween(transform.position, player.transform.position);
        }
    }

    private void MoveRandomly()
    {
        Vector2 direction = Random.insideUnitCircle.normalized;
        Vector3 direction3D = new Vector3(direction.x, direction.y, 0);

        float counter = 10;
        Vector3 endpoint = this.transform.position + direction3D * counter;
        while (this.pathFinder.IsObstacleInBetween(this.transform.position, endpoint) && counter > 0)
        {
            direction = Random.insideUnitCircle.normalized;
            direction3D = new Vector3(direction.x, direction.y, 0);
            endpoint = this.transform.position + direction3D * counter;
            counter -= 0.5f;
        }

        int randomForce = Random.Range(minimumForceApplied, maximumForceApplied);
        rb.AddForce(randomForce * this.moveSpeed * direction);
    }

    private void MoveTowardsPlayer()
    {
        Player player = this.owner.GetTargetPlayer();
        if (player == null) return;

        int randomForce = Random.Range(minimumForceApplied, maximumForceApplied);
        Vector3 direction = (player.transform.position - this.transform.position).normalized;
        rb.AddForce(randomForce * this.moveSpeed * direction);
    }
}
