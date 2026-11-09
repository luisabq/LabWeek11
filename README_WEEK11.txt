Pathfinding Lab
Author: Luisa Brizolim Quesada (Solo Project)
Link: https://youtu.be/XExHP0fD2qc?si=IuHT_DNrbpqPRm9_ 
What I Did
I updated the BFS pathfinding script to allow the start and goal positions to change, generate random grids, and add obstacles while the game is running. The path automatically updates whenever something changes.
Controls:
* Left Click: Set start
* Right Click: Set goal
* Middle Click or Shift + Left Click: Add obstacle
* R: Generate a new random grid
To run it, attach the Pathfinding script to an empty GameObject, position the camera above the grid, and enable Gizmos to see it.
Reflection
1. How does each algorithm calculate and prioritize paths?
BFS checks nearby cells in order, exploring all directions equally until it finds the goal. It works well for simple grids where every movement has the same cost.
Dijkstra prioritizes paths with the lowest total movement cost, making it useful when different areas have different costs.
A* considers both the distance already traveled and the estimated distance to the goal. This makes it more efficient since it focuses on moving toward the goal instead of searching everywhere.
2. What challenges arise when dynamically updating obstacles in real time?
The biggest challenge is making sure the path stays valid whenever obstacles are added or changed. If an obstacle blocks the current path, the algorithm needs to calculate a new one.
Constantly recalculating paths can also affect performance, especially on larger grids. Another issue is when obstacles completely block the goal, leaving no possible path.
3. Which algorithm should you choose, and how should you adapt it for larger grids or open-world settings?
I would choose A* because it's generally faster and more efficient for finding a path to a specific goal.
For larger grids or open worlds, I would split the map into smaller sections so the algorithm doesn't have to search the entire area at once. Using navigation meshes or reusing previously calculated paths could also help improve performance.
BFS would still work well for smaller grids, while Dijkstra would be useful when movement costs vary.
4. What would your approach be if you were to add weighted cells?
I would assign different movement costs to each type of terrain. For example, a normal path could cost 1, grass could cost 2, and rough terrain could cost 3.
Since BFS treats every step equally, I would switch to A* or Dijkstra so the algorithm can consider these costs.
This would allow the algorithm to choose a slightly longer route if it has a lower overall movement cost.