using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltimateBoss : Enemy
{
    public float lastUse = 0;
    public float currentCooldown = 3;
    public enum Phase {Charging,Dashing,MeleeAttack,RangedAttack,SpawningEnemies,Cooldown}
    public Phase phase = Phase.Cooldown;

    private void FixedUpdate()
    {
        if(phase == Phase.Cooldown && Time.time > lastUse + currentCooldown)
        {

        }
    }
}
