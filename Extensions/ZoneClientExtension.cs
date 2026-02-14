using System.Reflection;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Application.Network;
using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace OmenTools.Extensions;

public static unsafe class ZoneClientExtension
{
    private static int ZoneClientOffset
    {
        get
        {
            if (field != 0) return field;

            return field = GetFieldOffset(nameof(ZoneClient)) ?? 0;
            
            static int? GetFieldOffset(string fieldName)
            {
                var fieldInfo = typeof(NetworkModule).GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (fieldInfo == null)
                    return null;

                var attribute = fieldInfo.GetCustomAttribute<FieldOffsetAttribute>();
                return attribute?.Value;
            }
        }
    }

    extension(ZoneClient client)
    {
        public static ZoneClient* Instance()
        {
            if (ZoneClientOffset == 0) return null;

            var instancePtr = (nint)Framework.Instance()->NetworkModuleProxy;
            return (ZoneClient*)(instancePtr + ZoneClientOffset);
            
            
        }
    }
}
