using System.Numerics;

namespace OmenTools.Infos;

public class Polygon
{
    private readonly Vector2[] vertices;
    private const    float     Epsilon = 1e-6f;

    /// <summary>
    ///     创建一个多边形
    /// </summary>
    /// <param name="vertices">多边形的顶点集合</param>
    /// <exception cref="ArgumentException">当无法构成有效多边形时抛出</exception>
    public Polygon(IEnumerable<Vector2> vertices)
    {
        this.vertices = OptimizePolygon(vertices.ToArray());
        if (this.vertices.Length < 3)
            throw new ArgumentException("无法构成一个有效的多边形");
    }

    /// <summary>
    ///     判断点是否在多边形内部
    /// </summary>
    public bool IsPointInPolygon(Vector2 point)
    {
        var result = false;
        for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
        {
            if ((vertices[i].Y > point.Y) != (vertices[j].Y > point.Y) &&
                point.X                    < ((vertices[j].X - vertices[i].X) * (point.Y - vertices[i].Y) / (vertices[j].Y - vertices[i].Y)) + vertices[i].X)
                result = !result;
        }

        return result;
    }

    /// <summary>
    ///     生成多边形内部任意一点（非边上的点）
    /// </summary>
    /// <returns>多边形内部的随机点</returns>
    /// <exception cref="InvalidOperationException">如果无法生成有效的内部点</exception>
    public Vector2 GenerateRandomInnerPoint()
    {
        if (vertices.Length < 3)
            throw new InvalidOperationException("多边形无效，无法生成内部点");

        var centroid = CalculateCentroid();

        if (IsPointInPolygon(centroid) && !IsPointOnBoundary(centroid))
            return centroid;

        for (var attempts = 0; attempts < 100; attempts++)
        {
            var idx1 = Random.Shared.Next(vertices.Length);
            var idx2 = Random.Shared.Next(vertices.Length);
            var idx3 = Random.Shared.Next(vertices.Length);

            var w1 = (float)Random.Shared.NextDouble();
            var w2 = (float)Random.Shared.NextDouble() * (1 - w1);
            var w3 = 1 - w1 - w2;

            var candidate = new Vector2(
                (vertices[idx1].X * w1) + (vertices[idx2].X * w2) + (vertices[idx3].X * w3),
                (vertices[idx1].Y * w1) + (vertices[idx2].Y * w2) + (vertices[idx3].Y * w3)
            );

            if (IsPointInPolygon(candidate) && !IsPointOnBoundary(candidate))
                return candidate;
        }

        const float shrinkFactor = 0.1f;
        var         innerPoint   = centroid;

        foreach (var t in vertices)
        {
            var     direction = t - centroid;
            direction  = Vector2.Normalize(direction) * shrinkFactor * Vector2.Distance(centroid, t);
            innerPoint = centroid + direction;

            if (IsPointInPolygon(innerPoint) && !IsPointOnBoundary(innerPoint))
                return innerPoint;
        }

        throw new InvalidOperationException("无法在多边形内生成有效的点");
    }

    /// <summary>
    ///     判断点是否在多边形的边界上
    /// </summary>
    private bool IsPointOnBoundary(Vector2 point)
    {
        for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
        {
            if (IsPointOnLineSegment(point, vertices[i], vertices[j]))
                return true;
        }

        return false;
    }

    /// <summary>
    ///     判断点是否在线段上
    /// </summary>
    private static bool IsPointOnLineSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        var crossProduct = ((point.Y - start.Y) * (end.X - start.X)) - ((point.X - start.X) * (end.Y - start.Y));

        if (Math.Abs(crossProduct) > Epsilon)
            return false;

        var dotProduct = ((point.X - start.X) * (end.X - start.X)) + ((point.Y - start.Y) * (end.Y - start.Y));
        if (dotProduct < 0)
            return false;

        var squaredLength = ((end.X - start.X) * (end.X - start.X)) + ((end.Y - start.Y) * (end.Y - start.Y));
        return !(dotProduct > squaredLength);
    }

    /// <summary>
    ///     计算多边形的重心
    /// </summary>
    private Vector2 CalculateCentroid()
    {
        var   centroid   = Vector2.Zero;
        var   signedArea = 0.0f;

        for (var i = 0; i < vertices.Length; i++)
        {
            var x0 = vertices[i].X;
            var y0 = vertices[i].Y;
            var x1 = vertices[(i + 1) % vertices.Length].X;
            var y1 = vertices[(i + 1) % vertices.Length].Y;

            var a = (x0 * y1) - (x1 * y0);
            signedArea += a;

            centroid.X += (x0 + x1) * a;
            centroid.Y += (y0 + y1) * a;
        }

        signedArea *= 0.5f;

        if (Math.Abs(signedArea) < Epsilon)
        {
            return new Vector2(
                vertices.Sum(v => v.X) / vertices.Length,
                vertices.Sum(v => v.Y) / vertices.Length
            );
        }

        centroid.X /= 6.0f * signedArea;
        centroid.Y /= 6.0f * signedArea;

        return centroid;
    }

    /// <summary>
    ///     优化多边形顶点
    /// </summary>
    private static Vector2[] OptimizePolygon(Vector2[] vertices)
    {
        if (vertices == null || vertices.Length < 3)
            return vertices ?? [];

        // 去除重复顶点和距离过近的点
        var uniqueVertices = new List<Vector2>();
        foreach (var point in vertices)
        {
            var isDuplicate = false;
            foreach (var existing in uniqueVertices)
            {
                if (Vector2.Distance(existing, point) < Epsilon * 10)
                {
                    isDuplicate = true;
                    break;
                }
            }

            if (!isDuplicate)
                uniqueVertices.Add(point);
        }

        // 如果剩余点太少，无法构成多边形
        if (uniqueVertices.Count < 3)
            return uniqueVertices.ToArray();

        // 尝试修复自相交的多边形
        uniqueVertices = FixSelfIntersection(uniqueVertices);

        // 确保点的顺序是顺时针或逆时针
        return EnsureSimplePolygon(uniqueVertices.ToArray());
    }

    /// <summary>
    ///     尝试修复自相交的多边形
    /// </summary>
    private static List<Vector2> FixSelfIntersection(List<Vector2> vertices)
    {
        // 如果点数太少，不可能有自相交
        if (vertices.Count <= 3)
            return vertices;

        // 尝试检测并移除导致自相交的点
        var  result = new List<Vector2>(vertices);
        bool changed;

        do
        {
            changed = false;
            for (var i = 0; i < result.Count; i++)
            {
                var prev    = result[(i - 1 + result.Count) % result.Count];
                var current = result[i];
                var next    = result[(i + 1) % result.Count];

                // 检查当前点是否使得多边形非常不规则（比如尖角）
                var v1         = Vector2.Normalize(current - prev);
                var v2         = Vector2.Normalize(next    - current);
                var dotProduct = Vector2.Dot(v1, v2);

                // 如果点会导致很尖的角或者反向的边，考虑移除
                if (dotProduct < -0.9f)
                {
                    result.RemoveAt(i);
                    changed = true;
                    break;
                }
            }
        }
        while (changed && result.Count > 3);

        return result;
    }

    /// <summary>
    ///     确保多边形是简单的（无自相交）
    /// </summary>
    private static Vector2[] EnsureSimplePolygon(Vector2[] vertices)
    {
        if (vertices.Length <= 3)
            return vertices;

        // 计算凸包作为退化情况（如果无法形成简单多边形）
        try
        {
            // 尝试按角度排序构建多边形
            var centroid = new Vector2(
                vertices.Average(v => v.X),
                vertices.Average(v => v.Y)
            );

            // 按照与质心连线的角度排序点
            return vertices
                   .Select(v => (
                                    Point: v,
                                    Angle: Math.Atan2(v.Y - centroid.Y, v.X - centroid.X)
                                ))
                   .OrderBy(item => item.Angle)
                   .Select(item => item.Point)
                   .ToArray();
        }
        catch
        {
            // 如果排序方法失败，尝试使用凸包
            return ComputeConvexHull(vertices);
        }
    }

    /// <summary>
    ///     使用 Graham 扫描法计算凸包
    /// </summary>
    private static Vector2[] ComputeConvexHull(Vector2[] points)
    {
        var n = points.Length;
        if (n <= 3)
            return points;

        var pivot = 0;
        for (var i = 1; i < n; i++)
        {
            if (points[i].Y < points[pivot].Y ||
                (Math.Abs(points[i].Y - points[pivot].Y) < Epsilon && points[i].X < points[pivot].X))
                pivot = i;
        }

        (points[0], points[pivot]) = (points[pivot], points[0]);

        Array.Sort(points, 1, n - 1, new PolarAngleComparer(points[0]));

        var m = 1;
        for (var i = 2; i < n; i++)
        {
            while (i < n && CrossProduct(points[0], points[m], points[i]) == 0)
            {
                if (SquaredDistance(points[0], points[i]) > SquaredDistance(points[0], points[m]))
                    points[m] = points[i];
                i++;
            }

            if (i < n)
                points[++m] = points[i];
        }

        if (m + 1 < 3)
            return points.Take(m + 1).ToArray();

        var stack = new List<Vector2>(m + 1)
        {
            points[0],
            points[1]
        };

        for (var i = 2; i <= m; i++)
        {
            while (stack.Count > 1 && CrossProduct(stack[^2], stack[^1], points[i]) <= 0) 
                stack.RemoveAt(stack.Count - 1);
            stack.Add(points[i]);
        }

        return stack.ToArray();
    }

    /// <summary>
    ///     计算向量叉积
    /// </summary>
    private static float CrossProduct(Vector2 p0, Vector2 p1, Vector2 p2) => 
        ((p1.X - p0.X) * (p2.Y - p0.Y)) - ((p2.X - p0.X) * (p1.Y - p0.Y));

    /// <summary>
    ///     计算两点间的平方距离
    /// </summary>
    private static float SquaredDistance(Vector2 p1, Vector2 p2)
    {
        var dx = p2.X - p1.X;
        var dy = p2.Y - p1.Y;
        return (dx * dx) + (dy * dy);
    }

    /// <summary>
    ///     极角比较器
    /// </summary>
    private class PolarAngleComparer(Vector2 Pivot) : IComparer<Vector2>
    {
        public int Compare(Vector2 p1, Vector2 p2)
        {
            var cross = CrossProduct(Pivot, p1, p2);

            if (Math.Abs(cross) < Epsilon) return SquaredDistance(Pivot, p1).CompareTo(SquaredDistance(Pivot, p2));

            return cross > 0 ? -1 : 1;
        }
    }
}
