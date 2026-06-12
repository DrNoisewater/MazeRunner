# MazeRunner

MazeRunner is a single-project C# WPF desktop application for generating randomized mazes and visualizing pathfinding algorithms in real time.

## Included features

- Modern dark desktop shell with Visualizer, Compare, History, and Settings sections.
- Efficient `FrameworkElement` renderer using `DrawingContext` rather than per-cell WPF controls.
- Zooming, panning, fit-to-view, cell-grid toggling, endpoint selection, and PNG export.
- Recursive Backtracker, Randomized Prim, Randomized Kruskal, and Recursive Division generators.
- BFS, DFS, Dijkstra, A*, Greedy Best-First, Bidirectional BFS, and Wall Follower solvers.
- Pause, resume, single-step, animation speed, and instant-complete controls.
- Braiding / loop generation, randomized endpoints, and weighted terrain.
- Solver comparison cards, local JSON run history, local settings persistence, JSON maze import/export, theme switching, and accent presets.

## Target

- C#
- WPF
- .NET 10 Windows Desktop
- Single project


## Generation clarity update

- Generating a maze and solving a maze are explicitly separate actions.
- Generation progress coloring is cleared when a maze becomes ready so it cannot be mistaken for solver exploration.
- Dark ComboBox templates keep algorithm names readable in both the closed control and the expanded list.

## Final route display

After any solver animation completes, MazeRunner now removes the transient search tint and displays one clean green optimal route. For ordinary mazes this is the shortest valid route by steps. With weighted terrain enabled, it is the lowest-cost valid route.
