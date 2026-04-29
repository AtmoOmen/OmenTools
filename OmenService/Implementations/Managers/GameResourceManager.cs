using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.System.Resource;
using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;
using InteropGenerator.Runtime;
using OmenTools.Dalamud;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService;

public unsafe class GameResourceManager : OmenServiceBase<GameResourceManager>
{
    public GameResourceManagerConfig Config { get; private set; } = null!;

    public void AddToBlacklist(Type source, params string[] paths)
    {
        registeredPaths.AddOrUpdate
        (
            source,
            _ => ImmutableList.CreateRange(paths),
            (_, currentList) => currentList.AddRange(paths)
        );

        RegeneratePaths();
    }

    public void AddToBlacklist(Type source, IEnumerable<string> paths)
    {
        var items = paths as string[] ?? paths.ToArray();
        if (items.Length == 0) return;

        registeredPaths.AddOrUpdate
        (
            source,
            _ => ImmutableList.CreateRange(items),
            (_, currentList) => currentList.AddRange(items)
        );

        RegeneratePaths();
    }

    public void RemoveFromBlacklist(Type source, params string[] paths)
    {
        if (paths is not { Length: > 0 }) return;

        while (registeredPaths.TryGetValue(source, out var currentList))
        {
            var newList = currentList.RemoveRange(paths);

            if (newList == currentList)
                return;

            if (newList.IsEmpty)
            {
                var kvp = new KeyValuePair<Type, ImmutableList<string>>(source, currentList);
                if (((ICollection<KeyValuePair<Type, ImmutableList<string>>>)registeredPaths).Remove(kvp))
                    return;
            }
            else
            {
                if (registeredPaths.TryUpdate(source, newList, currentList))
                    return;
            }
        }

        RegeneratePaths();
    }

    public void RemoveFromBlacklist(Type source, IEnumerable<string> paths)
    {
        var items = paths as string[] ?? paths.ToArray();
        if (items.Length == 0) return;

        while (registeredPaths.TryGetValue(source, out var currentList))
        {
            var newList = currentList.RemoveRange(items);

            if (newList == currentList)
                return;

            if (newList.IsEmpty)
            {
                var kvp = new KeyValuePair<Type, ImmutableList<string>>(source, currentList);
                if (((ICollection<KeyValuePair<Type, ImmutableList<string>>>)registeredPaths).Remove(kvp))
                    break;
            }
            else
            {
                if (registeredPaths.TryUpdate(source, newList, currentList))
                    break;
            }
        }

        RegeneratePaths();
    }

    private Hook<ResourceManager.Delegates.GetResourceSync>?  GetResourceSyncHook;
    private Hook<ResourceManager.Delegates.GetResourceAsync>? GetResourceAsyncHook;

    private readonly CRC32 hashGenerator = new();

    private readonly ConcurrentDictionary<Type, ImmutableList<string>> registeredPaths  = [];
    private          ConcurrentDictionary<string, byte>                blacklistedPaths = new(StringComparer.OrdinalIgnoreCase);

    protected override void Init()
    {
        Config = LoadConfig<GameResourceManagerConfig>() ?? new();

        GetResourceSyncHook ??= DService.Instance().Hook.HookFromMemberFunction
        (
            typeof(ResourceManager.MemberFunctionPointers),
            "GetResourceSync",
            (ResourceManager.Delegates.GetResourceSync)GetResourceSyncDetour
        );
        GetResourceAsyncHook ??= DService.Instance().Hook.HookFromMemberFunction
        (
            typeof(ResourceManager.MemberFunctionPointers),
            "GetResourceAsync",
            (ResourceManager.Delegates.GetResourceAsync)GetResourceAsyncDetour
        );

        GameState.Instance().Login  += OnLogin;
        GameState.Instance().Logout += OnLogout;

        if (GameState.IsLoggedIn)
            Toggle(true);
    }

    protected override void Uninit()
    {
        GameState.Instance().Login  -= OnLogin;
        GameState.Instance().Logout -= OnLogout;

        Toggle(false);

        GetResourceSyncHook?.Dispose();
        GetResourceSyncHook = null;

        GetResourceAsyncHook?.Dispose();
        GetResourceAsyncHook = null;

        blacklistedPaths.Clear();
        registeredPaths.Clear();
    }

    private void OnLogin() =>
        Toggle(true);

    private void OnLogout() =>
        Toggle(false);

    private ResourceHandle* GetResourceSyncDetour
    (
        ResourceManager*  manager,
        ResourceCategory* category,
        uint*             type,
        uint*             hash,
        CStringPointer    path,
        void*             unknown,
        void*             unkDebugPtr,
        uint              unkDebugInt
    ) =>
        GetResourceHandler(true, manager, category, type, hash, path, unknown, false, unkDebugPtr, unkDebugInt);

    private ResourceHandle* GetResourceAsyncDetour
    (
        ResourceManager*  manager,
        ResourceCategory* category,
        uint*             type,
        uint*             hash,
        CStringPointer    path,
        void*             unknown,
        bool              isUnknown,
        void*             unkDebugPtr,
        uint              unkDebugInt
    ) =>
        GetResourceHandler(false, manager, category, type, hash, path, unknown, isUnknown, unkDebugPtr, unkDebugInt);

    private ResourceHandle* GetResourceHandler
    (
        bool              isSync,
        ResourceManager*  manager,
        ResourceCategory* category,
        uint*             type,
        uint*             hash,
        CStringPointer    path,
        void*             unknown,
        bool              isUnknown,
        void*             unkDebugPtr,
        uint              unkDebugInt
    )
    {
        if (!path.HasValue)
            return InvokeOriginal();

        var gamePathString = path.ToString().Trim();
        if (string.IsNullOrEmpty(gamePathString))
            return InvokeOriginal();

        if (Config.ShowGameResourceManagerLog &&
            (string.IsNullOrWhiteSpace(Config.GameResourceManagerKeyword) ||
             gamePathString.Contains(Config.GameResourceManagerKeyword)))
            DLog.Debug($"[Game Resource Manager]\n资源路径:{gamePathString}");

        gamePathString = gamePathString.ToLowerInvariant();

        if (blacklistedPaths.ContainsKey(gamePathString))
        {
            var        nothingPath = "vfx/path/nothing.avfx"u8;
            Span<byte> bPath       = stackalloc byte[nothingPath.Length + 1];
            nothingPath.CopyTo(bPath);
            path = (byte*)Unsafe.AsPointer(ref bPath.GetPinnableReference());
            hashGenerator.Init();
            hashGenerator.Update(nothingPath);
            *hash = hashGenerator.Checksum;
        }

        return InvokeOriginal();

        ResourceHandle* InvokeOriginal()
        {
            return isSync
                       ? GetResourceSyncHook.Original(manager, category, type, hash, path, unknown, unkDebugPtr, unkDebugInt)
                       : GetResourceAsyncHook.Original(manager, category, type, hash, path, unknown, isUnknown, unkDebugPtr, unkDebugInt);
        }
    }

    private void Toggle(bool isEnabled)
    {
        GetResourceSyncHook?.Toggle(isEnabled);
        GetResourceAsyncHook?.Toggle(isEnabled);
    }

    private void RegeneratePaths() =>
        blacklistedPaths = registeredPaths
                           .SelectMany(x => x.Value)
                           .Select(x => x.Trim().ToLowerInvariant())
                           .ToConcurrentDictionary(x => x, _ => byte.MinValue);


    public class GameResourceManagerConfig : OmenServiceConfig
    {
        public bool   ShowGameResourceManagerLog;
        public string GameResourceManagerKeyword = string.Empty;

        public void Save() =>
            this.Save(Instance());
    }

    private class CRC32
    {
        public uint Checksum => ~crc32;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init() =>
            crc32 = 0xFFFFFFFFu;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Update(ReadOnlySpan<byte> data)
        {
            var crc = crc32;


            if (data.Length >= 8)
            {
                ref var src = ref MemoryMarshal.GetReference(data);
                nint    i   = 0;
                nint    len = data.Length;


                const int T0 = 0 * 256;
                const int T1 = 1 * 256;
                const int T2 = 2 * 256;
                const int T3 = 3 * 256;
                const int T4 = 4 * 256;
                const int T5 = 5 * 256;
                const int T6 = 6 * 256;
                const int T7 = 7 * 256;


                var limit = len - 8;

                while (i <= limit)
                {

                    var block = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref src, i));


                    if (!BitConverter.IsLittleEndian)
                        block = BinaryPrimitives.ReverseEndianness(block);


                    var low  = (uint)block ^ crc;
                    var high = (uint)(block >> 32);


                    crc =
                        Table[T7 + (byte)(high >> 24)] ^
                        Table[T6 + (byte)(high >> 16)] ^
                        Table[T5 + (byte)(high >> 8)]  ^
                        Table[T4 + (byte)high]         ^
                        Table[T3 + (byte)(low >> 24)]  ^
                        Table[T2 + (byte)(low >> 16)]  ^
                        Table[T1 + (byte)(low >> 8)]   ^
                        Table[T0 + (byte)low];

                    i += 8;
                }


                for (; i < len; i++)
                {
                    var b = Unsafe.Add(ref src, i);
                    crc = Table[(crc ^ b) & 0xFF] ^ crc >> 8;
                }

                crc32 = crc;
                return;
            }


            foreach (var b in data)
                crc = Table[(crc ^ b) & 0xFF] ^ crc >> 8;

            crc32 = crc;
        }


        private const uint POLY = 0xEDB88320u;

        private static readonly uint[] Table = CreateSlicingBy8Tables();

        private uint crc32 = 0xFFFFFFFF;

        private static uint[] CreateSlicingBy8Tables()
        {
            var table = new uint[8 * 256];

            for (uint i = 0; i < 256; i++)
            {
                var c = i;
                for (var j = 0; j < 8; j++)
                    c = (c & 1) != 0 ? c >> 1 ^ POLY : c >> 1;
                table[i] = c;
            }

            for (var t = 1; t < 8; t++)
            {
                var prev = (t - 1) * 256;
                var cur  = t       * 256;

                for (var i = 0; i < 256; i++)
                {
                    var c = table[prev + i];
                    table[cur + i] = table[c & 0xFF] ^ c >> 8;
                }
            }

            return table;
        }
    }
}
