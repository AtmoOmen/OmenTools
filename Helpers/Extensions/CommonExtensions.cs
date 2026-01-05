using System.Globalization;
using Dalamud.Hooking;
using Timer = System.Timers.Timer;

namespace OmenTools.Helpers;

public static class CommonExtensions
{
    extension(Timer timer)
    {
        public void Restart()
        {
            timer.Stop();
            timer.Start();
        }
    }

    extension(byte[] haystack)
    {
        public bool TryFindBytes(byte[] needle, out int pos)
        {
            var len   = needle.Length;
            var limit = haystack.Length - len;
            for (var i = 0; i <= limit; i++)
            {
                var k = 0;
                for (; k < len; k++)
                {
                    if (needle[k] != haystack[i + k]) break;
                }

                if (k == len)
                {
                    pos = i;
                    return true;
                }
            }

            pos = 0;
            return false;
        }

        public bool TryFindBytes(string needle, out int pos) => 
            haystack.TryFindBytes(needle.Split(" ").Select(x => byte.Parse(x, NumberStyles.HexNumber)).ToArray(), out pos);
    }
}
