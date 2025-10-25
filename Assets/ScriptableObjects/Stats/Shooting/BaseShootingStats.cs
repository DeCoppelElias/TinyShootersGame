using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Entity/Stats/BaseShootingStats")]
public class BaseShootingStats : ScriptableObject
{
    [SerializeField] private float damage = 5;
    [SerializeField] private float range = 1;
    [SerializeField] private int pierce = 1;
    [SerializeField] private float split = 1;
    [SerializeField] private float fan = 1;
    [SerializeField] private float bulletSize = 1;
    [SerializeField] private float bulletVelocity = 6;
    [SerializeField] private float attackCooldown = 0.5f;

    [SerializeField] private bool explode = false;
    [SerializeField] private int explodeBulletAmount = 0;
    [SerializeField] private float explodeDamagePercentage = 0.5f;
    [SerializeField] private float explodeBulletRange = 1;
    [SerializeField] private float explodeBulletSize = 0.5f;
    [SerializeField] private float explodeBulletVelocity = 6;

    [SerializeField] private float shootingMoveSpeed = 2;

    #region Properties
    public float Damage { get => damage; }
    public float Range { get => range; }
    public int Pierce { get => pierce; }
    public float Split { get => split; }
    public float Fan { get => fan; }
    public float BulletSize { get => bulletSize; }
    public float BulletVelocity { get => bulletVelocity; }
    public float AttackCooldown { get => attackCooldown; }

    public bool Explode { get => explode; }
    public int ExplodeBulletAmount { get => explodeBulletAmount; }
    public float ExplodeDamagePercentage { get => explodeDamagePercentage; }
    public float ExplodeBulletRange { get => explodeBulletRange; }
    public float ExplodeBulletSize { get => explodeBulletSize; }
    public float ExplodeBulletVelocity { get => explodeBulletVelocity; }

    public float ShootingMoveSpeed { get => shootingMoveSpeed; }
    #endregion
}
