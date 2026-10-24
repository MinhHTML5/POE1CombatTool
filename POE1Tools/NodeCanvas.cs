using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using POE1Tools.Utilities;

namespace POE1Tools
{
    // The canvas of the Automation window: the nodes of an action preset (the Start, the End and the macros),
    // and the links between them. The wheel zooms around the cursor, dragging the background (or the middle button)
    // pans, dragging a node moves it, a click selects a node or a link, a right click opens the menu of the window.
    // "Link mode" (BeginLink) draws a line from a node to the cursor until another node is clicked.
    public class NodeCanvas : Control
    {
        public const float NODE_WIDTH = 180;
        public const float NODE_HEIGHT = 58;
        public const float PILL_WIDTH = 96;
        public const float PILL_HEIGHT = 40;
        public const float MIN_ZOOM = 0.2f;
        public const float MAX_ZOOM = 3f;
        private const float GRID_SIZE = 40;
        // How close to a link a click has to be, in screen pixels
        private const float LINK_HIT_DISTANCE = 7;
        // Space between the links going from a node to the same node
        private const float PARALLEL_LINK_GAP = 16;
        private const float ARROW_SIZE = 11;

        private static readonly Color GRID_COLOR = Color.FromArgb(26, 33, 58);
        private static readonly Color NODE_BORDER = Color.FromArgb(140, 146, 158);
        private static readonly Color NODE_TEXT = Color.FromArgb(30, 32, 36);
        private static readonly Color NODE_SUBTEXT = Color.FromArgb(110, 116, 128);
        private static readonly Color SELECTED_COLOR = Color.FromArgb(30, 120, 220);
        // The selected node, and dashed around the macros sharing its name
        private static readonly Color SELECTED_NODE_COLOR = Color.FromArgb(230, 60, 60);
        private static readonly Color RUNNING_COLOR = Color.FromArgb(240, 150, 30);
        private static readonly Color RUNNING_FILL = Color.FromArgb(255, 245, 220);
        private static readonly Color LINK_COLOR = Color.FromArgb(165, 172, 190);
        private static readonly Color START_COLOR = Color.FromArgb(70, 165, 105);
        private static readonly Color END_COLOR = Color.FromArgb(210, 80, 80);
        private static readonly Color WARNING_COLOR = Color.FromArgb(220, 100, 40);
        private static readonly Color DISABLED_FILL = Color.FromArgb(190, 194, 202);
        private static readonly Color DISABLED_BORDER = Color.FromArgb(110, 116, 128);
        private static readonly Color DISABLED_TEXT = Color.FromArgb(90, 95, 105);
        private static readonly Color FINAL_FILL = Color.FromArgb(255, 236, 180);
        private static readonly Color FINAL_BORDER = Color.FromArgb(200, 160, 60);

        // The bezier of a link, and its flattened points for the hit test. All in world coordinates
        private class LinkGeometry
        {
            public PointF Start, Control1, Control2, End;
            public PointF[] Points;
            public bool IsSelfLoop;
        }

        private ActionPreset _graph;
        private float _zoom = 1f;
        // Where the world origin is on the screen
        private PointF _pan = new PointF(0, 0);
        private ActionNode _selectedNode;
        private ActionLink _selectedLink;
        private ActionNode _linkSource;
        private PointF _mouseWorld;
        private ActionNode _dragNode;
        private PointF _dragOffset;
        private bool _dragMoved = false;
        private bool _panning = false;
        private Point _panStart;
        private PointF _panOrigin;
        private string _runningNodeId;
        private readonly Dictionary<ActionLink, LinkGeometry> _geometries = new Dictionary<ActionLink, LinkGeometry>();
        // A link to a macro also goes to the macros sharing its name: those are drawn dashed, and can't be clicked
        private readonly List<LinkGeometry> _ghostGeometries = new List<LinkGeometry>();
        private readonly Font _titleFont;
        private readonly Font _subtitleFont;
        private readonly Font _labelFont;
        private readonly Font _labelBoldFont;
        private readonly Font _badgeFont;
        private readonly Font _pillFont;

        public event Action SelectionChanged;
        // A node was dragged somewhere else
        public event Action GraphChanged;
        // Link mode ended on a node: from the source of the link mode, to the clicked node
        public event Action<ActionNode, ActionNode> LinkRequested;
        // The user right clicked: at this world position, and this client position. What was under the cursor is selected
        public event Action<PointF, Point> ContextMenuRequested;
        // Delete was pressed
        public event Action DeleteRequested;
        public event Action LinkModeChanged;
        // Zoom or pan changed
        public event Action ViewChanged;

        public ActionPreset Graph => _graph;
        public ActionNode SelectedNode => _selectedNode;
        public ActionLink SelectedLink => _selectedLink;
        public bool IsLinking => _linkSource != null;
        public ActionNode LinkSource => _linkSource;
        public float Zoom => _zoom;
        // No move / link while the automation is running. Selecting still works
        public bool Locked { get; set; } = false;

        // Drawn highlighted while the action runs. Null for none
        public string RunningNodeId
        {
            get { return _runningNodeId; }
            set
            {
                if (_runningNodeId == value) return;
                _runningNodeId = value;
                Invalidate();
            }
        }

        public NodeCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint
                | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            TabStop = true;
            BackColor = Color.FromArgb(12, 16, 32);
            _titleFont = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
            _subtitleFont = new Font("Segoe UI", 8f);
            _labelFont = new Font("Segoe UI", 8f);
            _labelBoldFont = new Font("Segoe UI Semibold", 8f, FontStyle.Bold);
            _badgeFont = new Font("Segoe UI Semibold", 6.5f, FontStyle.Bold);
            _pillFont = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _titleFont.Dispose();
                _subtitleFont.Dispose();
                _labelFont.Dispose();
                _labelBoldFont.Dispose();
                _badgeFont.Dispose();
                _pillFont.Dispose();
            }
            base.Dispose(disposing);
        }



        // ------------------------------------------------------------------------------------
        // Graph, selection, link mode
        // ------------------------------------------------------------------------------------

        // Show another action preset, null for none. The selection is cleared and the view fits the nodes
        public void SetGraph(ActionPreset graph)
        {
            _graph = graph;
            _selectedNode = null;
            _selectedLink = null;
            _linkSource = null;
            _dragNode = null;
            _panning = false;
            Cursor = Cursors.Default;
            _geometries.Clear();
            FitToContent();
            SelectionChanged?.Invoke();
        }

        public void Select(ActionNode node)
        {
            if (_selectedNode == node && _selectedLink == null) return;
            _selectedNode = node;
            _selectedLink = null;
            Invalidate();
            SelectionChanged?.Invoke();
        }

        public void Select(ActionLink link)
        {
            if (_selectedLink == link && _selectedNode == null) return;
            _selectedNode = null;
            _selectedLink = link;
            Invalidate();
            SelectionChanged?.Invoke();
        }

        public void ClearSelection()
        {
            if (_selectedNode == null && _selectedLink == null) return;
            _selectedNode = null;
            _selectedLink = null;
            Invalidate();
            SelectionChanged?.Invoke();
        }

        // Something was added / removed / renamed by the window
        public void RefreshGraph()
        {
            if (_graph != null)
            {
                if (_selectedNode != null && !_graph.Nodes.Contains(_selectedNode)) _selectedNode = null;
                if (_selectedLink != null && !_graph.Links.Contains(_selectedLink)) _selectedLink = null;
                if (_linkSource != null && !_graph.Nodes.Contains(_linkSource)) CancelLink();
            }
            Invalidate();
        }

        public void BeginLink(ActionNode source)
        {
            if (source == null || source.Kind == ActionNodeKind.End || Locked || _graph == null) return;
            _linkSource = source;
            Cursor = Cursors.Cross;
            Focus();
            Invalidate();
            LinkModeChanged?.Invoke();
        }

        public void CancelLink()
        {
            if (_linkSource == null) return;
            _linkSource = null;
            Cursor = Cursors.Default;
            Invalidate();
            LinkModeChanged?.Invoke();
        }



        // ------------------------------------------------------------------------------------
        // View
        // ------------------------------------------------------------------------------------

        public PointF WorldToScreen(PointF world)
        {
            return new PointF(world.X * _zoom + _pan.X, world.Y * _zoom + _pan.Y);
        }

        public PointF ScreenToWorld(Point screen)
        {
            return new PointF((screen.X - _pan.X) / _zoom, (screen.Y - _pan.Y) / _zoom);
        }

        public PointF ViewCenterWorld => ScreenToWorld(new Point(Width / 2, Height / 2));

        // A wheel notch (120) zooms by 10%
        public void ZoomAt(Point screenPoint, int wheelDelta)
        {
            float factor = (float)Math.Pow(1.1, wheelDelta / 120.0);
            SetZoom(_zoom * factor, screenPoint);
        }

        // Zoom while keeping the world point under "around" where it is on the screen
        public void SetZoom(float zoom, Point around)
        {
            zoom = Math.Max(MIN_ZOOM, Math.Min(MAX_ZOOM, zoom));
            PointF world = ScreenToWorld(around);
            _zoom = zoom;
            _pan = new PointF(around.X - world.X * _zoom, around.Y - world.Y * _zoom);
            Invalidate();
            ViewChanged?.Invoke();
        }

        // Zoom out (never in past 100%) and pan so every node is visible
        public void FitToContent()
        {
            if (_graph == null || _graph.Nodes.Count == 0 || Width <= 0 || Height <= 0)
            {
                _zoom = 1f;
                _pan = new PointF(0, 0);
                Invalidate();
                ViewChanged?.Invoke();
                return;
            }

            RectangleF bounds = RectangleF.Empty;
            bool first = true;
            foreach (ActionNode node in _graph.Nodes)
            {
                RectangleF nodeBounds = GetNodeBounds(node);
                bounds = first ? nodeBounds : RectangleF.Union(bounds, nodeBounds);
                first = false;
            }
            // Room for the links looping around the nodes
            bounds.Inflate(70, 90);

            float zoom = Math.Min(Width / bounds.Width, Height / bounds.Height);
            _zoom = Math.Max(MIN_ZOOM, Math.Min(1f, zoom));
            _pan = new PointF(
                Width / 2f - (bounds.X + bounds.Width / 2) * _zoom,
                Height / 2f - (bounds.Y + bounds.Height / 2) * _zoom);
            Invalidate();
            ViewChanged?.Invoke();
        }



        // ------------------------------------------------------------------------------------
        // Geometry and hit tests
        // ------------------------------------------------------------------------------------

        public static RectangleF GetNodeBounds(ActionNode node)
        {
            if (node.IsMacro) return new RectangleF((float)node.X, (float)node.Y, NODE_WIDTH, NODE_HEIGHT);
            return new RectangleF((float)node.X, (float)node.Y, PILL_WIDTH, PILL_HEIGHT);
        }

        // The node under a world point, the one drawn on top first
        public ActionNode HitTestNode(PointF world)
        {
            if (_graph == null) return null;
            for (int i = _graph.Nodes.Count - 1; i >= 0; i--)
            {
                if (GetNodeBounds(_graph.Nodes[i]).Contains(world)) return _graph.Nodes[i];
            }
            return null;
        }

        public ActionLink HitTestLink(PointF world)
        {
            if (_graph == null) return null;
            RebuildGeometries();

            float maxDistance = LINK_HIT_DISTANCE / _zoom;
            ActionLink best = null;
            float bestDistance = float.MaxValue;
            foreach (KeyValuePair<ActionLink, LinkGeometry> pair in _geometries)
            {
                PointF[] points = pair.Value.Points;
                for (int i = 1; i < points.Length; i++)
                {
                    float distance = DistanceToSegment(world, points[i - 1], points[i]);
                    if (distance <= maxDistance && distance < bestDistance)
                    {
                        bestDistance = distance;
                        best = pair.Key;
                    }
                }
            }
            return best;
        }

        private static float DistanceToSegment(PointF p, PointF a, PointF b)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            float length2 = dx * dx + dy * dy;
            float t = length2 == 0 ? 0 : Math.Max(0, Math.Min(1, ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / length2));
            float px = a.X + t * dx - p.X;
            float py = a.Y + t * dy - p.Y;
            return (float)Math.Sqrt(px * px + py * py);
        }

        private static PointF BezierPoint(LinkGeometry g, float t)
        {
            float u = 1 - t;
            float b0 = u * u * u, b1 = 3 * u * u * t, b2 = 3 * u * t * t, b3 = t * t * t;
            return new PointF(
                b0 * g.Start.X + b1 * g.Control1.X + b2 * g.Control2.X + b3 * g.End.X,
                b0 * g.Start.Y + b1 * g.Control1.Y + b2 * g.Control2.Y + b3 * g.End.Y);
        }

        // Where a link leaves a node, and where it enters one
        private static PointF OutAnchor(ActionNode node, float offset)
        {
            RectangleF b = GetNodeBounds(node);
            return new PointF(b.Right, b.Y + b.Height / 2 + offset);
        }

        private static PointF InAnchor(ActionNode node, float offset)
        {
            RectangleF b = GetNodeBounds(node);
            return new PointF(b.Left, b.Y + b.Height / 2 + offset);
        }

        private void RebuildGeometries()
        {
            _geometries.Clear();
            _ghostGeometries.Clear();
            if (_graph == null) return;

            // The links between the same two nodes are spread out, or they would overlap
            Dictionary<string, int> pairCounts = new Dictionary<string, int>();
            Dictionary<string, int> pairSeen = new Dictionary<string, int>();
            foreach (ActionLink link in _graph.Links)
            {
                string key = link.FromId + ">" + link.ToId;
                pairCounts[key] = pairCounts.TryGetValue(key, out int count) ? count + 1 : 1;
            }

            foreach (ActionLink link in _graph.Links)
            {
                ActionNode from = _graph.FindNode(link.FromId);
                ActionNode to = _graph.FindNode(link.ToId);
                if (from == null || to == null) continue;

                string key = link.FromId + ">" + link.ToId;
                int index = pairSeen.TryGetValue(key, out int seen) ? seen : 0;
                pairSeen[key] = index + 1;
                float offset = (index - (pairCounts[key] - 1) / 2f) * PARALLEL_LINK_GAP;

                _geometries[link] = MakeGeometry(from, to, offset);
                foreach (ActionNode sibling in _graph.GroupOf(to))
                {
                    if (sibling != to) _ghostGeometries.Add(MakeGeometry(from, sibling, offset));
                }
            }
        }

        private static LinkGeometry MakeGeometry(ActionNode from, ActionNode to, float offset)
        {
            LinkGeometry g = new LinkGeometry();
            if (from == to)
            {
                // A loop over the top of the node
                RectangleF b = GetNodeBounds(from);
                float rise = 80 + Math.Abs(offset) * 2;
                g.IsSelfLoop = true;
                g.Start = new PointF(b.Right - 30, b.Top);
                g.End = new PointF(b.Left + 30, b.Top);
                g.Control1 = new PointF(g.Start.X + 50, b.Top - rise);
                g.Control2 = new PointF(g.End.X - 50, b.Top - rise);
            }
            else
            {
                g.Start = OutAnchor(from, offset);
                g.End = InAnchor(to, offset);
                if (g.End.X < g.Start.X + 40)
                {
                    // Going back to the left: swing below the nodes instead of cutting through them
                    float drop = 90 + offset;
                    g.Control1 = new PointF(g.Start.X + 90, g.Start.Y + drop);
                    g.Control2 = new PointF(g.End.X - 90, g.End.Y + drop);
                }
                else
                {
                    float dx = Math.Max(50, Math.Abs(g.End.X - g.Start.X) * 0.5f);
                    // A long flat link probably skips over a node in between: bow it under that node
                    float bow = g.End.X - g.Start.X > NODE_WIDTH * 1.8f && Math.Abs(g.End.Y - g.Start.Y) < NODE_HEIGHT ? 70 + offset : 0;
                    g.Control1 = new PointF(g.Start.X + dx, g.Start.Y + bow);
                    g.Control2 = new PointF(g.End.X - dx, g.End.Y + bow);
                }
            }

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddBezier(g.Start, g.Control1, g.Control2, g.End);
                path.Flatten(null, 0.5f);
                g.Points = path.PathPoints;
            }
            return g;
        }



        // ------------------------------------------------------------------------------------
        // Mouse and keyboard
        // ------------------------------------------------------------------------------------

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            _mouseWorld = ScreenToWorld(e.Location);
            if (_graph == null) return;

            if (e.Button == MouseButtons.Left)
            {
                if (IsLinking)
                {
                    // Clicking the background keeps the link mode going
                    ActionNode target = HitTestNode(_mouseWorld);
                    if (target != null)
                    {
                        ActionNode source = _linkSource;
                        CancelLink();
                        LinkRequested?.Invoke(source, target);
                    }
                    return;
                }

                ActionNode node = HitTestNode(_mouseWorld);
                if (node != null)
                {
                    Select(node);
                    if (!Locked)
                    {
                        _dragNode = node;
                        _dragOffset = new PointF(_mouseWorld.X - (float)node.X, _mouseWorld.Y - (float)node.Y);
                        _dragMoved = false;
                    }
                    return;
                }

                ActionLink link = HitTestLink(_mouseWorld);
                if (link != null)
                {
                    Select(link);
                    return;
                }

                ClearSelection();
                StartPan(e.Location);
            }
            else if (e.Button == MouseButtons.Middle)
            {
                StartPan(e.Location);
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (IsLinking)
                {
                    CancelLink();
                    return;
                }

                ActionNode node = HitTestNode(_mouseWorld);
                if (node != null)
                {
                    Select(node);
                }
                else
                {
                    ActionLink link = HitTestLink(_mouseWorld);
                    if (link != null) Select(link);
                    else ClearSelection();
                }
                ContextMenuRequested?.Invoke(_mouseWorld, e.Location);
            }
        }

        private void StartPan(Point screen)
        {
            _panning = true;
            _panStart = screen;
            _panOrigin = _pan;
            Cursor = Cursors.SizeAll;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _mouseWorld = ScreenToWorld(e.Location);

            if (_dragNode != null && (e.Button & MouseButtons.Left) != 0)
            {
                _dragNode.X = _mouseWorld.X - _dragOffset.X;
                _dragNode.Y = _mouseWorld.Y - _dragOffset.Y;
                _dragMoved = true;
                Invalidate();
            }
            else if (_panning)
            {
                _pan = new PointF(_panOrigin.X + (e.X - _panStart.X), _panOrigin.Y + (e.Y - _panStart.Y));
                Invalidate();
                ViewChanged?.Invoke();
            }
            else if (IsLinking)
            {
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_dragNode != null)
            {
                bool moved = _dragMoved;
                _dragNode = null;
                if (moved) GraphChanged?.Invoke();
            }
            if (_panning)
            {
                _panning = false;
            }
            Cursor = IsLinking ? Cursors.Cross : Cursors.Default;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            ZoomAt(e.Location, e.Delta);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Escape || keyData == Keys.Delete) return true;
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape && IsLinking)
            {
                CancelLink();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete && !Locked && !IsLinking)
            {
                DeleteRequested?.Invoke();
                e.Handled = true;
            }
        }



        // ------------------------------------------------------------------------------------
        // Painting
        // ------------------------------------------------------------------------------------

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            DrawGrid(g);

            if (_graph == null)
            {
                TextRenderer.DrawText(g, "No action preset. Create one with the New button.", Font, ClientRectangle, Color.Gray,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.TranslateTransform(_pan.X, _pan.Y);
            g.ScaleTransform(_zoom, _zoom);

            RebuildGeometries();
            foreach (LinkGeometry ghost in _ghostGeometries)
            {
                DrawGhostLink(g, ghost);
            }
            foreach (ActionLink link in _graph.Links)
            {
                if (link != _selectedLink && _geometries.TryGetValue(link, out LinkGeometry geometry)) DrawLink(g, link, geometry);
            }
            // The selected link goes over the others
            if (_selectedLink != null && _geometries.TryGetValue(_selectedLink, out LinkGeometry selectedGeometry))
            {
                DrawLink(g, _selectedLink, selectedGeometry);
            }
            foreach (ActionNode node in _graph.Nodes)
            {
                DrawNode(g, node);
            }
            // The macros sharing the name of the selected one: a link to it goes to any of them
            if (_selectedNode != null && _selectedNode.IsMacro)
            {
                foreach (ActionNode sibling in _graph.GroupOf(_selectedNode))
                {
                    if (sibling != _selectedNode) DrawOutline(g, GetNodeBounds(sibling), 8, SELECTED_NODE_COLOR, true);
                }
            }
            // The labels go over everything, or a node right next to another would hide them
            foreach (ActionLink link in _graph.Links)
            {
                if (link != _selectedLink && _geometries.TryGetValue(link, out LinkGeometry geometry)) DrawLinkLabel(g, link, geometry);
            }
            if (_selectedLink != null && _geometries.TryGetValue(_selectedLink, out LinkGeometry selectedLabelGeometry))
            {
                DrawLinkLabel(g, _selectedLink, selectedLabelGeometry);
            }
            if (IsLinking) DrawRubberBand(g);

            g.ResetTransform();
        }

        private Color GetLinkColor(ActionLink link)
        {
            if (link == _selectedLink) return SELECTED_COLOR;
            if (link.Condition != null && link.Condition.NeedsSample) return WARNING_COLOR;
            ActionNode from = _graph.FindNode(link.FromId);
            return from != null && from.Kind == ActionNodeKind.Start ? START_COLOR : LINK_COLOR;
        }

        private void DrawGrid(Graphics g)
        {
            float step = GRID_SIZE * _zoom;
            // Too dense to be useful when zoomed far out
            while (step < 12) step *= 4;

            using (Pen pen = new Pen(GRID_COLOR, 1))
            {
                float x0 = _pan.X % step;
                if (x0 < 0) x0 += step;
                for (float x = x0; x < Width; x += step) g.DrawLine(pen, x, 0, x, Height);
                float y0 = _pan.Y % step;
                if (y0 < 0) y0 += step;
                for (float y = y0; y < Height; y += step) g.DrawLine(pen, 0, y, Width, y);
            }
        }

        private void DrawLink(Graphics g, ActionLink link, LinkGeometry geometry)
        {
            bool selected = link == _selectedLink;
            Color color = GetLinkColor(link);
            float width = selected ? 3f : 2f;

            using (Pen pen = new Pen(color, width))
            {
                g.DrawBezier(pen, geometry.Start, geometry.Control1, geometry.Control2, geometry.End);
            }
            DrawArrowHead(g, geometry, color);
        }

        // Where a link also goes, because the target shares its name with other macros
        private void DrawGhostLink(Graphics g, LinkGeometry geometry)
        {
            Color color = Color.FromArgb(130, LINK_COLOR);
            using (Pen pen = new Pen(color, 1.5f) { DashStyle = DashStyle.Dash })
            {
                g.DrawBezier(pen, geometry.Start, geometry.Control1, geometry.Control2, geometry.End);
            }
            DrawArrowHead(g, geometry, color);
        }

        // Arrow head at the end, pointing along the curve
        private static void DrawArrowHead(Graphics g, LinkGeometry geometry, Color color)
        {
            PointF beforeEnd = BezierPoint(geometry, 0.96f);
            float dx = geometry.End.X - beforeEnd.X;
            float dy = geometry.End.Y - beforeEnd.Y;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            if (length > 0.01f)
            {
                dx /= length;
                dy /= length;
                PointF tip = geometry.End;
                PointF baseCenter = new PointF(tip.X - dx * ARROW_SIZE, tip.Y - dy * ARROW_SIZE);
                PointF side = new PointF(-dy * ARROW_SIZE * 0.5f, dx * ARROW_SIZE * 0.5f);
                using (Brush brush = new SolidBrush(color))
                {
                    g.FillPolygon(brush, new[]
                    {
                        tip,
                        new PointF(baseCenter.X + side.X, baseCenter.Y + side.Y),
                        new PointF(baseCenter.X - side.X, baseCenter.Y - side.Y)
                    });
                }
            }

        }

        // The condition, in a little box just above the middle of the curve
        private void DrawLinkLabel(Graphics g, ActionLink link, LinkGeometry geometry)
        {
            if (link.Condition == null) return;

            bool selected = link == _selectedLink;
            bool warning = link.Condition.NeedsSample;
            Color color = GetLinkColor(link);
            string text = link.Condition.DescribeShort();
            Font font = selected ? _labelBoldFont : _labelFont;
            SizeF size = g.MeasureString(text, font);
            PointF middle = BezierPoint(geometry, 0.5f);
            RectangleF box = new RectangleF(middle.X - size.Width / 2 - 4, middle.Y - size.Height - 6, size.Width + 8, size.Height + 4);
            using (GraphicsPath path = RoundedRectangle(box, 5))
            using (Brush fill = new SolidBrush(Color.FromArgb(245, 255, 255, 255)))
            using (Pen border = new Pen(color, 1))
            using (Brush textBrush = new SolidBrush(warning && !selected ? WARNING_COLOR : NODE_TEXT))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
                g.DrawString(text, font, textBrush, box.X + 4, box.Y + 2);
            }
        }

        private void DrawNode(Graphics g, ActionNode node)
        {
            RectangleF bounds = GetNodeBounds(node);
            bool selected = node == _selectedNode;
            bool running = _runningNodeId != null && node.Id == _runningNodeId;

            if (!node.IsMacro)
            {
                Color fill = node.Kind == ActionNodeKind.Start ? START_COLOR : END_COLOR;
                using (GraphicsPath path = RoundedRectangle(bounds, bounds.Height / 2))
                using (Brush brush = new SolidBrush(fill))
                using (Pen border = new Pen(selected ? SELECTED_NODE_COLOR : Darken(fill), selected ? 3f : 1.5f))
                using (StringFormat format = CenteredFormat())
                {
                    g.FillPath(brush, path);
                    g.DrawPath(border, path);
                    g.DrawString(node.Kind == ActionNodeKind.Start ? "START" : "END", _pillFont, Brushes.White, bounds, format);
                }
                if (node == _linkSource) DrawLinkSourceOutline(g, bounds, bounds.Height / 2);
                return;
            }

            // A disabled macro is grayed out, the links to it go elsewhere
            bool disabled = !node.Enabled;
            Color borderColor = selected ? SELECTED_NODE_COLOR : (running ? RUNNING_COLOR : (disabled ? DISABLED_BORDER : NODE_BORDER));
            float borderWidth = selected ? 3f : (running ? 2.5f : 1.5f);
            using (GraphicsPath path = RoundedRectangle(bounds, 8))
            using (Brush fill = new SolidBrush(running ? RUNNING_FILL : (disabled ? DISABLED_FILL : Color.White)))
            using (Pen border = new Pen(borderColor, borderWidth) { DashStyle = disabled ? DashStyle.Dot : DashStyle.Solid })
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
            }

            // The name, with room left for the FINAL badge
            float badgeWidth = node.IsFinal ? 44 : 0;
            RectangleF titleRect = new RectangleF(bounds.X + 10, bounds.Y + 8, bounds.Width - 20 - badgeWidth, 20);
            RectangleF subtitleRect = new RectangleF(bounds.X + 10, bounds.Y + 30, bounds.Width - 20, 18);
            using (StringFormat format = new StringFormat(StringFormatFlags.NoWrap) { Trimming = StringTrimming.EllipsisCharacter })
            using (Brush titleBrush = new SolidBrush(disabled ? DISABLED_TEXT : NODE_TEXT))
            using (Brush subtitleBrush = new SolidBrush(disabled ? DISABLED_TEXT : NODE_SUBTEXT))
            {
                g.DrawString(node.DisplayName, _titleFont, titleBrush, titleRect, format);
                g.DrawString((disabled ? "DISABLED. " : "") + node.DescribeMacro(), _subtitleFont, subtitleBrush, subtitleRect, format);
            }

            if (node.IsFinal)
            {
                RectangleF badge = new RectangleF(bounds.Right - 48, bounds.Y + 7, 40, 14);
                using (GraphicsPath path = RoundedRectangle(badge, 4))
                using (Brush brush = new SolidBrush(FINAL_FILL))
                using (Pen pen = new Pen(FINAL_BORDER, 1))
                using (Brush textBrush = new SolidBrush(Darken(FINAL_BORDER)))
                using (StringFormat format = CenteredFormat())
                {
                    g.FillPath(brush, path);
                    g.DrawPath(pen, path);
                    g.DrawString("FINAL", _badgeFont, textBrush, badge, format);
                }
            }

            if (node == _linkSource) DrawLinkSourceOutline(g, bounds, 8);
        }

        private void DrawLinkSourceOutline(Graphics g, RectangleF bounds, float radius)
        {
            DrawOutline(g, bounds, radius, SELECTED_COLOR, true);
        }

        // A line a little outside the node
        private static void DrawOutline(Graphics g, RectangleF bounds, float radius, Color color, bool dashed)
        {
            bounds.Inflate(4, 4);
            using (GraphicsPath path = RoundedRectangle(bounds, radius + 4))
            using (Pen pen = new Pen(color, dashed ? 1.8f : 2.5f) { DashStyle = dashed ? DashStyle.Dash : DashStyle.Solid })
            {
                g.DrawPath(pen, path);
            }
        }

        private void DrawRubberBand(Graphics g)
        {
            PointF start = OutAnchor(_linkSource, 0);
            ActionNode hovered = HitTestNode(_mouseWorld);
            PointF end = hovered != null && hovered != _linkSource ? InAnchor(hovered, 0) : _mouseWorld;
            using (Pen pen = new Pen(SELECTED_COLOR, 2f) { DashStyle = DashStyle.Dash })
            {
                g.DrawLine(pen, start, end);
            }
            if (hovered != null && hovered != _linkSource)
            {
                RectangleF bounds = GetNodeBounds(hovered);
                DrawLinkSourceOutline(g, bounds, hovered.IsMacro ? 8 : bounds.Height / 2);
            }
        }

        private static StringFormat CenteredFormat()
        {
            return new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        }

        private static Color Darken(Color color)
        {
            return Color.FromArgb(color.R * 3 / 4, color.G * 3 / 4, color.B * 3 / 4);
        }

        private static GraphicsPath RoundedRectangle(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float d = Math.Min(radius * 2, Math.Min(rect.Width, rect.Height));
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
