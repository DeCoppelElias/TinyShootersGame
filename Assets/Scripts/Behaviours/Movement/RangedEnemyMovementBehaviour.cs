using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy), typeof(ShootingAbility))]
public class RangedEnemyMovementBehaviour : MovementBehaviour
{
    private Enemy owner;
    private ShootingAbility shootingAbility;

    [Header("Ranged Movement Settings")]
    [SerializeField] private float shootingMoveSpeedReduction = 0.8f;
    [SerializeField] private float movementTargetDistance = 0.8f;

    protected override void Start()
    {
        base.Start();

        owner = GetComponent<Enemy>();
        shootingAbility = GetComponent<ShootingAbility>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (owner == null) return;
        if (shootingAbility == null) return;

        Player player = owner.GetTargetPlayer();
        if (player == null) return;

        // If player is not shootable => walk towards player
        if (!shootingAbility.IsShootable(this.transform.position, player.transform.position))
        {
            WalkToPosition(player.transform.position);
        }
        else
        {
            if (Vector3.Distance(this.transform.position, player.transform.position) > movementTargetDistance * shootingAbility.GetRange())
            {
                WalkToPosition(player.transform.position, shootingMoveSpeedReduction);
            }
            else
            {
                StopWalking();
            }
        }
    }
}
