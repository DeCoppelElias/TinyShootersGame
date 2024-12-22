using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ReflectShield : MonoBehaviour
{
    [Header("Reflect Shield Settings")]
    [SerializeField] private Sprite bulletSprite;
    [SerializeField] private string ownerTag;

    private void Start()
    {
        Entity entity = GetComponentInParent<Entity>();
        if (entity != null) ownerTag = entity.tag;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Bullet oldBullet = collision.GetComponent<Bullet>();
        if (oldBullet != null && !oldBullet.hit)
        {
            if (oldBullet.ownerTag != ownerTag)
            {
                Rigidbody2D rbOld = oldBullet.GetComponent<Rigidbody2D>();

                GameObject newBulletGameObject;

                newBulletGameObject = oldBullet.CreateCopyWithNewOwner(ownerTag);
                newBulletGameObject.GetComponent<SpriteRenderer>().sprite = bulletSprite;

                Bullet newBullet = newBulletGameObject.GetComponent<Bullet>();
                Rigidbody2D rbNew = newBulletGameObject.GetComponent<Rigidbody2D>();
                if (!oldBullet.reflected)
                {
                    newBullet.damage *= 2;
                    rbNew.AddForce(-rbOld.velocity * 2, ForceMode2D.Impulse);
                    newBullet.reflected = true;
                }
                else
                {
                    rbNew.AddForce(-rbOld.velocity, ForceMode2D.Impulse);
                }

                Destroy(collision.gameObject);
            }
        }
    }
}
