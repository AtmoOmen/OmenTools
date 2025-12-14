using System.Globalization;
using Dalamud.Hooking;
using Timer = System.Timers.Timer;

namespace OmenTools.Helpers;

public static class CommonExtensions
{
    public static void Toggle<T>(this Hook<T>? hook, bool? isEnabled = null) where T : Delegate
    {
        if (hook == null || hook.IsDisposed) return;

        if (isEnabled == null)
        {
            if (hook.IsEnabled) 
                hook.Disable();
            else 
                hook.Enable();
        }
        else
        {
            if (isEnabled.Value) 
                hook.Enable();
            else 
                hook.Disable();
        }
    }

    public static void Restart(this Timer timer)
    {
        timer.Stop();
        timer.Start();
    }

    public static unsafe uint AsUInt32(this float f) => *(uint*)&f;

    public static unsafe float AsFloat(this uint u) => *(float*)&u;

    public static ref int ValidateRange(this ref int i, int min, int max)
    {
        if (i > max) 
            i = max;
        if (i < min) 
            i = min;
        
        return ref i;
    }

    public static ref float ValidateRange(this ref float i, float min, float max)
    {
        if (i > max) 
            i = max;
        if (i < min) 
            i = min;
        
        return ref i;
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
            TryFindBytes(haystack, needle.Split(" ").Select(x => byte.Parse(x, NumberStyles.HexNumber)).ToArray(), out pos);
    }
}
