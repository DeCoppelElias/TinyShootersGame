using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShootingAbility))]
public class LittleGunner : MonoBehaviour
{
    private ShootingAbility shootingAbility;
    private Entity target;

    public Entity owner;
    private float lastRefresh = 0;
    private float refreshCooldown = 1;

    // Start is called before the first frame update
    void Start()
    {
        shootingAbility = this.GetComponent<ShootingAbility>();
    }

    public void SetOwner(Entity entity)
    {
        owner = entity;
        shootingAbility = this.GetComponent<ShootingAbility>();
        shootingAbility.damage = entity.GetComponent<ShootingAbility>().damage / 2;
        shootingAbility.owner = owner;
    }

    // Update is called once per frame
    void Update()
    {
        if (owner == null) return;

        if (target != null)
        {
            Vector2 lookDir = (target.transform.position - gameObject.transform.position).normalized;
            this.transform.rotation = Quaternion.LookRotation(Vector3.forward, lookDir);
            shootingAbility.shooting = true;

            if (Time.time - lastRefresh > refreshCooldown)
            {
                lastRefresh = Time.time;
                target = FindTarget();
            }
        }
        else
        {
            shootingAbility.shooting = false;
            lastRefresh = Time.time;
            target = FindTarget();
        }
    }

    private Entity FindTarget()
    {
        Entity[] entities = Object.FindObjectsOfType<Entity>();

        Entity closestPlayer = null;
        float closestDistance = float.MaxValue;

        foreach (Entity entity in entities)
        {
            float distance = Vector3.Distance(this.transform.position, entity.transform.position);
            if (distance < closestDistance && entity.tag != this.tag)
            {
                closestDistance = distance;
                closestPlayer = entity;
            }
        }

        return closestPlayer;
    }
}
