using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitMeleeEnemy : MeleeEnemy
{
    public float splitAmount = 0;
    public override void OnDeath()
    {
        GetComponent<SplitAbility>().Split((targetPlayer.transform.position - transform.position).normalized);
        base.OnDeath();
    }
}
