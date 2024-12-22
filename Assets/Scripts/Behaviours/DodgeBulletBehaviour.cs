using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DashAbility))]
public class DodgeBulletBehaviour : MonoBehaviour
{
    private MovementBehaviour movementBehaviour;
    private DashAbility dashAbility;

    [Header("Dodge Settings")]
    [SerializeField] private int dodgeAngle = 90;

    [Header("Trigger Settings")]
    [SerializeField] private bool customBulletTrigger = false;
    [SerializeField] private BulletTrigger bulletTrigger;

    private void Start()
    {
        movementBehaviour = GetComponent<MovementBehaviour>();
        dashAbility = GetComponent<DashAbility>();

        if (!customBulletTrigger) this.bulletTrigger = GetComponentInChildren<BulletTrigger>();

        if (bulletTrigger != null) bulletTrigger.AddTriggerAction(DodgeBullet);
    }

    private void DodgeBullet(Vector3 bulletDirection)
    {
        if (!dashAbility.Ready()) return;

        if (movementBehaviour != null) movementBehaviour.EnableMovement(false);
        Vector3 dashDirection = ChooseDashDirection(bulletDirection);
        dashAbility.Dash(dashDirection, () => {
            if (movementBehaviour != null) movementBehaviour.EnableMovement(true);
        });
    }

    private Vector3 ChooseDashDirection(Vector3 bulletDirection)
    {
        int r = Random.Range(0, 2);
        if (r == 0)
        {
            return (Quaternion.Euler(0, 0, dodgeAngle) * bulletDirection).normalized;
        }
        else
        {
            return (Quaternion.Euler(0, 0, -dodgeAngle) * bulletDirection).normalized;
        }
    }
}
