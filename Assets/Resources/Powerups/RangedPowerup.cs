using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedPowerup : ScriptableObject
{
    [Header("Minimum, Maximum, Delta")]
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
