using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy), typeof(ShootingAbility))]
public class ShootPlayerEnemyBehaviour : MonoBehaviour
{
    private SpriteRenderer enemySprite;
    private ShootingAbility shootingAbility;

    private Enemy owner;

    private bool playerInRange = false;
    private float startPlayerInRange = 0;
    private float aimingDuration = 0.5f;

    private void Start()
    {
        shootingAbility = this.GetComponent<ShootingAbility>();
        shootingAbility.SetBulletColor(new Color(239 / 255f, 125 / 255f, 87 / 255f));

        owner = this.GetComponent<Enemy>();
        enemySprite = transform.Find("Sprite").GetComponent<SpriteRenderer>();
    }

    private void LookAtPlayer()
    {
        Player targetPlayer = owner.GetTargetPlayer();
        if (targetPlayer == null) return;

        Vector2 lookDir = (targetPlayer.transform.position - gameObject.transform.position).normalized;
        enemySprite.transform.rotation = Quaternion.LookRotation(Vector3.forward, lookDir);
    }

    private void ShootPlayer()
    {
        Player targetPlayer = owner.GetTargetPlayer();
        if (targetPlayer == null) return;

        if (this.shootingAbility.IsShootable(this.transform.position, targetPlayer.transform.position))
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

    private void Update()
    {
        LookAtPlayer();
        ShootPlayer();
    }
}
