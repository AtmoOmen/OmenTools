using System.Numerics;

namespace OmenTools.Infos;

public class Polygon
{
    private readonly Vector2[] _vertices;
    private const float Epsilon = 1e-6f;

    public Polygon(Vector2[] vertices)
    {
        _vertices = OptimizePolygon(vertices);
        if (!IsValidPolygon(_vertices))
            throw new ArgumentException("无法构成一个有效的多边形");
    }

    public bool IsPointInPolygon(Vector2 point)
        => IsPointInside(_vertices, point);

    private static Vector2[] OptimizePolygon(Vector2[] vertices)
    {
        // 移除重复点
        vertices = RemoveDuplicatePoints(vertices);

        // 移除共线点
        vertices = RemoveCollinearPoints(vertices);

        // 修复自相交
        vertices = FixSelfIntersections(vertices);

        return vertices;
    }

    private static Vector2[] RemoveDuplicatePoints(Vector2[] vertices)
        => vertices.Distinct().ToArray();

    private static Vector2[] RemoveCollinearPoints(Vector2[] vertices)
    {
        var result = new List<Vector2>();
        var n = vertices.Length;
        for (var i = 0; i < n; i++)
        {
            var prev = vertices[(i - 1 + n) % n];
            var curr = vertices[i];
            var next = vertices[(i + 1) % n];

            if (Math.Abs(Orientation(prev, curr, next)) > Epsilon)
            {
                result.Add(curr);
            }
        }
        return result.ToArray();
    }

    private static Vector2[] FixSelfIntersections(Vector2[] vertices)
    {
        var result = new List<Vector2>(vertices);
        bool intersectionFound;
        do
        {
            intersectionFound = false;
            for (var i = 0; i < result.Count; i++)
            {
                for (var j = i + 2; j < result.Count; j++)
                {
                    if (i == 0 && j == result.Count - 1) continue;

                    var p1 = result[i];
                    var q1 = result[(i + 1) % result.Count];
                    var p2 = result[j];
                    var q2 = result[(j + 1) % result.Count];

                    if (SegmentsIntersect(p1, q1, p2, q2))
                    {
                        // 移除导致相交的点
                        result.RemoveAt((j + 1) % result.Count);
                        if (i > j) i--;
                        intersectionFound = true;
                        break;
                    }
                }
                if (intersectionFound) break;
            }
        } while (intersectionFound && result.Count > 3);

        return result.ToArray();
    }

    // 判断点是否在多边形内 - 射线法
    private static bool IsPointInside(Vector2[] vertices, Vector2 point)
    {
        var count = vertices.Length;
        var result = false;

        for (int i = 0, j = count - 1; i < count; j = i++)
        {
            if ((vertices[i].Y > point.Y) != (vertices[j].Y > point.Y) &&
                point.X < ((vertices[j].X - vertices[i].X) * (point.Y - vertices[i].Y) /
                           (vertices[j].Y - vertices[i].Y)) + vertices[i].X)
                result = !result;
        }

        return result;
    }

    private static bool IsValidPolygon(Vector2[] points)
    {
        if (points.Length < 3) return false;

        // 共线判断
        if (!IsConvexOrConcave(points)) return false;

        // 自相交判断
        if (HasSelfIntersections(points)) return false;

        return true;
    }

    // 判断点顺序 - 面积法 (顺时针为负, 逆时针为正)
    private static bool IsConvexOrConcave(Vector2[] points)
    {
        float sum = 0;
        for (var i = 0; i < points.Length; i++)
        {
            var current = points[i];
            var next = points[(i + 1) % points.Length];
            sum += (next.X - current.X) * (next.Y + current.Y);
        }

        return Math.Abs(sum) > Epsilon;
    }

    // 自相交判断
    private static bool HasSelfIntersections(Vector2[] points)
    {
        var n = points.Length;
        for (var i = 0; i < n; i++)
        {
            for (var j = i + 2; j < n; j++)
            {
                // 防止相邻边检测重叠
                if (i == 0 && j == n - 1) continue;

                if (SegmentsIntersect(points[i], points[(i + 1) % n], points[j], points[(j + 1) % n]))
                    return true;
            }
        }
        return false;
    }

    // 线段相交判断
    private static bool SegmentsIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
    {
        var o1 = Orientation(p1, q1, p2);
        var o2 = Orientation(p1, q1, q2);
        var o3 = Orientation(p2, q2, p1);
        var o4 = Orientation(p2, q2, q1);

        // 考虑容差
        if (Math.Abs(o1) < Epsilon) o1 = 0;
        if (Math.Abs(o2) < Epsilon) o2 = 0;
        if (Math.Abs(o3) < Epsilon) o3 = 0;
        if (Math.Abs(o4) < Epsilon) o4 = 0;

        // 主要相交检查
        if (o1 != o2 && o3 != o4) return true;

        // 共线情况的特殊处理
        if (o1 == 0 && OnSegment(p1, p2, q1)) return true;
        if (o2 == 0 && OnSegment(p1, q2, q1)) return true;
        if (o3 == 0 && OnSegment(p2, p1, q2)) return true;
        if (o4 == 0 && OnSegment(p2, q1, q2)) return true;

        return false;
    }

    // 计算三点排列方向
    private static float Orientation(Vector2 p, Vector2 q, Vector2 r)
        => (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);

    // 判断点是否在线段上
    private static bool OnSegment(Vector2 p, Vector2 q, Vector2 r) =>
        q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
        q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y);
}
