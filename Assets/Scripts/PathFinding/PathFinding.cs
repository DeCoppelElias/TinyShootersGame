using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathFinding : MonoBehaviour
{
    [Header("Size of the entity")]
    [SerializeField] private bool customSize = false;
    [SerializeField] private float size = 0;

    [Header("Pathfinding settings")]
    [SerializeField] private float stepSize = 0.25f;
    [SerializeField] private int maxIterations = 1000;

    [Header("Display settings")]
    [SerializeField] private LineRenderer displayLineRenderer;

    private void Start()
    {
        // Initialise size
        if (!customSize)
        {
            Collider2D collider = GetComponent<Collider2D>();
            if (collider == null) throw new Exception("Cannot find size because collider is missing");
            size = collider.bounds.size.x;
        }
    }

    /// <summary>
    /// A GridTile class represents a node in a graph like structure needed for the A* shortest path implementation.
    /// </summary>
    public class GridTile : IComparable<GridTile>
    {
        public GridTile previous;
        public Vector3 location;
        public float priority;

        public GridTile(Vector3 location, GridTile previous, float priority)
        {
            this.previous = previous;
            this.location = location;
            this.priority = priority;
        }

        public int CompareTo(GridTile other)
        {
            if (this.priority < other.priority) return -1;
            else if (this.priority > other.priority) return 1;
            else return 0;
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

    /// <summary>
    /// This method finds the shortest path between two positions with A*. Written by Chatgtp.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public List<Vector3> FindShortestPath(Vector3 start, Vector3 target)
    {
        // Initialize the open and closed sets
        PriorityQueue<GridTile> openSet = new PriorityQueue<GridTile>();
        HashSet<Vector3> visited = new HashSet<Vector3>();
        openSet.Enqueue(new GridTile(start, null, 0));

        int iteration = 0;
        while (openSet.Count > 0 && iteration < this.maxIterations)
        {
            GridTile current = openSet.Dequeue();

            // If we've reached the target, reconstruct the path
            if (Vector3.Distance(current.location, target) < size)
            {
                return ReconstructPath(current);
            }

            if (visited.Contains(current.location))
                continue;

            visited.Add(current.location);

            // Explore neighbors
            foreach (Vector3 neighbor in GetNeighbors(current.location))
            {
                if (visited.Contains(neighbor) || InvalidPosition(neighbor))
                    continue;

                float newPriority = Vector3.Distance(neighbor, target);
                openSet.Enqueue(new GridTile(neighbor, current, newPriority));
            }

            iteration++;
        }

        // Return an empty path if no path is found
        return new List<Vector3>();
    }

    /// <summary>
    /// Checks wether a certain position is valid by checking if it isn't too close to some obstacle. 
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public bool InvalidPosition(Vector3 location)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(location, 0.5f * size);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Wall") || collider.CompareTag("Pit")) return true;
        }
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

        Vector3 raycastDirection = (to - from).normalized;
        RaycastHit2D[] rays = Physics2D.RaycastAll(from, raycastDirection, distance);
        if (RaycastHitsObstacle(rays)) return true;

        // Perpendicular vectors
        Vector3 perpendicular1 = new Vector3(-raycastDirection.y, raycastDirection.x, raycastDirection.z);
        Vector3 perpendicular2 = new Vector3(raycastDirection.y, -raycastDirection.x, raycastDirection.z);

        Vector3 newFrom1 = from + (size * perpendicular1);
        raycastDirection = (to - newFrom1).normalized;
        rays = Physics2D.RaycastAll(newFrom1, raycastDirection, distance);
        if (RaycastHitsObstacle(rays)) return true;

        Vector3 newFrom2 = from + (size * perpendicular2);
        raycastDirection = (to - newFrom2).normalized;
        rays = Physics2D.RaycastAll(newFrom2, raycastDirection, distance);
        if (RaycastHitsObstacle(rays)) return true;

        return false;
    }

    /// <summary>
    /// Checks whether a raycast has hit an obstacle.
    /// </summary>
    /// <param name="rays"></param>
    /// <returns></returns>
    public bool RaycastHitsObstacle(RaycastHit2D[] rays)
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

    public float GetSize()
    {
        return this.size;
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

