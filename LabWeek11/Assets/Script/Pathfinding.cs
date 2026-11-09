using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// BFS pathfinding with:
///  - dynamic start / goal (SetStart, SetGoal, or mouse clicks)
///  - GenerateRandomGrid(width, height, obstacleProbability)
///  - AddObstacle(position) at runtime
///
/// Mouse controls (in Play mode, Scene or Game view with Main Camera looking down at the grid):
///   Left click   = set start
///   Right click  = set goal
///   Middle click = add obstacle
///   R key        = generate a new random grid
/// </summary>
public class Pathfinding : MonoBehaviour
{
    [Header("Random grid settings")]
    [SerializeField] private int randomWidth = 10;
    [SerializeField] private int randomHeight = 10;
    [Range(0f, 1f)]
    [SerializeField] private float obstacleProbability = 0.25f;

    private List<Vector2Int> path = new List<Vector2Int>();
    private Vector2Int start = new Vector2Int(0, 1);
    private Vector2Int goal = new Vector2Int(4, 4);

    private Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    // 0 = walkable, 1 = wall. Indexed as grid[y, x].
    private int[,] grid = new int[,]
    {
        { 0, 1, 0, 0, 0 },
        { 0, 1, 0, 1, 0 },
        { 0, 0, 0, 1, 0 },
        { 0, 1, 1, 1, 0 },
        { 0, 0, 0, 0, 0 }
    };

    private void Start()
    {
        // Find the first path.
        RecalculatePath();
    }

    private void Update()
    {
        // Check what the player pressed.
        HandleInput();
    }

    // Step 2.1 - Dynamic start and goal

    public void SetStart(Vector2Int position)
    {
        // Make sure the new start spot is usable.
        if (!IsInBounds(position) || grid[position.y, position.x] == 1)
        {
            Debug.Log("Cannot place start on a wall or outside the grid.");
            return;
        }

        // Set the new start and update the path.
        start = position;
        RecalculatePath();
    }

    public void SetGoal(Vector2Int position)
    {
        // Make sure the new goal spot is usable.
        if (!IsInBounds(position) || grid[position.y, position.x] == 1)
        {
            Debug.Log("Cannot place goal on a wall or outside the grid.");
            return;
        }

        // Set the new goal and update the path.
        goal = position;
        RecalculatePath();
    }

    // Step 2.2 - Random grid

    public void GenerateRandomGrid(int width, int height, float obstacleProbability)
    {
        // Keep the settings within valid limits.
        width = Mathf.Max(2, width);
        height = Mathf.Max(2, height);
        obstacleProbability = Mathf.Clamp01(obstacleProbability);

        // Make a new grid with random walls.
        grid = new int[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                grid[y, x] = Random.value < obstacleProbability ? 1 : 0;
            }
        }

        // Keep the start and goal in the grid and make them clear.
        start = new Vector2Int(Mathf.Clamp(start.x, 0, width - 1), Mathf.Clamp(start.y, 0, height - 1));
        goal = new Vector2Int(Mathf.Clamp(goal.x, 0, width - 1), Mathf.Clamp(goal.y, 0, height - 1));

        if (start == goal)
            goal = new Vector2Int(width - 1, height - 1);

        grid[start.y, start.x] = 0;
        grid[goal.y, goal.x] = 0;

        // Find a path through the new grid.
        RecalculatePath();
    }

    // Step 2.3 - Runtime obstacles

    public void AddObstacle(Vector2Int position)
    {
        // Ignore spots outside the grid.
        if (!IsInBounds(position))
            return;

        // Don't let the player wall off the start or goal cell itself.
        if (position == start || position == goal)
        {
            Debug.Log("Cannot place an obstacle on the start or goal.");
            return;
        }

        // Add a wall and update the path.
        grid[position.y, position.x] = 1;
        RecalculatePath(); // the old path may now be blocked
    }

    // Input (works with the new Input System or the legacy Input Manager)

    private void HandleInput()
    {
#if ENABLE_INPUT_SYSTEM
        // Use Unity's newer input system.
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            TryGetCell(Mouse.current.position.ReadValue(), out var _c1, c => SetStart(c));
        if (Mouse.current.rightButton.wasPressedThisFrame)
            TryGetCell(Mouse.current.position.ReadValue(), out var _c2, c => SetGoal(c));
        if (Mouse.current.middleButton.wasPressedThisFrame)
            TryGetCell(Mouse.current.position.ReadValue(), out var _c3, c => AddObstacle(c));
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            GenerateRandomGrid(randomWidth, randomHeight, obstacleProbability);
#elif ENABLE_LEGACY_INPUT_MANAGER
        // Use the older input system if needed.
        if (Input.GetMouseButtonDown(0))
            TryGetCell(Input.mousePosition, out var _c1, c => SetStart(c));
        if (Input.GetMouseButtonDown(1))
            TryGetCell(Input.mousePosition, out var _c2, c => SetGoal(c));
        if (Input.GetMouseButtonDown(2))
            TryGetCell(Input.mousePosition, out var _c3, c => AddObstacle(c));
        if (Input.GetKeyDown(KeyCode.R))
            GenerateRandomGrid(randomWidth, randomHeight, obstacleProbability);
#endif
    }

    // Turn a mouse click into a grid cell.
    private bool TryGetCell(Vector2 screenPosition, out Vector2Int cell, System.Action<Vector2Int> onHit)
    {
        cell = default;
        Camera cam = Camera.main;
        if (cam == null) return false;

        // Find where the mouse hit the ground.
        Ray ray = cam.ScreenPointToRay(screenPosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hit = ray.GetPoint(distance);
            // Change that position into a grid cell.
            cell = new Vector2Int(Mathf.RoundToInt(hit.x), Mathf.RoundToInt(hit.z));
            onHit?.Invoke(cell);
            return true;
        }
        return false;
    }

    // Drawing

    private void OnDrawGizmos()
    {
        float cellSize = 1f;

        // Draw the walls and open spaces.
        for (int y = 0; y < grid.GetLength(0); y++)
        {
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                Vector3 cellPosition = new Vector3(x * cellSize, 0, y * cellSize);
                Gizmos.color = grid[y, x] == 1 ? Color.black : Color.white;
                Gizmos.DrawCube(cellPosition, new Vector3(cellSize, 0.1f, cellSize));
            }
        }

        // Draw the path in blue.
        foreach (var step in path)
        {
            Vector3 cellPosition = new Vector3(step.x * cellSize, 0, step.y * cellSize);
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(cellPosition, new Vector3(cellSize, 0.1f, cellSize));
        }

        // Show the start in green and the goal in red.
        Gizmos.color = Color.green;
        Gizmos.DrawCube(new Vector3(start.x * cellSize, 0, start.y * cellSize), new Vector3(cellSize, 0.1f, cellSize));

        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector3(goal.x * cellSize, 0, goal.y * cellSize), new Vector3(cellSize, 0.1f, cellSize));
    }

    // BFS

    private bool IsInBounds(Vector2Int point)
    {
        return point.x >= 0 && point.x < grid.GetLength(1) && point.y >= 0 && point.y < grid.GetLength(0);
    }

    private void RecalculatePath()
    {
        FindPath(start, goal);
    }

    private void FindPath(Vector2Int start, Vector2Int goal)
    {
        // Clear the old path first.
        path.Clear();

        // Start searching from the start cell.
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        frontier.Enqueue(start);

        // Remember how we reached each cell.
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        cameFrom[start] = start;

        while (frontier.Count > 0)
        {
            // Check the next cell in line.
            Vector2Int current = frontier.Dequeue();

            if (current == goal)
                break;

            foreach (Vector2Int direction in directions)
            {
                // Look at each nearby cell.
                Vector2Int next = current + direction;

                if (IsInBounds(next) && grid[next.y, next.x] == 0 && !cameFrom.ContainsKey(next))
                {
                    // Add open cells that we have not checked yet.
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        if (!cameFrom.ContainsKey(goal))
        {
            // Stop if the goal cannot be reached.
            Debug.Log("Path not found.");
            return;
        }

        // Work backward from the goal to build the path.
        Vector2Int step = goal;
        while (step != start)
        {
            path.Add(step);
            step = cameFrom[step];
        }
        path.Add(start);
        // Reverse it so it starts at the beginning.
        path.Reverse();
    }
}
