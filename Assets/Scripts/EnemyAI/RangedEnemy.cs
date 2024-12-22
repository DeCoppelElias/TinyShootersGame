using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShootingAbility))]
public class RangedEnemy : Enemy
{
    private float range;
    private SpriteRenderer enemySprite;
    private ShootingAbility shootingAbility;

    private bool playerInRange = false;
    private float startPlayerInRange = 0;
    private float aimingDuration = 0.5f;
    public override void StartEntity()
    {
        base.StartEntity();

        shootingAbility = GetComponent<ShootingAbility>();
        range = shootingAbility.range;
        enemySprite = transform.Find("Sprite").GetComponent<SpriteRenderer>();
    }

    private void LookAtPlayer()
    {
        Vector2 lookDir = (player.transform.position - gameObject.transform.position).normalized;
        enemySprite.transform.rotation = Quaternion.LookRotation(Vector3.forward, lookDir);
    }

    public override void UpdateEntity()
    {
        base.UpdateEntity();

        LookAtPlayer();

        // Shoot at player
        if (IsPlayerShootable())
        {
            if (playerInRange && Time.time - startPlayerInRange > aimingDuration)
            {
                shootingAbility.TryShootOnce();
            }
            else if (!playerInRange)
            {
                playerInRange = true;
                startPlayerInRange = Time.time;
            }
        }
        else
        {
            playerInRange = false;
        }
    }

    public float GetRange()
    {
        return range;
    }

    public bool IsPlayerShootable()
    {
        if (player == null) return false;
        if (Vector3.Distance(this.transform.position, player.transform.position) > range) return false;

        Vector3 to = player.transform.position;
        Vector3 from = this.transform.position;

        Vector3 raycastDirection = (to - from).normalized;
        RaycastHit2D[] rays = Physics2D.RaycastAll(from, raycastDirection, Vector3.Distance(from, to));
        if (RaycastContainsWall(rays)) return false;

        // Perpendicular vectors
        Vector3 perpendicular1 = new Vector3(-raycastDirection.y, raycastDirection.x, raycastDirection.z);
        Vector3 perpendicular2 = new Vector3(raycastDirection.y, -raycastDirection.x, raycastDirection.z);

        Vector3 newFrom1 = from + (0.2f * size * perpendicular1);
        raycastDirection = (to - newFrom1).normalized;
        rays = Physics2D.RaycastAll(newFrom1, raycastDirection, Vector3.Distance(newFrom1, to));
        if (RaycastContainsWall(rays)) return false;

        Vector3 newFrom2 = from + (0.2f * size * perpendicular2);
        raycastDirection = (to - newFrom2).normalized;
        rays = Physics2D.RaycastAll(newFrom2, raycastDirection, Vector3.Distance(newFrom2, to));
        if (RaycastContainsWall(rays)) return false;

        return true;
    }

    protected bool RaycastContainsWall(RaycastHit2D[] rays)
    {
        foreach (RaycastHit2D ray in rays)
        {
            if (ray.transform.CompareTag("Wall"))
            {
                return true;
            }
        }
        return false;
    }
}
