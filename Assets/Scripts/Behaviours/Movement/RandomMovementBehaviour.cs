using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovementBehaviour : MovementBehaviour
{
    private Rigidbody2D rb;
    private float lastMovement;
    [SerializeField] private float minimumMovementCooldown = 1;
    [SerializeField] private float maximumMovementCooldown = 4;
    [SerializeField] private int minimumForceApplied = 500;
    [SerializeField] private int maximumForceApplied = 1500;

    private float movementCooldown;


    protected override void Start()
    {
        base.Start();

        this.rb = GetComponent<Rigidbody2D>();
        this.movementCooldown = Random.Range(minimumMovementCooldown, maximumMovementCooldown);
    }
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (Time.time - this.lastMovement > movementCooldown)
        {
            this.MoveRandomly();
        }
    }

    private void MoveRandomly()
    {
        Vector3 direction = Random.onUnitSphere;
        int counter = 0;
        while (this.pathFinder.InvalidPosition(this.transform.position + direction) && counter < 5)
        {
            counter++;
            direction = Random.onUnitSphere;
        }

        int randomForce = Random.Range(minimumForceApplied, maximumForceApplied);
        rb.AddForce(randomForce * this.moveSpeed * direction);

        this.lastMovement = Time.time;
        this.movementCooldown = Random.Range(this.minimumMovementCooldown, this.maximumMovementCooldown);
    }
}
