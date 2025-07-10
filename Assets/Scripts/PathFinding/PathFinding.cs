using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathFinding : MonoBehaviour
{
    [Header("Size of the entity")]
    [SerializeField] private bool customSize = false;
    [SerializeField] private float diameter = 0;

    [Header("Pathfinding settings")]
    [SerializeField] private float stepSize = 0.25f;
    [SerializeField] private int maxIterations = 10000;

    [Header("Display settings")]
    [SerializeField] private LineRenderer displayLineRenderer;

    private WaveManager waveManager;

    public LayerMask layerMask;

    private void Start()
    {
        // Initialise size
        if (!customSize)
        {
            Collider2D collider = GetComponent<Collider2D>();
            if (collider == null) throw new Exception("Cannot find size because collider is missing");
            diameter = collider.bounds.size.x;
        }

        // Initial Pathfinding settings
        this.stepSize = 0.25f;
        this.maxIterations = 10000;

        waveManager = GameObject.Find("WaveManager")?.GetComponent<WaveManager>();
    }

    /// <summary>
    /// A GridTile class represents a node in a graph like structure needed for the A* shortest path implementation.
    /// </summary>
    public class GridTile : IComparable<GridTile>
    {
        public GridTile previous;
        public Vector3 location;
        public float gCost;
        public float priority;

        public GridTile(Vector3 location, GridTile previous, float gCost, float priority)
        {
            this.previous = previous;
            this.location = location;
            this.gCost = gCost;
            this.priority = priority;
        }

        public int CompareTo(GridTile other)
        {
            return this.priority.CompareTo(other.priority);
        }
    }

    /// <summary>
    /// Priority Queue, written by Chatgtp.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> elements = new List<T>();

        public int Count => elements.Count;

        public void Enqueue(T item)
        {
            elements.Add(item);
            int c = elements.Count - 1;

            // Percolate up
            while (c > 0)
            {
                int p = (c - 1) / 2; // Parent index
                if (elements[c].CompareTo(elements[p]) >= 0)
                    break;

                // Swap child and parent
                T temp = elements[c];
                elements[c] = elements[p];
                elements[p] = temp;
                c = p;
            }
        }

        public T Dequeue()
        {
            int lastIndex = elements.Count - 1;

            // Swap root with the last element
            T root = elements[0];
            elements[0] = elements[lastIndex];
            elements.RemoveAt(lastIndex);

            // Percolate down
            int c = 0;
            while (true)
            {
                int left = 2 * c + 1;
                int right = 2 * c + 2;

                if (left >= elements.Count) break;

                int min = left;
                if (right < elements.Count && elements[right].CompareTo(elements[left]) < 0)
                    min = right;

                if (elements[c].CompareTo(elements[min]) <= 0)
                    break;

                // Swap parent and child
                T temp = elements[c];
                elements[c] = elements[min];
                elements[min] = temp;
                c = min;
            }

            return root;
        }
    }

    private float GetTileCost(Vector3 position)
    {
        float radius = 0.5f * diameter;

        LayerMask obstacleLayerMask = LayerMask.GetMask("Wall", "Pit");
        if (Physics2D.OverlapCircle(position, radius, obstacleLayerMask) != null)
            return Mathf.Infinity;

        LayerMask pushableLayerMask = LayerMask.GetMask("Pushable");
        if (Physics2D.OverlapCircle(position, radius, pushableLayerMask) != null)
            return 3;

        LayerMask entityLayerMask = LayerMask.GetMask("Entity");
        if (Physics2D.OverlapCircle(position, radius, entityLayerMask) != null)
            return 2;

        // Add random noise to make paths more random
        float noise = UnityEngine.Random.Range(0,1f) * 0.2f;
        return 1f + noise;
    }

    /// <summary>
    /// This method finds the shortest path between two positions with A*. Written by Chatgtp.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public List<Vector3> FindPath(Vector3 start, Vector3 target)
    {
        PriorityQueue<GridTile> openSet = new PriorityQueue<GridTile>();
        HashSet<Vector3> closedSet = new HashSet<Vector3>();
        Dictionary<Vector3, float> gCosts = new Dictionary<Vector3, float>();

        float initialGCost = 0;
        float initialHCost = Vector3.Distance(start, target);
        openSet.Enqueue(new GridTile(start, null, initialGCost, initialGCost + initialHCost));
        gCosts[start] = 0;

        int iteration = 0;
        while (openSet.Count > 0 && iteration < maxIterations)
        {
            GridTile current = openSet.Dequeue();

            if (Vector3.Distance(current.location, target) < diameter)
            {
                return ReconstructPath(current);
            }

            if (closedSet.Contains(current.location))
                continue;

            closedSet.Add(current.location);

            foreach (Vector3 neighbor in GetNeighbors(current.location))
            {
                if (closedSet.Contains(neighbor) || InvalidPosition(neighbor))
                    continue;

                float tileCost = GetTileCost(neighbor);
                if (tileCost == Mathf.Infinity)
                    continue;

                float tentativeGCost = current.gCost + Vector3.Distance(current.location, neighbor) * tileCost;

                if (!gCosts.ContainsKey(neighbor) || tentativeGCost < gCosts[neighbor])
                {
                    gCosts[neighbor] = tentativeGCost;
                    float hCost = Vector3.Distance(neighbor, target);
                    float fCost = tentativeGCost + hCost;
                    openSet.Enqueue(new GridTile(neighbor, current, tentativeGCost, fCost));
                }
            }

            iteration++;
        }

        // No path found
        return new List<Vector3>();
    }

    /// <summary>
    /// Checks wether a certain position is valid by checking if it isn't too close to some obstacle and by checking if the position is inside the level.
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public bool InvalidPosition(Vector3 location)
    {
        float radius = 0.5f * diameter;

        LayerMask obstacleLayerMask = LayerMask.GetMask("Wall", "Pit");
        Collider2D collider = Physics2D.OverlapCircle(location, radius, obstacleLayerMask);
        if (collider != null)
            return true;

        if (waveManager != null && !waveManager.InsideLevel(location))
            return true;

        return false;
    }

    /// <summary>
    /// This method reconstructs a path from the GridTile class.
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    private List<Vector3> ReconstructPath(GridTile tile)
    {
        List<Vector3> path = new List<Vector3>();
        while (tile != null)
        {
            path.Add(tile.location);
            tile = tile.previous;
        }
        path.Reverse();
        return path;
    }

    /// <summary>
    /// This method returns the neighboring positions based on the stepsize.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private List<Vector3> GetNeighbors(Vector3 position)
    {
        List<Vector3> neighbors = new List<Vector3>
        {
            position + new Vector3(stepSize, 0, 0),
            position + new Vector3(-stepSize, 0, 0),
            position + new Vector3(0, stepSize, 0),
            position + new Vector3(0, -stepSize, 0)
        };

        // Shuffle neighbors randomly
        for (int i = neighbors.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var temp = neighbors[i];
            neighbors[i] = neighbors[j];
            neighbors[j] = temp;
        }

        return neighbors;
    }

    /// <summary>
    /// Displays a route.
    /// </summary>
    /// <param name="route"></param>
    public void DisplayRoute(List<Vector3> route)
    {
        if (displayLineRenderer != null)
        {
            displayLineRenderer.positionCount = route.Count;
            displayLineRenderer.SetPositions(route.ToArray());
        }
    }

    /// <summary>
    /// Smooths a route by removing unnecessary points on a line without obstacles in between.
    /// </summary>
    /// <param name="route"></param>
    /// <returns></returns>
    public List<Vector3> SmoothRoute(List<Vector3> route)
    {
        List<Vector3> newRoute = new List<Vector3>();
        if (route.Count <= 2) return route;

        newRoute.Add(route[0]);
        int i = 0;
        while (i < route.Count - 2)
        {
            int j = i + 2;

            while (j < route.Count && !IsObstacleInBetween(route[i], route[j]))
            {
                j++;
            }
            newRoute.Add(route[j - 1]);
            i = j - 1;
        }

        return newRoute;
    }

    /// <summary>
    /// Checks whether there is an obstacle in between two points.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public bool IsObstacleInBetween(Vector3 from, Vector3 to, float distance = -1)
    {
        if (distance <= 0) distance = Vector3.Distance(from, to);

        Vector3 direction = (to - from).normalized;
        float radius = 0.5f * diameter;

        LayerMask obstacleLayerMask = LayerMask.GetMask("Wall", "Pit", "Pushable");
        RaycastHit2D hit = Physics2D.CircleCast(from, radius, direction, distance, obstacleLayerMask);
        return hit.collider != null;
    }

    public float GetSize()
    {
        return this.diameter;
    }

    public float GetPathDistance(List<Vector3> path)
    {
        float totalDistance = 0;

        for (int i = 0; i < path.Count - 1; i++)
        {
            totalDistance += Vector3.Distance(path[i], path[i + 1]);
        }

        return totalDistance;
    }
}

