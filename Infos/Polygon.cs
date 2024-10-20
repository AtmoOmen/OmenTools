using System.Numerics;

namespace OmenTools.Infos;

public class Polygon
{
    private readonly Vector2[] _vertices;
    private const float Epsilon = 1e-6f;

    public Polygon(IEnumerable<Vector2> vertices)
    {
        _vertices = OptimizePolygon(vertices.ToArray());
        if (_vertices.Length < 3)
            throw new ArgumentException("无法构成一个有效的多边形");
    }

    public bool IsPointInPolygon(Vector2 point)
    {
        var result = false;
        for (int i = 0, j = _vertices.Length - 1; i < _vertices.Length; j = i++)
        {
            if ((_vertices[i].Y > point.Y) != (_vertices[j].Y > point.Y) &&
                (point.X < (_vertices[j].X - _vertices[i].X) * (point.Y - _vertices[i].Y) / (_vertices[j].Y - _vertices[i].Y) + _vertices[i].X))
            {
                result = !result;
            }
        }
        return result;
    }

    private static Vector2[] OptimizePolygon(Vector2[] vertices)
    {
        vertices = vertices.Distinct().ToArray();
        var result = new List<Vector2>();

        for (var i = 0; i < vertices.Length; i++)
        {
            var prev = vertices[(i - 1 + vertices.Length) % vertices.Length];
            var curr = vertices[i];
            var next = vertices[(i + 1) % vertices.Length];

            if (Math.Abs((next.Y - prev.Y) * (curr.X - prev.X) - (next.X - prev.X) * (curr.Y - prev.Y)) > Epsilon)
                result.Add(curr);
        }

        return result.ToArray();
    }
}