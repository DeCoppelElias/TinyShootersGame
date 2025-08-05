using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is the latest version of the class system in Tiny Shooters. 
/// Instead of forcing set values when a player selects a class, I want to define how much is added or subtracted from player stats. 
/// This is the first step in allowing more random power updates inbetween levels, allowing for more unique combinations.
/// </summary>
[CreateAssetMenu(fileName = "NewPlayerClass", menuName = "Player/PlayerClass")]
public class PlayerClass : ScriptableObject
{
    [Header("General Settings")]
    public string className;
    [TextArea(15, 20)]
    public string classDescription;
    public RuntimeAnimatorController animatorController;
    public Sprite UISprite;

    public float healthDelta = 0;

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
    public int pierceDelta = 0;
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

    [Header("Upgrades")]
    public List<PlayerClass> upgrades = new List<PlayerClass>();
}
