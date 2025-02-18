using System.Numerics;

namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    public static async Task WaitForCondition(Func<bool> condition, TimeSpan? timeout = null)
    {
        var tcs = new TaskCompletionSource<bool>();

        _ = Task.Run(async () =>
        {
            while (!condition())
            {
                await Task.Delay(100);
                if (tcs.Task.IsCompleted) return;
            }

            tcs.TrySetResult(true);
        });

        if (timeout.HasValue)
        {
            using var cts = new CancellationTokenSource(timeout.Value);
            cts.Token.Register(() => tcs.TrySetCanceled());
        }

        await tcs.Task;
    }
    
    public static float WorldDirHToCharaRotation(Vector2 direction) 
        => direction == Vector2.Zero ? 0f : MathF.Atan2(direction.X, direction.Y);

    public static float CharaRotationSymmetricTransform(float rotation) 
        => MathF.IEEERemainder(rotation + MathF.PI, 2 * MathF.PI);
    
    public static float CameraDirHToCharaRotation(float cameraDirH)
        => (cameraDirH - MathF.PI) % (2 * MathF.PI);

    public static DateTime UnixSecondToDateTime(long unixTimeStampS) 
        => DateTimeOffset.FromUnixTimeSeconds(unixTimeStampS).LocalDateTime;

    public static DateTime UnixMillisecondToDateTime(long unixTimeStampMS) 
        => DateTimeOffset.FromUnixTimeMilliseconds(unixTimeStampMS).LocalDateTime;
    
    public static void MoveItemToPosition<T>(List<T> list, Func<T, bool> sourceItemSelector, int targetedIndex)
    {
        var sourceIndex = -1;
        for (var i = 0; i < list.Count; i++)
            if (sourceItemSelector(list[i]))
            {
                sourceIndex = i;
                break;
            }

        if (sourceIndex == targetedIndex) return;
        var item = list[sourceIndex];
        list.RemoveAt(sourceIndex);
        list.Insert(targetedIndex, item);
    }
}
