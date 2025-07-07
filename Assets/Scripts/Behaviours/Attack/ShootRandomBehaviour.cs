using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShootingAbility))]
public class ShootRandomBehaviour : MonoBehaviour
{
    private ShootingAbility shootingAbility;

    // Start is called before the first frame update
    void Start()
    {
        shootingAbility = this.GetComponent<ShootingAbility>();
        shootingAbility.SetBulletColor(new Color(239 / 255f, 125 / 255f, 87 / 255f));
    }

    // Update is called once per frame
    void Update()
    {
        shootingAbility.TryShootOnce();
    }
}
