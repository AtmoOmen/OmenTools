using System.Globalization;
using System.Text;
using Newtonsoft.Json;

namespace OmenTools.Extensions;

public static class CommonExtension
{
    extension<T>(T config) where T : class
    {
        public string ToJSONBase64() =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(config.ToJSON()));

        public string ToJSON() =>
            JsonConvert.SerializeObject(config, JSONSettings);
    }

    extension(string textData)
    {
        public T? FromJSON<T>() where T : class
        {
            if (string.IsNullOrEmpty(textData)) return null;

            return JsonConvert.DeserializeObject<T>(textData, JSONSettings);
        }

        public T? FromJSONBase64<T>() where T : class
        {
            if (string.IsNullOrEmpty(textData)) return null;

            return Encoding.UTF8.GetString(Convert.FromBase64String(textData)).FromJSON<T>();
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
                    if (needle[k] != haystack[i + k])
                        break;
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
