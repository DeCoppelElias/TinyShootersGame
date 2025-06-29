using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RangedEnemy))]
public class RangedEnemyMovementBehaviour : MovementBehaviour
{
    private RangedEnemy owner;
    private ShootingAbility ownerShootingAbility;

    [Header("Ranged Movement Settings")]
    [SerializeField] private float shootingMoveSpeedReduction = 0.8f;
    [SerializeField] private float movementTargetDistance = 0.8f;

    protected override void Start()
    {
        base.Start();

        owner = GetComponent<RangedEnemy>();
        ownerShootingAbility = owner.GetComponent<ShootingAbility>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (owner != null && owner.GetTargetPlayer() != null)
        {
            Player player = owner.GetTargetPlayer();

            // If player is not shootable => walk towards player
            if (!owner.IsPlayerShootable())
            {
                WalkToPosition(player.transform.position);
            }
            else
            {
                if (Vector3.Distance(this.transform.position, player.transform.position) > movementTargetDistance * ownerShootingAbility.GetRange())
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
}
