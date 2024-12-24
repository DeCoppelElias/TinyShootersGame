using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathFinding))]
public abstract class MovementBehaviour : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2;
    [SerializeField] private float currentMoveSpeed;
    private enum MovementBehaviourState { Enabled, WalkingToPosition, Disabled}
    [SerializeField] private MovementBehaviourState movementBehaviourState = MovementBehaviourState.Enabled;

    [Header("Walking To Position State")]
    [SerializeField] private Vector3 targetPosition;
    private enum WalkToPositionState { Normal, DodgingObstacle, NoPathToPosition }
    [SerializeField] private WalkToPositionState walkToPositionState = WalkToPositionState.Normal;
    private PathFinding pathFinder;
    private List<Vector3> dodgeObstaclePath;
    private float lastPathFindingRefresh = 0;
    [SerializeField] private float pathFindingRefreshCooldown = 1f;

    [Header("Debug Settings")]
    [SerializeField] private bool debug = false;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        pathFinder = GetComponent<PathFinding>();
        currentMoveSpeed = moveSpeed;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (movementBehaviourState == MovementBehaviourState.WalkingToPosition)
        {
            WalkToPositionUpdate(targetPosition);
        }
    }

    public void WalkToPosition(Vector3 position, float customMoveSpeedPercentage = -1)
    {
        if (customMoveSpeedPercentage < 0) currentMoveSpeed = moveSpeed;
        else
        {
            currentMoveSpeed = moveSpeed * customMoveSpeedPercentage;
        }

        if (movementBehaviourState == MovementBehaviourState.Enabled)
        {
            walkToPositionState = WalkToPositionState.Normal;
            movementBehaviourState = MovementBehaviourState.WalkingToPosition;
            targetPosition = position;
        }
        else if (movementBehaviourState == MovementBehaviourState.WalkingToPosition)
        {
            UpdateTargetPosition(position);
        }
    }

    public void UpdateTargetPosition(Vector3 newTargetPosition)
    {
        this.targetPosition = newTargetPosition;
    }

    public void StopWalking()
    {
        if (movementBehaviourState == MovementBehaviourState.WalkingToPosition)
        {
            movementBehaviourState = MovementBehaviourState.Enabled;
        }
    }

    public void EnableMovement(bool enable)
    {
        if (!enable) movementBehaviourState = MovementBehaviourState.Disabled;
        else movementBehaviourState = MovementBehaviourState.Enabled;
    }

    private void WalkToPositionUpdate(Vector3 position)
    {
        float step = currentMoveSpeed * Time.deltaTime;
        // If there are no obstacles between enemy and player, move towards player
        if (walkToPositionState == WalkToPositionState.Normal)
        {
            transform.position = Vector2.MoveTowards(transform.position, position, step);

            // If there is an obstacle blocking the way, try to find a path to player and change state
            if (pathFinder.IsObstacleInBetween(transform.position, position, 1))
            {
                dodgeObstaclePath = pathFinder.FindShortestPath(transform.position, position);
                lastPathFindingRefresh = Time.time;

                // There is an existing path
                if (dodgeObstaclePath.Count > 0)
                {
                    dodgeObstaclePath = pathFinder.SmoothRoute(dodgeObstaclePath);
                    if (debug) pathFinder.DisplayRoute(dodgeObstaclePath);
                    walkToPositionState = WalkToPositionState.DodgingObstacle;
                }
                // There exists no path
                else
                {
                    walkToPositionState = WalkToPositionState.NoPathToPosition;
                }
            }
        }
        else if (walkToPositionState == WalkToPositionState.DodgingObstacle)
        {
            // If path is empty or there are no obstacles to the player, then return no normal state
            if (dodgeObstaclePath.Count == 0 || !pathFinder.IsObstacleInBetween(transform.position, position))
            {
                dodgeObstaclePath.Clear();
                walkToPositionState = WalkToPositionState.Normal;
            }
            // Otherwise, move to next point in route
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, dodgeObstaclePath[0], step);
                if (Vector3.Distance(transform.position, dodgeObstaclePath[0]) <= 0.1f * pathFinder.GetSize())
                {
                    dodgeObstaclePath.RemoveAt(0);
                }

                // Refresh route every few seconds
                if (Time.time - lastPathFindingRefresh > pathFindingRefreshCooldown)
                {
                    dodgeObstaclePath = pathFinder.FindShortestPath(transform.position, position);
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
                        walkToPositionState = WalkToPositionState.NoPathToPosition;
                    }
                }
            }
        }
        else if (walkToPositionState == WalkToPositionState.NoPathToPosition)
        {
            if (Time.time - lastPathFindingRefresh > pathFindingRefreshCooldown)
            {
                if (!pathFinder.IsObstacleInBetween(transform.position, position))
                {
                    dodgeObstaclePath.Clear();
                    walkToPositionState = WalkToPositionState.Normal;
                }
                else
                {
                    dodgeObstaclePath = pathFinder.FindShortestPath(transform.position, position);
                    lastPathFindingRefresh = Time.time;

                    // There is an existing path
                    if (dodgeObstaclePath.Count > 0)
                    {
                        dodgeObstaclePath = pathFinder.SmoothRoute(dodgeObstaclePath);
                        if (debug) pathFinder.DisplayRoute(dodgeObstaclePath);

                        walkToPositionState = WalkToPositionState.DodgingObstacle;
                    }
                }
            }
        }
    }
}
