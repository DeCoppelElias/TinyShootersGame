using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy), typeof(DashAbility), typeof(PathFinding))]
public class ChargePlayerEnemyBehaviour : MonoBehaviour
{
    private Enemy owner;
    private DashAbility dashAbility;
    private MovementBehaviour movementBehaviour;
    private PathFinding pathFinding;

    private void Start()
    {
        owner = GetComponent<Enemy>();
        dashAbility = GetComponent<DashAbility>();
        pathFinding = GetComponent<PathFinding>();
        movementBehaviour = GetComponent<MovementBehaviour>();

        dashAbility.SetDashColor(new Color(239 / 255f, 125 / 255f, 87 / 255f));
    }

    private void Update()
    {
        if (owner.GetTargetPlayer() != null)
        {
            Vector3 dashDirection = owner.GetTargetPlayer().transform.position - transform.position;
            if (Vector3.Distance(owner.GetTargetPlayer().transform.position, this.transform.position) < dashAbility.GetDashingDistance() &&
                !pathFinding.IsObstacleInBetween(this.transform.position, owner.GetTargetPlayer().transform.position) && dashAbility.Ready())
            {
                if (movementBehaviour != null) movementBehaviour.EnableMovement(false);
                dashAbility.Dash(dashDirection, () => {
                    if (movementBehaviour != null) movementBehaviour.EnableMovement(true);
                });
            }
        }
    }
}
