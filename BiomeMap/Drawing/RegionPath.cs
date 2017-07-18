using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClipperLib;
using Newtonsoft.Json;

namespace BiomeMap.Drawing
{
    public class RegionPath
    {
        public IntPoint[][] Points { get; private set; } = new IntPoint[0][];

        private List<Point> _rawPoints = new List<Point>();

        private bool _isDirty;

        public void AddPoint(int x, int y)
        {
            var p = new Point(x, y);
            if (_rawPoints.Contains(p)) return;

            _rawPoints.Add(p);
            _isDirty = true;
            //RecalculateBlocks();
            //RecalculateOutline();
        }

        public void RecalculateBlocks()
        {
            if (!_isDirty) return;
            _isDirty = false;

            var c = new Clipper();
            var rects = new List<List<IntPoint>>();

            foreach (var p in _rawPoints.ToArray())
            {
                var polygon = new List<IntPoint>();
                //var r = new Rectangle(p.X, p.Y, 1, 1);
                polygon.Add(new IntPoint(p.X, p.Y));
                polygon.Add(new IntPoint(p.X+1, p.Y));
                polygon.Add(new IntPoint(p.X+1, p.Y+1));
                polygon.Add(new IntPoint(p.X, p.Y+1));
                rects.Add(polygon);
            }

            var paths = new List<List<IntPoint>>();

            foreach (var r in rects)
            {
                c.AddPolygon(r, PolyType.ptClip);
            }

            c.Execute(ClipType.ctUnion, paths, PolyFillType.pftNonZero, PolyFillType.pftNonZero);


            Points = paths.Select(p => p.ToArray()).ToArray();
        }

        private void RecalculateOutline()
        {
            var rawEdges = new List<PointF>();
            var points = _rawPoints.ToArray();

            foreach (var p in points)
            {
                rawEdges.Add(new PointF(p.X + 0.5f, p.Y));
                rawEdges.Add(new PointF(p.X, p.Y + 0.5f));
                rawEdges.Add(new PointF(p.X + 0.5f, p.Y + 1));
                rawEdges.Add(new PointF(p.X + 1, p.Y + 0.5f));
            }

            var counts = new Dictionary<PointF, int>();
            foreach (var p in rawEdges.ToArray())
            {
                if (!counts.ContainsKey(p))
                    counts[p] = 0;

                counts[p]++;
            }

            var edges = counts.Where(c => c.Value == 1).Select(kvp => kvp.Key).ToList();

            if (!edges.Any()) return;

            var point = edges.FirstOrDefault();

            var orderedPoints = new List<Point>();

            var valid = true;
            while (valid)
            {
                edges.Remove(point);

                if (Math.Abs(Math.Floor(point.X) - point.X) > 0.1)
                {
                    orderedPoints.Add(new Point((int)Math.Floor(point.X), (int)Math.Floor(point.Y)));
                    orderedPoints.Add(new Point((int)Math.Floor(point.X+1), (int)Math.Floor(point.Y)));
                }

                if (Math.Abs(Math.Floor(point.Y) - point.Y) > 0.1)
                {
                    orderedPoints.Add(new Point((int)Math.Floor(point.X), (int)Math.Floor(point.Y)));
                    orderedPoints.Add(new Point((int)Math.Floor(point.X), (int)Math.Floor(point.Y)+1));
                }

                var right = new PointF(point.X+1, point.Y);
                var bottom = new PointF(point.X, point.Y-1);
                var left = new PointF(point.X - 1, point.Y);
                var top = new PointF(point.X, point.Y+1);

                // check clockwise
                if (edges.Contains(right))
                {
                    point = right;
                }
                else if (edges.Contains(bottom))
                {
                    point = bottom;
                }
                else if (edges.Contains(left))
                {
                    point = left;
                }
                else if (edges.Contains(top))
                {
                    point = top;
                }
                else
                {
                    valid = false;
                }
            }

            //Points = orderedPoints.ToArray();
        }

        private bool HasUpper(IEnumerable<Point> points, Point point)
        {
            return points.Contains(new Point(point.X, point.Y + 1));
        }

        private bool HasLower(IEnumerable<Point> points, Point point)
        {
            return points.Contains(new Point(point.X, point.Y - 1));
        }
        private bool HasLeft(IEnumerable<Point> points, Point point)
        {
            return points.Contains(new Point(point.X-1, point.Y));
        }

        private bool HasRight(IEnumerable<Point> points, Point point)
        {
            return points.Contains(new Point(point.X+1, point.Y));
        }
    }
}
