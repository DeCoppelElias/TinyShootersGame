using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewHeavyAbility", menuName = "ClassAbilities/HeavyAbility")]
public class HeavyAbility : ClassAbility
{
    private ShootingAbility shootingAbility;
    private PlayerMovement playerMovement;
    private CameraManager cameraManager;
    public override void PerformAbility(Player player)
    {
        shootingAbility.ShootBullet(10, 2, 5, 10, shootingAbility.damage * 2);

        // Add knockback after shooting
        Vector3 knockbackDirection = (player.transform.position - shootingAbility.firePoint.position).normalized;
        playerMovement.ApplyKnockBack(knockbackDirection, 8, 1.5f);

        // Add screen shake after use
        if (this.cameraManager != null) cameraManager.ShakeScreen();
    }

    public override bool Initialise(Player player)
    {
        this.shootingAbility = player.GetComponent<ShootingAbility>();
        this.playerMovement = player.GetComponent<PlayerMovement>();

        GameObject cameraManagerObj = GameObject.Find("CameraManager");
        if (cameraManagerObj != null) this.cameraManager = cameraManagerObj.GetComponent<CameraManager>();

        if (shootingAbility == null || playerMovement == null) return false;
        return true;
    }
}
