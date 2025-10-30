using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ReflectShield : MonoBehaviour
{
    [Header("Reflect Shield Settings")]
    [SerializeField] private Sprite bulletSprite;
    [SerializeField] private string ownerTag;
    private GameObject owner;
    private Color ownerBulletColor = Color.white;

    [SerializeField] private float reflectDamageMultiplier = 2;
    [SerializeField] private float reflectVelocityMultiplier = 2;

    private void Start()
    {
        Entity entity = GetComponentInParent<Entity>();
        if (entity != null)
        {
            ownerTag = entity.tag;
            owner = entity.gameObject;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Bullet oldBullet = collision.GetComponent<Bullet>();
        if (oldBullet != null)
        {
            if ((ownerTag != oldBullet.ownerTag || ownerTag == "Player" && oldBullet.ownerTag == "Player") && !GameObject.ReferenceEquals(owner, oldBullet.owner))
            {
                Bullet reflectedBullet = BulletManager.Instance.TryGetBullet();
                if (reflectedBullet == null) return;
                reflectedBullet.AssignOnComplete(() => BulletManager.Instance.ReturnBullet(reflectedBullet));

                reflectedBullet.reflected = true;

                float reflectedDamage = oldBullet.damage;
                float reflectedVelocity = oldBullet.velocity;
                if (!oldBullet.reflected)
                {
                    reflectedDamage *= reflectDamageMultiplier;
                    reflectedVelocity *= reflectVelocityMultiplier;
                }

                ShootingAbility shootingAbility = GetComponentInParent<ShootingAbility>();
                if (shootingAbility != null) ownerBulletColor = shootingAbility.GetBulletColor();

                float reflectedZ = oldBullet.transform.eulerAngles.z + 180f;
                Quaternion reflectedRotation = Quaternion.Euler(0, 0, reflectedZ);

                reflectedBullet.Initialize(ownerTag, owner, oldBullet.transform.position, reflectedRotation, oldBullet.transform.localScale, reflectedDamage, oldBullet.airTime, reflectedVelocity, oldBullet.pierce, ownerBulletColor);
                if (oldBullet.splitOnHit) reflectedBullet.InitializeSplitting(oldBullet.splitAmount, oldBullet.splitRange, oldBullet.splitBulletSize, oldBullet.splitBulletSpeed, oldBullet.splitDamagePercentage);

                reflectedBullet.Shoot();
                oldBullet.RemoveBullet();
            }
        }
    }
}
