using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    public Vector3 bulletDirection;
    public bool bulletTrigger;

    private enum MovementState { Normal, DodgingObstacle, NoPathToPosition}
    [SerializeField] private MovementState movementState = MovementState.Normal;
    private PathFinding pathFinder;
    private List<Vector3> dodgeObstaclePath;
    private float lastPathFindingRefresh = 0;
    private float pathFindingCooldown = 3f;
    protected float size = 0;

    public Player player;

    private float refreshPlayerTargetCooldown = 2;
    private float lastPlayerRefresh = 0;

    [Header("Debug Settings")]
    [SerializeField] private bool debug = false;

    public override void StartEntity()
    {
        base.StartEntity();

        player = FindClosestPlayer();
        pathFinder = GetComponent<PathFinding>();

        Collider2D collider = GetComponentInChildren<Collider2D>();
        if (collider != null)
        {
            size = collider.bounds.size.x;
        }
    }

    private Player FindClosestPlayer()
    {
        lastPlayerRefresh = Time.time;
        Player[] players = Object.FindObjectsOfType<Player>();

        Player closestPlayer = null;
        float closestDistance = float.MaxValue;

        foreach (Player player in players)
        {
            float distance = Vector3.Distance(this.transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

    public override void UpdateEntity()
    {
        base.UpdateEntity();

        if (Time.time - lastPlayerRefresh > refreshPlayerTargetCooldown)
        {
            this.player = FindClosestPlayer();
        }
    }

    public void WalkToPlayerUpdate()
    {
        if (player == null) return;
        WalkToPositionUpdate(player.transform.position);
    }

    public void WalkToPositionUpdate(Vector3 targetPosition)
    {
        float step = moveSpeed * Time.deltaTime;
        // If there are no obstacles between enemy and player, move towards player
        if (movementState == MovementState.Normal)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, step);

            // If there is an obstacle blocking the way, try to find a path to player and change state
            if (IsObstacleInDistance(transform.position, targetPosition))
            {
                dodgeObstaclePath = pathFinder.FindShortestPath(transform.position, targetPosition);
                lastPathFindingRefresh = Time.time;

                // There is an existing path
                if (dodgeObstaclePath.Count > 0)
                {
                    dodgeObstaclePath = pathFinder.SmoothRoute(dodgeObstaclePath);
                    if (debug) pathFinder.DisplayRoute(dodgeObstaclePath);
                    movementState = MovementState.DodgingObstacle;
                }
                // There exists no path
                else
                {
                    movementState = MovementState.NoPathToPosition;
                }
            }
        }
        else if (movementState == MovementState.DodgingObstacle)
        {
            // If path is empty or there are no obstacles to the player, then return no normal state
            if (dodgeObstaclePath.Count == 0 || IsPositionDirectlyReachable(transform.position, targetPosition))
            {
                dodgeObstaclePath.Clear();
                movementState = MovementState.Normal;
            }
            // Otherwise, move to next point in route
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, dodgeObstaclePath[0], step);
                if (Vector3.Distance(transform.position, dodgeObstaclePath[0]) <= 0.2f * size)
                {
                    dodgeObstaclePath.RemoveAt(0);
                }

                // Refresh route every few seconds
                if (Time.time - lastPathFindingRefresh > pathFindingCooldown)
                {
                    dodgeObstaclePath = pathFinder.FindShortestPath(transform.position, targetPosition);
                    lastPathFindingRefresh = Time.time;

                    // There is an existing path
                    if (dodgeObstaclePath.Count > 0)
                    {
                        dodgeObstaclePath = pathFinder.SmoothRoute(dodgeObstaclePath);
                        if (debug) pathFinder.DisplayRoute(dodgeObstaclePath);
                    }
                    // There exists no path
                    else
                    {
                        movementState = MovementState.NoPathToPosition;
                    }
                }
            }
        }
        else if (movementState == MovementState.NoPathToPosition)
        {
            if (Time.time - lastPathFindingRefresh > pathFindingCooldown)
            {
                if (IsPositionDirectlyReachable(transform.position, targetPosition))
                {
                    dodgeObstaclePath.Clear();
                    movementState = MovementState.Normal;
                }
                else
                {
                    dodgeObstaclePath = pathFinder.FindShortestPath(transform.position, targetPosition);
                    lastPathFindingRefresh = Time.time;

                    // There is an existing path
                    if (dodgeObstaclePath.Count > 0)
                    {
                        dodgeObstaclePath = pathFinder.SmoothRoute(dodgeObstaclePath);
                        if (debug) pathFinder.DisplayRoute(dodgeObstaclePath);
                    }
                }
            }
        }
    }

    public bool RaycastContainsObstacle(RaycastHit2D[] rays)
    {
        foreach (RaycastHit2D ray in rays)
        {
            if (ray.transform.CompareTag("Wall") || ray.transform.CompareTag("Pit"))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsPositionDirectlyReachable(Vector3 from, Vector3 to)
    {
        Vector3 raycastDirection = (to - from).normalized;
        RaycastHit2D[] rays = Physics2D.RaycastAll(from, raycastDirection, Vector3.Distance(from, to));
        if (RaycastContainsObstacle(rays)) return false;

        // Perpendicular vectors
        Vector3 perpendicular1 = new Vector3(-raycastDirection.y, raycastDirection.x, raycastDirection.z);
        Vector3 perpendicular2 = new Vector3(raycastDirection.y, -raycastDirection.x, raycastDirection.z);

        Vector3 newFrom1 = from + (0.2f * size * perpendicular1);
        raycastDirection = (to - newFrom1).normalized;
        rays = Physics2D.RaycastAll(newFrom1, raycastDirection, Vector3.Distance(newFrom1, to));
        if (RaycastContainsObstacle(rays)) return false;

        Vector3 newFrom2 = from + (0.2f * size * perpendicular2);
        raycastDirection = (to - newFrom2).normalized;
        rays = Physics2D.RaycastAll(newFrom2, raycastDirection, Vector3.Distance(newFrom2, to));
        if (RaycastContainsObstacle(rays)) return false;

        return true;
    }

    private bool IsObstacleInDistance(Vector3 from, Vector3 to, float distance = 1)
    {
        Vector3 raycastDirection = (to - from).normalized;
        RaycastHit2D[] rays = Physics2D.RaycastAll(from, raycastDirection, distance);
        if (RaycastContainsObstacle(rays)) return true;

        // Perpendicular vectors
        Vector3 perpendicular1 = new Vector3(-raycastDirection.y, raycastDirection.x, raycastDirection.z);
        Vector3 perpendicular2 = new Vector3(raycastDirection.y, -raycastDirection.x, raycastDirection.z);

        Vector3 newFrom1 = from + (0.2f * size * perpendicular1);
        raycastDirection = (to - newFrom1).normalized;
        rays = Physics2D.RaycastAll(newFrom1, raycastDirection, distance);
        if (RaycastContainsObstacle(rays)) return true;

        Vector3 newFrom2 = from + (0.2f * size * perpendicular2);
        raycastDirection = (to - newFrom2).normalized;
        rays = Physics2D.RaycastAll(newFrom2, raycastDirection, distance);
        if (RaycastContainsObstacle(rays)) return true;

        return false;
    }
}
