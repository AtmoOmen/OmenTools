using System.Collections.Concurrent;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using OmenTools.Interop.Game.Lumina;
using OmenTools.OmenService.Abstractions;
using Action = Lumina.Excel.Sheets.Action;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace OmenTools.OmenService;

public sealed class AOEManager : OmenServiceBase<AOEManager>
{
    #region 公开

    public bool IsInAnyDangerZone(out Vector3 nearestSafePoint) =>
        IsInAnyDangerZone(LocalPlayerState.Object?.Position ?? Vector3.Zero, out nearestSafePoint);

    public bool IsInAnyDangerZone(Vector3 pos, out Vector3 nearestSafePoint)
    {
        var exit = FindNearestExit(pos);
        nearestSafePoint = exit ?? pos;
        return exit != null;
    }

    #endregion

    private readonly ConcurrentDictionary<uint, AOEShape> activeAOEs = new();

    protected override void Init()
    {
        UseActionManager.Instance().RegPostCharacterStartCast(OnCastStart);
        UseActionManager.Instance().RegPostCharacterCompleteCast(OnCastComplete);
    }

    protected override void Uninit()
    {
        UseActionManager.Instance().Unreg(OnCastStart);
        UseActionManager.Instance().Unreg(OnCastComplete);

        activeAOEs.Clear();
    }

    private void OnCastStart
    (
        bool         result,
        IBattleChara caster,
        ActionType   type,
        uint         actionID,
        nint         a4,
        float        rotation,
        float        a6
    )
    {
        if (!result) return;
        if (caster.ObjectKind != ObjectKind.BattleNpc) return;
        if (!LuminaGetter.TryGetRow(actionID, out Action actionRow)) return;

        var shape = AOEShape.FromAction(actionRow, caster);
        if (shape == null) return;

        activeAOEs[caster.EntityID] = shape;
    }

    private void OnCastComplete
    (
        bool         result,
        IBattleChara caster,
        ActionType   type,
        uint         actionID,
        uint         spellID,
        GameObjectId animationTargetID,
        Vector3      location,
        float        rotation,
        short        lastUsedActionSequence,
        int          animationVariation,
        int          ballistaEntityID
    )
    {
        if (caster.ObjectKind != ObjectKind.BattleNpc) return;
        activeAOEs.TryRemove(caster.EntityID, out _);
    }

    private Vector3? FindNearestExit(Vector3 pos)
    {
        var      bestDistSq = float.MaxValue;
        Vector3? best       = null;

        foreach (var kv in activeAOEs)
        {
            var aoe = kv.Value;
            if (!aoe.Contains(pos)) continue;

            var exit    = aoe.NearestExit(pos);
            var blocked = false;

            foreach (var okv in activeAOEs)
            {
                if (okv.Value.Contains(exit))
                {
                    blocked = true;
                    break;
                }
            }

            if (blocked) continue;

            var distSq = (new Vector2(pos.X, pos.Z) - new Vector2(exit.X, exit.Z)).LengthSquared();

            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best       = exit;
            }
        }

        return best;
    }



    #region AOE


    private abstract class AOEShape
    {
        public abstract bool Contains(Vector3 point);

        public abstract Vector3 NearestExit(Vector3 point);

        public static AOEShape? FromAction(Action data, IBattleChara caster)
        {
            var targetPos = caster.CastTargetObject?.Position ?? caster.Position;
            var rot       = caster.Rotation;

            return data.CastType switch
            {
                2  => new Circle(targetPos, data.EffectRange),
                3  => new Cone(caster.Position, data.EffectRange + caster.HitboxRadius, rot, ConeAngle(data)),
                4  => new Rect(caster.Position, data.EffectRange + caster.HitboxRadius, data.XAxisModifier * 0.5f, rot),
                5  => new Circle(targetPos, data.EffectRange     + caster.HitboxRadius),
                8  => new Rect(caster.Position, 1, data.XAxisModifier * 0.5f, rot),
                10 => new Donut(targetPos, DonutInner(data), data.EffectRange),
                11 => new Cross(targetPos, data.EffectRange, data.XAxisModifier * 0.5f, rot),
                12 => new Rect(targetPos, data.EffectRange, data.XAxisModifier  * 0.5f, rot),
                13 => new Cone(caster.Position, data.EffectRange, rot, ConeAngle(data)),
                _  => null
            };
        }

        private static float ConeAngle(Action data)
        {
            var omen = data.Omen.ValueNullable;
            if (omen == null)
                return DEFAULT_CONE_ANGLE;

            var path   = omen.Value.Path.ToString();
            var fanPos = path.IndexOf("fan", StringComparison.Ordinal);
            if (fanPos < 0 || fanPos + 6 > path.Length)
                return DEFAULT_CONE_ANGLE;

            return int.TryParse(path.AsSpan(fanPos + 3, 3), out var angle) ? angle : DEFAULT_CONE_ANGLE;
        }

        private static float DonutInner(Action data)
        {
            var omen = data.Omen.ValueNullable;
            if (omen == null) return 0;
            var path      = omen.Value.Path.ToString();
            var circlePos = path.IndexOf("circle", StringComparison.OrdinalIgnoreCase);
            if (circlePos < 0) return 0;
            var rest = path[(circlePos + 6)..];
            var end  = 0;
            while (end < rest.Length && char.IsDigit(rest[end])) end++;
            return end > 0 && int.TryParse(rest[..end], out var inner) ? inner : 0;
        }

        protected const float EXIT_MARGIN = 0.01f;

        protected static Vector2 ToXZ(Vector3 v) => new(v.X, v.Z);

        protected static Vector3 FromXZ(float x, float z, float y) => new(x, y, z);
    }

    private sealed class Rect
    (
        Vector3 origin,
        float   length,
        float   halfWidth,
        float   rot
    ) : AOEShape
    {
        private readonly Vector2 facing = new(MathF.Sin(rot), MathF.Cos(rot));
        private readonly Vector2 side   = new(-MathF.Cos(rot), MathF.Sin(rot));

        public override bool Contains(Vector3 point)
        {
            var dir   = ToXZ(point) - ToXZ(origin);
            var fDist = Vector2.Dot(dir, facing);
            var sDist = MathF.Abs(Vector2.Dot(dir, side));
            return fDist >= 0 && fDist <= length && sDist <= halfWidth;
        }

        public override Vector3 NearestExit(Vector3 point)
        {
            var dir   = ToXZ(point) - ToXZ(origin);
            var fDist = Vector2.Dot(dir, facing);
            var sDist = Vector2.Dot(dir, side);

            var dFront = length    - fDist;
            var dLeft  = halfWidth - sDist;
            var dRight = halfWidth + sDist;

            var min    = MathF.Min(MathF.Min(dFront, fDist), MathF.Min(dLeft, dRight));
            var margin = min + EXIT_MARGIN;

            if (min == dFront)
                return point with { X = point.X + (facing.X * margin), Z = point.Z + (facing.Y * margin) };
            if (min == fDist)
                return point with { X = point.X - (facing.X * margin), Z = point.Z - (facing.Y * margin) };
            if (min == dLeft)
                return point with { X = point.X - (side.X * margin), Z = point.Z - (side.Y * margin) };

            return point with { X = point.X + (side.X * margin), Z = point.Z + (side.Y * margin) };
        }
    }

    private sealed class Circle
    (
        Vector3 center,
        float   radius
    ) : AOEShape
    {
        public override bool Contains(Vector3 point) =>
            Vector2.DistanceSquared(ToXZ(point), ToXZ(center)) <= radius * radius;

        public override Vector3 NearestExit(Vector3 point)
        {
            var dir = ToXZ(point) - ToXZ(center);
            var len = dir.Length();
            if (len < 0.0001f) return center + new Vector3(radius + EXIT_MARGIN, 0, 0);
            var outward = dir / len * (radius                     + EXIT_MARGIN);
            return FromXZ(center.X + outward.X, center.Z + outward.Y, point.Y);
        }
    }

    private sealed class Cone
    (
        Vector3 center,
        float   radius,
        float   rot,
        float   ang
    ) : AOEShape
    {
        private readonly float   cosHalf = MathF.Cos(ang * 0.5f * MathF.PI / 180f);
        private readonly float   sinHalf = MathF.Sin(ang * 0.5f * MathF.PI / 180f);
        private readonly Vector2 facing  = new(MathF.Sin(rot), MathF.Cos(rot));

        public override bool Contains(Vector3 point)
        {
            var dir    = ToXZ(point) - ToXZ(center);
            var distSq = dir.LengthSquared();
            if (distSq > radius * radius) return false;
            var dot = Vector2.Dot(dir, facing);
            return dot > 0 && dot * dot >= distSq * cosHalf * cosHalf;
        }

        public override Vector3 NearestExit(Vector3 point)
        {
            var dir   = ToXZ(point) - ToXZ(center);
            var fDist = Vector2.Dot(dir, facing);
            if (fDist <= 0) return new Circle(center, 0.5f).NearestExit(point);

            var leftN  = new Vector2((-facing.X * cosHalf) - (facing.Y * sinHalf), (facing.Y  * cosHalf) - (facing.X * sinHalf));
            var rightN = new Vector2((facing.X  * cosHalf) - (facing.Y * sinHalf), (-facing.Y * cosHalf) - (facing.X * sinHalf));
            var dLeft  = Vector2.Dot(dir, leftN);
            var dRight = Vector2.Dot(dir, rightN);

            var arcExit = new Circle(center, radius).NearestExit(point);
            var arcDist = (ToXZ(point) - ToXZ(arcExit)).LengthSquared();

            var leftExit = point with { X = point.X - (leftN.X * (dLeft + EXIT_MARGIN)), Z = point.Z - (leftN.Y * (dLeft + EXIT_MARGIN)) };
            var leftDist = dLeft > 0 ? float.MaxValue : (ToXZ(point) - ToXZ(leftExit)).LengthSquared();

            var rightExit = point with { X = point.X - (rightN.X * (dRight + EXIT_MARGIN)), Z = point.Z - (rightN.Y * (dRight + EXIT_MARGIN)) };
            var rightDist = dRight > 0 ? float.MaxValue : (ToXZ(point) - ToXZ(rightExit)).LengthSquared();

            var minDist = MathF.Min(arcDist, MathF.Min(leftDist, rightDist));
            if (minDist == arcDist) return arcExit;
            return minDist == leftDist ? leftExit : rightExit;
        }
    }

    private sealed class Donut
    (
        Vector3 center,
        float   inner,
        float   outer
    ) : AOEShape
    {
        public override bool Contains(Vector3 point)
        {
            var distSq = Vector2.DistanceSquared(ToXZ(point), ToXZ(center));
            return distSq >= inner * inner && distSq <= outer * outer;
        }

        public override Vector3 NearestExit(Vector3 point)
        {
            var dir = ToXZ(point) - ToXZ(center);
            var len = dir.Length();

            if (len < inner && len > 0.0001f)
            {
                var outward = dir / len * (inner + EXIT_MARGIN);
                return FromXZ(center.X + outward.X, center.Z + outward.Y, point.Y);
            }

            return new Circle(center, outer).NearestExit(point);
        }
    }

    private sealed class Cross
    (
        Vector3 center,
        float   length,
        float   halfWidth,
        float   rot
    ) : AOEShape
    {
        private readonly Vector2 forward = new(MathF.Sin(rot), MathF.Cos(rot));
        private readonly Vector2 side    = new(-MathF.Cos(rot), MathF.Sin(rot));

        public override bool Contains(Vector3 point)
        {
            var dir   = ToXZ(point) - ToXZ(center);
            var fDist = MathF.Abs(Vector2.Dot(dir, forward));
            var sDist = MathF.Abs(Vector2.Dot(dir, side));
            return (fDist <= length && sDist <= halfWidth) || (sDist <= length && fDist <= halfWidth);
        }

        public override Vector3 NearestExit(Vector3 point)
        {
            var dir     = ToXZ(point) - ToXZ(center);
            var fDist   = MathF.Abs(Vector2.Dot(dir, forward));
            var sDist   = MathF.Abs(Vector2.Dot(dir, side));
            var inVert  = fDist <= length && sDist <= halfWidth;
            var inHoriz = sDist <= length && fDist <= halfWidth;

            var     bestDist = float.MaxValue;
            Vector2 bestOff  = default;

            if (inVert)
            {
                Try(forward,  length    - fDist);
                Try(-forward, fDist     + length);
                Try(side,     halfWidth - sDist);
                Try(-side,    sDist     + halfWidth);
            }

            if (inHoriz)
            {
                Try(side,     length    - sDist);
                Try(-side,    sDist     + length);
                Try(forward,  halfWidth - fDist);
                Try(-forward, fDist     + halfWidth);
            }

            return point with { X = point.X + bestOff.X, Z = point.Z + bestOff.Y };

            void Try(Vector2 pushDir, float dist)
            {
                if (dist >= bestDist) return;
                bestDist = dist;
                bestOff  = pushDir * (dist + EXIT_MARGIN);
            }
        }
    }

    #endregion

    #region 常量
    
    private const float DEFAULT_CONE_ANGLE = 90f;

    #endregion
}
