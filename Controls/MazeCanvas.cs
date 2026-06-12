using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MazeRunner.Models;

namespace MazeRunner.Controls;

public enum EndpointSelectionMode
{
    None,
    Start,
    Goal
}

public sealed class MazeCellClickedEventArgs : EventArgs
{
    public MazeCellClickedEventArgs(MazePosition position) => Position = position;
    public MazePosition Position { get; }
}

/// <summary>
/// A single-pass, model-backed maze renderer. It intentionally avoids creating
/// thousands of child WPF controls so large mazes remain responsive.
/// </summary>
public sealed class MazeCanvas : FrameworkElement
{
    private const double BaseCellSize = 24.0;
    private const double ViewportPadding = 36.0;
    private const double MinimumZoom = 0.35;
    private const double MaximumZoom = 5.0;
    private const double ZoomIncrement = 0.14;

    public static readonly DependencyProperty MazeProperty = DependencyProperty.Register(
        nameof(Maze),
        typeof(Maze),
        typeof(MazeCanvas),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnMazeChanged));

    public static readonly DependencyProperty ShowGridProperty = DependencyProperty.Register(
        nameof(ShowGrid),
        typeof(bool),
        typeof(MazeCanvas),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowDirectionalMarkersProperty = DependencyProperty.Register(
        nameof(ShowDirectionalMarkers),
        typeof(bool),
        typeof(MazeCanvas),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    private bool _isPanning;
    private Point _panStart;
    private Vector _panOrigin;
    private Vector _panOffset;
    private double _zoomLevel = 1.0;
    private MazeVisualization _visualization = new();

    public MazeCanvas()
    {
        ClipToBounds = true;
        Cursor = Cursors.Hand;
        Focusable = true;
        SnapsToDevicePixels = true;
    }

    public event EventHandler? ZoomLevelChanged;
    public event EventHandler<MazeCellClickedEventArgs>? CellClicked;

    public Maze? Maze
    {
        get => (Maze?)GetValue(MazeProperty);
        set => SetValue(MazeProperty, value);
    }

    public bool ShowGrid
    {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    public bool ShowDirectionalMarkers
    {
        get => (bool)GetValue(ShowDirectionalMarkersProperty);
        set => SetValue(ShowDirectionalMarkersProperty, value);
    }

    public EndpointSelectionMode SelectionMode { get; set; }

    public MazeVisualization Visualization
    {
        get => _visualization;
        set
        {
            _visualization = value ?? throw new ArgumentNullException(nameof(value));
            InvalidateVisual();
        }
    }

    public double ZoomLevel => _zoomLevel;

    public void Refresh() => InvalidateVisual();
    public void ZoomIn() => ZoomAroundPoint(_zoomLevel + ZoomIncrement, new Point(ActualWidth / 2.0, ActualHeight / 2.0));
    public void ZoomOut() => ZoomAroundPoint(_zoomLevel - ZoomIncrement, new Point(ActualWidth / 2.0, ActualHeight / 2.0));

    public void FitToView()
    {
        _zoomLevel = 1.0;
        _panOffset = new Vector();
        InvalidateVisual();
        ZoomLevelChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        Rect viewport = new(0, 0, ActualWidth, ActualHeight);
        drawingContext.DrawRectangle(FindBrush("MazeCanvasBackgroundBrush", Brushes.Black), null, viewport);

        if (Maze is null || ActualWidth < 60 || ActualHeight < 60)
        {
            return;
        }

        double scale = GetRenderScale();
        Point origin = GetMazeOrigin(scale);
        Pen wallPen = new(FindBrush("MazeWallBrush", Brushes.SlateGray), Math.Max(1.35 / scale, 0.72));
        Pen gridPen = new(FindBrush("MazeCellGridBrush", Brushes.DarkSlateGray), Math.Max(0.55 / scale, 0.28));

        drawingContext.PushClip(new RectangleGeometry(viewport));
        drawingContext.PushTransform(new TranslateTransform(origin.X, origin.Y));
        drawingContext.PushTransform(new ScaleTransform(scale, scale));
        DrawCellFills(drawingContext);
        if (ShowGrid) DrawGrid(drawingContext, gridPen);
        DrawSolutionRoute(drawingContext);
        DrawWalls(drawingContext, wallPen);
        DrawEndpointMarkers(drawingContext);
        if (ShowDirectionalMarkers) DrawTerrainLabels(drawingContext, scale);
        drawingContext.Pop();
        drawingContext.Pop();
        drawingContext.Pop();
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        ZoomAroundPoint(_zoomLevel + (e.Delta > 0 ? ZoomIncrement : -ZoomIncrement), e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (Maze is null) return;
        Focus();
        CaptureMouse();
        _isPanning = true;
        _panStart = e.GetPosition(this);
        _panOrigin = _panOffset;
        Cursor = Cursors.SizeAll;
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (!_isPanning || e.LeftButton != MouseButtonState.Pressed) return;
        Point current = e.GetPosition(this);
        Vector movement = current - _panStart;
        if (SelectionMode == EndpointSelectionMode.None || movement.Length > 5)
        {
            _panOffset = _panOrigin + movement;
            InvalidateVisual();
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (!_isPanning) return;
        Point released = e.GetPosition(this);
        Vector movement = released - _panStart;
        _isPanning = false;
        ReleaseMouseCapture();
        Cursor = Cursors.Hand;

        if (SelectionMode != EndpointSelectionMode.None && movement.Length <= 5 && TryGetCellAt(released, out MazePosition position))
        {
            CellClicked?.Invoke(this, new MazeCellClickedEventArgs(position));
        }

        e.Handled = true;
    }

    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);
        _isPanning = false;
        Cursor = Cursors.Hand;
    }

    private static void OnMazeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        ((MazeCanvas)dependencyObject).FitToView();
    }

    private void ZoomAroundPoint(double requestedZoom, Point anchor)
    {
        if (Maze is null) return;
        double constrained = Math.Clamp(requestedZoom, MinimumZoom, MaximumZoom);
        if (Math.Abs(constrained - _zoomLevel) < 0.001) return;

        double oldScale = GetRenderScale();
        Point oldOrigin = GetMazeOrigin(oldScale);
        Point logicalAnchor = new((anchor.X - oldOrigin.X) / oldScale, (anchor.Y - oldOrigin.Y) / oldScale);
        _zoomLevel = constrained;
        double newScale = GetRenderScale();
        Size mazeSize = GetMazePixelSize();
        Point centeredOrigin = new((ActualWidth - mazeSize.Width * newScale) / 2.0, (ActualHeight - mazeSize.Height * newScale) / 2.0);
        _panOffset = new Vector(anchor.X - logicalAnchor.X * newScale - centeredOrigin.X, anchor.Y - logicalAnchor.Y * newScale - centeredOrigin.Y);
        InvalidateVisual();
        ZoomLevelChanged?.Invoke(this, EventArgs.Empty);
    }

    private void DrawCellFills(DrawingContext drawingContext)
    {
        if (Maze is null) return;
        foreach (MazeCell cell in Maze.Cells)
        {
            Brush brush = GetCellBrush(cell);
            drawingContext.DrawRectangle(brush, null, GetCellRect(cell.Position));
        }
    }

    private Brush GetCellBrush(MazeCell cell)
    {
        MazePosition position = cell.Position;
        if (Visualization.SolutionPath.Contains(position)) return FindBrush("MazeSolutionBrush", Brushes.DarkGreen);
        if (Maze is not null && position == Maze.Start) return FindBrush("MazeStartCellBrush", Brushes.DarkGreen);
        if (Maze is not null && position == Maze.Goal) return FindBrush("MazeGoalCellBrush", Brushes.DarkRed);
        if (Visualization.ShowSearchActivity)
        {
            if (Visualization.CurrentCell == position) return FindBrush("MazeCurrentBrush", Brushes.Orange);
            if (Visualization.Explored.Contains(position)) return FindBrush("MazeExploredBrush", Brushes.SteelBlue);
            if (Visualization.Frontier.Contains(position)) return FindBrush("MazeFrontierBrush", Brushes.CornflowerBlue);
            if (Visualization.Backtracked.Contains(position)) return FindBrush("MazeBacktrackBrush", Brushes.DarkSlateBlue);
        }
        if (Visualization.GeneratorVisited.Contains(position)) return FindBrush("MazeVisitedBrush", Brushes.DarkSlateGray);
        if (cell.TerrainCost > 1) return cell.TerrainCost >= 4
            ? FindBrush("MazeTerrainHighBrush", Brushes.SaddleBrown)
            : FindBrush("MazeTerrainBrush", Brushes.DarkGoldenrod);
        return (position.X + position.Y) % 2 == 0
            ? FindBrush("MazeCellBrush", Brushes.Transparent)
            : FindBrush("MazeCellAlternateBrush", Brushes.Transparent);
    }

    private void DrawGrid(DrawingContext drawingContext, Pen gridPen)
    {
        if (Maze is null) return;
        double width = Maze.Width * BaseCellSize;
        double height = Maze.Height * BaseCellSize;
        for (int x = 0; x <= Maze.Width; x++) drawingContext.DrawLine(gridPen, new Point(x * BaseCellSize, 0), new Point(x * BaseCellSize, height));
        for (int y = 0; y <= Maze.Height; y++) drawingContext.DrawLine(gridPen, new Point(0, y * BaseCellSize), new Point(width, y * BaseCellSize));
    }

    private void DrawWalls(DrawingContext drawingContext, Pen wallPen)
    {
        if (Maze is null) return;
        foreach (MazeCell cell in Maze.Cells)
        {
            double left = cell.Position.X * BaseCellSize;
            double top = cell.Position.Y * BaseCellSize;
            double right = left + BaseCellSize;
            double bottom = top + BaseCellSize;
            if (cell.HasWall(MazeWall.North)) drawingContext.DrawLine(wallPen, new Point(left, top), new Point(right, top));
            if (cell.HasWall(MazeWall.West)) drawingContext.DrawLine(wallPen, new Point(left, top), new Point(left, bottom));
            if (cell.Position.X == Maze.Width - 1 && cell.HasWall(MazeWall.East)) drawingContext.DrawLine(wallPen, new Point(right, top), new Point(right, bottom));
            if (cell.Position.Y == Maze.Height - 1 && cell.HasWall(MazeWall.South)) drawingContext.DrawLine(wallPen, new Point(left, bottom), new Point(right, bottom));
        }
    }

    private void DrawSolutionRoute(DrawingContext drawingContext)
    {
        if (Visualization.SolutionPath.Count < 2) return;
        Pen routePen = new(FindBrush("MazeSolutionRouteBrush", Brushes.LimeGreen), 3.4)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round
        };
        for (int index = 1; index < Visualization.SolutionPath.Count; index++)
        {
            drawingContext.DrawLine(routePen, GetCellCenter(Visualization.SolutionPath[index - 1]), GetCellCenter(Visualization.SolutionPath[index]));
        }
    }

    private void DrawEndpointMarkers(DrawingContext drawingContext)
    {
        if (Maze is null) return;
        DrawEndpointMarker(drawingContext, Maze.Start, FindBrush("MazeStartMarkerBrush", Brushes.LightGreen), "S");
        DrawEndpointMarker(drawingContext, Maze.Goal, FindBrush("MazeGoalMarkerBrush", Brushes.LightCoral), "G");
    }

    private void DrawEndpointMarker(DrawingContext drawingContext, MazePosition position, Brush brush, string label)
    {
        Point center = GetCellCenter(position);
        double radius = BaseCellSize * 0.29;
        drawingContext.DrawEllipse(brush, null, center, radius, radius);
        FormattedText text = CreateText(label, BaseCellSize * 0.42, FindBrush("MazeEndpointTextBrush", Brushes.Black));
        text.SetFontWeight(FontWeights.Bold);
        drawingContext.DrawText(text, new Point(center.X - text.Width / 2.0, center.Y - text.Height / 2.0));
    }

    private void DrawTerrainLabels(DrawingContext drawingContext, double scale)
    {
        if (Maze is null || scale < 0.5) return;
        foreach (MazeCell cell in Maze.Cells.Where(cell => cell.TerrainCost > 1))
        {
            FormattedText text = CreateText(cell.TerrainCost.ToString(CultureInfo.InvariantCulture), 8.5, FindBrush("TextPrimaryBrush", Brushes.White));
            Point center = GetCellCenter(cell.Position);
            drawingContext.DrawText(text, new Point(center.X - text.Width / 2.0, center.Y - text.Height / 2.0));
        }
    }

    private FormattedText CreateText(string text, double size, Brush brush) => new(
        text,
        CultureInfo.InvariantCulture,
        FlowDirection.LeftToRight,
        new Typeface("Segoe UI"),
        size,
        brush,
        VisualTreeHelper.GetDpi(this).PixelsPerDip);

    private bool TryGetCellAt(Point viewportPoint, out MazePosition position)
    {
        position = default;
        if (Maze is null) return false;
        double scale = GetRenderScale();
        Point origin = GetMazeOrigin(scale);
        int x = (int)Math.Floor((viewportPoint.X - origin.X) / scale / BaseCellSize);
        int y = (int)Math.Floor((viewportPoint.Y - origin.Y) / scale / BaseCellSize);
        position = new MazePosition(x, y);
        return Maze.Contains(position);
    }

    private Rect GetCellRect(MazePosition position) => new(position.X * BaseCellSize, position.Y * BaseCellSize, BaseCellSize, BaseCellSize);
    private Point GetCellCenter(MazePosition position) => new(position.X * BaseCellSize + BaseCellSize / 2.0, position.Y * BaseCellSize + BaseCellSize / 2.0);
    private double GetRenderScale() => GetFitScale() * _zoomLevel;

    private double GetFitScale()
    {
        Size mazeSize = GetMazePixelSize();
        if (mazeSize.Width <= 0 || mazeSize.Height <= 0) return 1.0;
        double availableWidth = Math.Max(ActualWidth - ViewportPadding * 2.0, 1.0);
        double availableHeight = Math.Max(ActualHeight - ViewportPadding * 2.0, 1.0);
        return Math.Min(availableWidth / mazeSize.Width, availableHeight / mazeSize.Height);
    }

    private Size GetMazePixelSize() => Maze is null ? new Size() : new Size(Maze.Width * BaseCellSize, Maze.Height * BaseCellSize);

    private Point GetMazeOrigin(double scale)
    {
        Size mazeSize = GetMazePixelSize();
        return new Point((ActualWidth - mazeSize.Width * scale) / 2.0 + _panOffset.X, (ActualHeight - mazeSize.Height * scale) / 2.0 + _panOffset.Y);
    }

    private Brush FindBrush(string resourceKey, Brush fallback) => TryFindResource(resourceKey) as Brush ?? fallback;
}
