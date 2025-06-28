using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShootingAbility))]
public class LittleGunner : MonoBehaviour
{
    private ShootingAbility gunnerShootingAbility;
    [SerializeField] private Entity target;

    [SerializeField] private Entity owner;
    private float lastRefresh = 0;
    private float refreshCooldown = 1;

    // Start is called before the first frame update
    void Start()
    {
        gunnerShootingAbility = this.GetComponent<ShootingAbility>();
    }

    public void SetOwner(Entity entity)
    {
        this.owner = entity;

        gunnerShootingAbility = this.GetComponent<ShootingAbility>();
        ShootingAbility ownerShootingAbility = entity.GetComponent<ShootingAbility>();

        RuntimeShootingStats ownerShootingStats = ownerShootingAbility.RuntimeStats;
        ownerShootingStats.Damage /= 2f;
        gunnerShootingAbility.ApplyShootingStats(ownerShootingStats);
        gunnerShootingAbility.owner = entity;

        this.tag = entity.tag;
    }

    // Update is called once per frame
    void Update()
    {
        if (owner == null) return;

        if (target != null)
        {
            Vector2 lookDir = (target.transform.position - gameObject.transform.position).normalized;
            this.transform.rotation = Quaternion.LookRotation(Vector3.forward, lookDir);
            gunnerShootingAbility.shooting = true;

            if (Time.time - lastRefresh > refreshCooldown)
            {
                lastRefresh = Time.time;
                target = FindTarget();
            }
        }
        else
        {
            gunnerShootingAbility.shooting = false;
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
