using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RangedEnemy))]
public class RangedEnemyMovementBehaviour : MovementBehaviour
{
    private RangedEnemy owner;

    protected override void Start()
    {
        base.Start();

        owner = GetComponent<RangedEnemy>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (owner != null && owner.player != null)
        {
            // Only walk to player if not shootable
            if (!owner.IsPlayerShootable())
            {
                WalkToPosition(owner.player.transform.position);
            }
            else
            {
                StopWalking();
            }
        }
    }
}
