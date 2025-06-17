using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerup", menuName = "Player/Powerup")]
public class Powerup : ScriptableObject
{
    [Header("General Settings")]
    public string powerupName;

    public float healthDelta = 0;
    public float pvpHealthDelta = 0;

    public float normalMoveSpeedDelta = 0;
    public float shootingMoveSpeedDelta = 0;

    public float invulnerableDurationDelta = 0;

    public float contactDamageDelta = 0;
    public float contactHitCooldownDelta = 0;

    public ClassAbility classAbility;

    [Header("Ranged Combat Settings")]
    public bool hasShootAbility = false;
    public float damageDelta = 0;
    public float attackCooldownDelta = 0;
    public float rangeDelta = 0;
    public float pierceDelta = 0;
    public float totalSplitDelta = 0;
    public float totalFanDelta = 0;
    public float bulletSizeDelta = 0;
    public float bulletSpeedDelta = 0;
    public bool splitOnHit = false;
    public float splitAmountDelta = 0;
    public float splitRangeDelta = 0;
    public float splitBulletSizeDelta = 0;
    public float splitBulletSpeedDelta = 0;
    public float splitDamagePercentageDelta = 0;

    [Header("Dash Settings")]
    public bool hasDashAbility = false;
    public int dashCooldownDelta = 0;
    public float dashDurationDelta = 0;
    public float chargeDurationDelta = 0;
    public float dashSpeedDelta = 0;
}
