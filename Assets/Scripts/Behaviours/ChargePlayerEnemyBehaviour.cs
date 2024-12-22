using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy), typeof(DashAbility))]
public class ChargePlayerEnemyBehaviour : MonoBehaviour
{
    private Enemy owner;
    private DashAbility dashAbility;
    private MovementBehaviour movementBehaviour;

    private void Start()
    {
        owner = GetComponent<Enemy>();
        dashAbility = GetComponent<DashAbility>();
        movementBehaviour = GetComponent<MovementBehaviour>();
    }

    private void Update()
    {
        if (owner.player != null)
        {
            Vector3 dashDirection = owner.player.transform.position - transform.position;
            if (Vector3.Distance(owner.player.transform.position, this.transform.position) < dashAbility.GetDashingDistance() &&
                owner.IsPositionDirectlyReachable(this.transform.position, owner.player.transform.position) && dashAbility.Ready())
            {
                if (movementBehaviour != null) movementBehaviour.EnableMovement(false);
                dashAbility.Dash(dashDirection, () => {
                    if (movementBehaviour != null) movementBehaviour.EnableMovement(true);
                });
            }
        }
    }
}
