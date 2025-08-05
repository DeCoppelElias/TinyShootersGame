using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewHeavyAbility", menuName = "ClassAbilities/HeavyAbility")]
public class HeavyAbility : ClassAbility
{
    private ShootingAbility shootingAbility;
    private PlayerMovement playerMovement;
    public override void PerformAbility(Player player)
    {
        shootingAbility.ShootBullet(10, 2, 5, 10, shootingAbility.GetDamage() * 2, 5);

        // Add screen shake after use
        if (CameraManager.Instance != null) CameraManager.Instance.ShakeScreen();
    }

    public override bool Initialise(Player player)
    {
        this.shootingAbility = player.GetComponent<ShootingAbility>();
        this.playerMovement = player.GetComponent<PlayerMovement>();

        if (shootingAbility == null || playerMovement == null) return false;
        return true;
    }
}
