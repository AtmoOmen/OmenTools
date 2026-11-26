using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;

namespace OmenTools.Extensions;

public static class CommonExtension
{
    /// <summary>
    ///     比较两个值；引用类型按对象身份比较，值类型按内存内容比较。
    /// </summary>
    /// <typeparam name="T">参与比较的数据类型。</typeparam>
    /// <param name="left">左侧值。</param>
    /// <param name="right">右侧值。</param>
    /// <returns>引用相同或值类型内存内容相同时返回真。</returns>
    public static bool AreEqual<T>(this T? left, T? right)
    {
        if (default(T) == null)
            return ReferenceEquals(left, right);

        if (left  == null ||
            right == null)
            return left  == null &&
                   right == null;

        var leftSpan  = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref left),  Unsafe.SizeOf<T>());
        var rightSpan = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref right), Unsafe.SizeOf<T>());
        return leftSpan.SequenceEqual(rightSpan);
    }

    extension<T>(T config) where T : class
    {
        public string ToJSONBase64() =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(config.ToJSON()));

        public string ToJSON() =>
            JsonConvert.SerializeObject(config, JsonSerializerSettings.GetShared());
    }

    extension(string textData)
    {
        public T? FromJSON<T>() where T : class =>
            string.IsNullOrEmpty(textData) ?
                null :
                JsonConvert.DeserializeObject<T>(textData, JsonSerializerSettings.GetShared());

        public T? FromJSONBase64<T>() where T : class =>
            string.IsNullOrEmpty(textData) ?
                null :
                Encoding.UTF8.GetString(Convert.FromBase64String(textData)).FromJSON<T>();
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
                    if (needle[k] != haystack[i + k])
                        break;

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
            haystack.TryFindBytes([.. needle.Split(" ").Select(x => byte.Parse(x, NumberStyles.HexNumber))], out pos);
    }
}
