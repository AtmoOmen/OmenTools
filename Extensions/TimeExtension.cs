using System.Numerics;
using System.Runtime.CompilerServices;
using Timer = System.Timers.Timer;

namespace OmenTools.Extensions;

public static class TimeExtension
{
    extension(Timer timer)
    {
        public void Restart()
        {
            timer.Stop();
            timer.Start();
        }
    }

    extension<T>(T unixTimestamp) where T : IBinaryInteger<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime ToUTCDateTimeFromUnixSeconds()
        {
            var seconds = long.CreateTruncating(unixTimestamp);
            var ticks   = seconds * 10000000L + 621355968000000000L;

            return new DateTime(ticks, DateTimeKind.Utc);
        }
    }
}
