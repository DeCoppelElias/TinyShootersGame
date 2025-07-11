using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShootingAbility))]
public class ShootRandomBehaviour : MonoBehaviour
{
    private ShootingAbility shootingAbility;

    private float ShootOffset = 3.2f;
    private float ShootCooldown = 2;

    private float lastShotTiming;
    private float startTiming;

    // Start is called before the first frame update
    void Start()
    {
        shootingAbility = this.GetComponent<ShootingAbility>();
        shootingAbility.SetBulletColor(new Color(239 / 255f, 125 / 255f, 87 / 255f));

        startTiming = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - startTiming > ShootOffset && Time.time - lastShotTiming > ShootCooldown)
        {
            shootingAbility.TryShootOnce();
            lastShotTiming = Time.time;
        }
    }
}
