using System.Numerics;

namespace OmenTools.Infos;

public class Polygon
{
    private readonly Vector2[] _vertices;

    public Polygon(Vector2[] vertices)
    {
        if (!IsValidPolygon(vertices))
            throw new ArgumentException("传入的点无法构成一个有效的多边形");

        _vertices = vertices;
    }

    public bool IsPointInPolygon(Vector2 point)
        => IsPointInside(_vertices, point);

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

        return sum != 0;
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

        return o1 != o2 && o3 != o4;
    }

    // 计算三点排列方向
    private static float Orientation(Vector2 p, Vector2 q, Vector2 r)
        => (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);
}
