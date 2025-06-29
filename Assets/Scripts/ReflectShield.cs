using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ReflectShield : MonoBehaviour
{
    [Header("Reflect Shield Settings")]
    [SerializeField] private Sprite bulletSprite;
    [SerializeField] private string ownerTag;
    private Color ownerBulletColor = Color.white;

    private BulletManager bulletManager;

    private void Start()
    {
        Entity entity = GetComponentInParent<Entity>();
        if (entity != null) ownerTag = entity.tag;

        bulletManager = GameObject.Find("Bullets").GetComponent<BulletManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Bullet oldBullet = collision.GetComponent<Bullet>();
        if (oldBullet != null)
        {
            if (oldBullet.ownerTag != ownerTag)
            {
                Bullet reflectedBullet = bulletManager.TryGetBullet();
                if (reflectedBullet == null) return;
                reflectedBullet.AssignOnComplete(() => bulletManager.ReturnBullet(reflectedBullet));

                float reflectedDamage = oldBullet.damage;
                float reflectedVelocity = oldBullet.velocity;
                if (!oldBullet.reflected)
                {
                    reflectedDamage *= 2;
                    reflectedVelocity *= 2;
                }

                ShootingAbility shootingAbility = GetComponentInParent<ShootingAbility>();
                if (shootingAbility != null) ownerBulletColor = shootingAbility.GetBulletColor();

                float reflectedZ = oldBullet.transform.eulerAngles.z + 180f;
                Quaternion reflectedRotation = Quaternion.Euler(0, 0, reflectedZ);

                reflectedBullet.Initialize(ownerTag, oldBullet.transform.position, reflectedRotation, oldBullet.transform.localScale, reflectedDamage, oldBullet.airTime, reflectedVelocity, oldBullet.pierce, ownerBulletColor);
                if (oldBullet.splitOnHit) reflectedBullet.InitializeSplitting(oldBullet.splitAmount, oldBullet.splitRange, oldBullet.splitBulletSize, oldBullet.splitBulletSpeed, oldBullet.splitDamagePercentage);

                reflectedBullet.Shoot();
                oldBullet.RemoveBullet();
            }
        }
    }
}
