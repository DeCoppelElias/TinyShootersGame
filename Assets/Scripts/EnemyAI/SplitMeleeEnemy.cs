using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitMeleeEnemy : MeleeEnemy
{
    public int splitAmount = 0;
    public override void OnDeath()
    {
        bool split = GetComponent<SplitAbility>().Split((targetPlayer.transform.position - transform.position).normalized);
        this.spawnBody = !split;

        base.OnDeath();
    }
}
