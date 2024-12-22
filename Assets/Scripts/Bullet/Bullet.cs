using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Bullet : MonoBehaviour
{
    public float pierce;
    public float damage;
    public float airTime;
    public float createTime;
    public bool hit = false;

    public bool splitOnHit = false;
    public float splitAmount = 0;
    public float splitRange = 1;
    public float splitBulletSize = 0.5f;
    public float splitBulletSpeed = 6;
    public float splitDamagePercentage = 0.5f;

    public bool reflected = false;

    public string ownerTag;

    private void Update()
    {
        if(!hit && Time.time > createTime + airTime)
        {
            BulletMiss();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Wall")
        {
            BulletHit();
            return;
        }

        Entity entity = collision.GetComponent<Entity>();
        if (entity != null && ownerTag != collision.tag && entity.health > 0)
        {
            entity.TakeDamage(damage, ownerTag, Entity.DamageType.Ranged);
            pierce--;
            if (pierce == 0)
            {
                BulletHit();
            }
        }
    }

    public void BulletHit()
    {
        if (splitOnHit)
        {
            Split();
        }

        hit = true;
        Destroy(gameObject);
    }

    public void BulletMiss()
    {
        Split();
        Destroy(gameObject);
    }
    public void Split()
    {
        if (splitAmount > 0)
        {
            float angleInterval = 360 / splitAmount;
            float currentAngle = 0;
            for (int i = 0; i < splitAmount; i++)
            {
                CreateSplitBullet(currentAngle);
                currentAngle += angleInterval;
            }
        }
    }
    public void CreateSplitBullet(float currentAngle)
    {
        Vector3 vector = Quaternion.Euler(0, 0, currentAngle) * Vector3.up;
        GameObject bullet = Instantiate(gameObject, transform.position + vector/3, Quaternion.identity,transform.parent);
        bullet.transform.localScale = new Vector3(splitBulletSize, splitBulletSize,1);

        bullet.GetComponent<Bullet>().pierce = 1;
        bullet.GetComponent<Bullet>().damage = damage * splitDamagePercentage;
        bullet.GetComponent<Bullet>().ownerTag = ownerTag;
        bullet.GetComponent<Bullet>().airTime = splitRange / splitBulletSpeed;
        bullet.GetComponent<Bullet>().createTime = Time.time;

        bullet.GetComponent<Bullet>().splitAmount = 0;

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        rb.AddForce(vector * splitBulletSpeed, ForceMode2D.Impulse);
    }

    public GameObject CreateCopyWithNewOwner(string ownerTag)
    {
        GameObject newBulletGameObject = Instantiate(gameObject, transform.position, Quaternion.identity, transform.parent);
        newBulletGameObject.GetComponent<Bullet>().createTime = Time.time;
        newBulletGameObject.GetComponent<Bullet>().ownerTag = ownerTag;
        newBulletGameObject.GetComponent<Bullet>().reflected = reflected;

        return newBulletGameObject;
    }
}
 