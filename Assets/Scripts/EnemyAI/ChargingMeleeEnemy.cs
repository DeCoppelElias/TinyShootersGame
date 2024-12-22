using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DashAbility))]
public class ChargingMeleeEnemy : Enemy
{
    private DashAbility dashAbility;

    public override void StartEntity()
    {
        base.StartEntity();

        dashAbility = GetComponent<DashAbility>();
    }

    public override void UpdateEntity()
    {
        base.UpdateEntity();

        Vector3 raycastDirection = player.transform.position - transform.position;
        RaycastHit2D[] rays = Physics2D.RaycastAll(transform.position, raycastDirection, Vector2.Distance(transform.position, player.transform.position));
        /*if (Vector2.Distance(gameObject.transform.position, player.transform.position) < dashAbility.GetDashingDistance() && !RaycastContainsObstacle(rays) && dashAbility.dashingState == DashAbility.DashingState.Ready)
        {
            Vector3 dashDirection = player.transform.position - transform.position;
            dashAbility.Dash(dashDirection);
        }
        else if (dashAbility.dashingState != DashAbility.DashingState.Charging && dashAbility.dashingState != DashAbility.DashingState.Dashing)
        {
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, step);
        }*/
    }
}
