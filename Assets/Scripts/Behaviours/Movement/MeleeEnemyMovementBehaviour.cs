using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class MeleeEnemyMovementBehaviour : MovementBehaviour
{
    private Enemy owner;

    protected override void Start()
    {
        base.Start();

        owner = GetComponent<Enemy>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (owner != null && owner.player != null) WalkToPosition(owner.player.transform.position);
    }
}
