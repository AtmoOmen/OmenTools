using System.Runtime.CompilerServices;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.System.String;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public unsafe class GameResourceManager : OmenServiceBase
{
    public static GameResourceManagerConfig Config { get; private set; } = null!;
    
    private static readonly CompSig GetResourceSyncSig = new("E8 ?? ?? ?? ?? 48 8B 8E ?? ?? ?? ?? 49 89 04 0E");
    private delegate void* GetResourceSyncPrototype(
        nint  pFileManager, 
        uint* pCategoryID,
        char* pResourceType, 
        uint* pResourceHash,
        byte* pPath, 
        void* pUnknown);
    private static Hook<GetResourceSyncPrototype>? GetResourceSyncHook;

    private static readonly CompSig GetResourceAsyncSig = new("E8 ?? ?? ?? ?? 48 8B 5C 24 ?? 48 83 C4 68");
    private delegate void* GetResourceAsyncPrototype(
        nint  pFileManager,
        uint* pCategoryID,
        char* pResourceType,
        uint* pResourceHash,
        byte* pPath,
        void* pUnknown, 
        bool isUnknown);
    private static Hook<GetResourceAsyncPrototype>? GetResourceAsyncHook;

    private static readonly Crc32 HashGenerator = new();
    
    private static readonly Dictionary<Type, HashSet<string>> RegisteredPaths = [];
    private static HashSet<string> BlacklistedPaths = new(StringComparer.OrdinalIgnoreCase);

    internal override void Init()
    {
        Config = LoadConfig<GameResourceManagerConfig>() ?? new();
        
        GetResourceSyncHook  ??= GetResourceSyncSig.GetHook<GetResourceSyncPrototype>(GetResourceSyncDetour);
        GetResourceAsyncHook ??= GetResourceAsyncSig.GetHook<GetResourceAsyncPrototype>(GetResourceAsyncDetour);
        
        GameState.Login  += OnLogin;
        GameState.Logout += OnLogout;

        if (GameState.IsLoggedIn)
            Toggle(true);
    }

    private static void OnLogin() => Toggle(true);
    
    private static void OnLogout() => Toggle(false);
    
    public static void Toggle(bool isEnabled)
    {
        GetResourceSyncHook?.Toggle(isEnabled);
        GetResourceAsyncHook?.Toggle(isEnabled);
    }

    private static void* GetResourceSyncDetour(
        nint  pFileManager,
        uint* pCategoryID,
        char* pResourceType,
        uint* pResourceHash,
        byte* pPath,
        void* pUnknown) => 
        GetResourceHandler(true, pFileManager, pCategoryID, pResourceType, pResourceHash, pPath, pUnknown, false);

    private static void* GetResourceAsyncDetour(
        nint  pFileManager,
        uint* pCategoryID,
        char* pResourceType,
        uint* pResourceHash,
        byte* pPath,
        void* pUnknown,
        bool  isUnknown) =>
        GetResourceHandler(false, pFileManager, pCategoryID, pResourceType, pResourceHash, pPath, pUnknown, isUnknown);

    private static void* GetResourceHandler(
        bool  isSync,
        nint  pFileManager,
        uint* pCategoryID,
        char* pResourceType,
        uint* pResourceHash,
        byte* pPath,
        void* pUnknown,
        bool  isUnknown)
    {
        var gamePath = Utf8String.FromSequence(pPath);
        if (gamePath == null || gamePath->IsEmpty)
        {
            if (gamePath != null)
                gamePath->Dtor(true);
            return CallOriginalHandler(isSync, pFileManager, pCategoryID, pResourceType, pResourceHash, pPath, pUnknown, isUnknown);
        }

        var gamePathString = gamePath->ToString();
        gamePath->Dtor(true);
        if (Config.ShowGameResourceManagerLog &&
            (string.IsNullOrWhiteSpace(Config.GameResourceManagerKeyword) ||
             gamePathString.Contains(Config.GameResourceManagerKeyword)))
            Debug($"[Game Resource Manager]\n资源路径:{gamePathString}");

        var copy = BlacklistedPaths;
        if (copy.Contains(gamePathString))
        {
            var        path  = "vfx/path/nothing.avfx"u8;
            Span<byte> bPath = stackalloc byte[path.Length + 1];
            path.CopyTo(bPath);
            pPath = (byte*)Unsafe.AsPointer(ref bPath.GetPinnableReference());
            HashGenerator.Init();
            HashGenerator.Update(path);
            *pResourceHash = HashGenerator.Checksum;
        }

        return CallOriginalHandler(isSync, pFileManager, pCategoryID, pResourceType, pResourceHash, pPath, pUnknown, isUnknown);
    }

    private static void* CallOriginalHandler(
        bool  isSync,
        nint  pFileManager,
        uint* pCategoryID,
        char* pResourceType,
        uint* pResourceHash,
        byte* pPath,
        void* pUnknown,
        bool  isUnknown) =>
        isSync
            ? GetResourceSyncHook!.Original(pFileManager, pCategoryID, pResourceType, pResourceHash, pPath, pUnknown)
            : GetResourceAsyncHook!.Original(pFileManager, pCategoryID, pResourceType, pResourceHash, pPath, pUnknown, isUnknown);
    
    public static void AddToBlacklist(Type source, params string[] path)
    {
        RegisteredPaths.TryAdd(source, []);
        RegisteredPaths[source].AddRange(path);
        
        BlacklistedPaths = RegisteredPaths
                           .SelectMany(x => x.Value)
                           .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
    
    public static void AddToBlacklist(Type source, IEnumerable<string> paths)
    {
        RegisteredPaths.TryAdd(source, []);
        paths.ForEach(x => RegisteredPaths[source].Add(x));
        
        BlacklistedPaths = RegisteredPaths
                           .SelectMany(x => x.Value)
                           .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
    
    public static void RemoveFromBlacklist(Type source, params string[] paths)
    {
        RegisteredPaths.TryAdd(source, []);
        RegisteredPaths[source].RemoveRange(paths);

        BlacklistedPaths = RegisteredPaths
                           .SelectMany(x => x.Value)
                           .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
    
    public static void RemoveFromBlacklist(Type source, IEnumerable<string> paths)
    {
        RegisteredPaths.TryAdd(source, []);
        paths.ForEach(x => RegisteredPaths[source].Remove(x));

        BlacklistedPaths = RegisteredPaths
                           .SelectMany(x => x.Value)
                           .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    internal override void Uninit()
    {
        GameState.Login  -= OnLogin;
        GameState.Logout -= OnLogout;
        
        Toggle(false);
        
        GetResourceSyncHook?.Dispose();
        GetResourceSyncHook = null;
        
        GetResourceAsyncHook?.Dispose();
        GetResourceAsyncHook = null;
        
        BlacklistedPaths.Clear();
        RegisteredPaths.Clear();
    }
    
    public class GameResourceManagerConfig : OmenServiceConfiguration
    {
        public bool   ShowGameResourceManagerLog;
        public string GameResourceManagerKeyword = string.Empty;

        public void Save() => 
            this.Save(DService.GetOmenService<GameResourceManager>());
    }
    
    private class Crc32
    {
        private const uint POLY = 0xEDB88320;
        private static readonly uint[] CrcArray = Enumerable.Range(0, 256).Select(i =>
        {
            var k = (uint)i;
            for (var j = 0; j < 8; j++)
                k = (k & 1) != 0 ? (k >> 1) ^ POLY : k >> 1;
            return k;
        }).ToArray();

        private uint CRC32 = 0xFFFFFFFF;
        
        public uint Checksum => ~CRC32;

        public void Init() => CRC32 = 0xFFFFFFFF;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(ReadOnlySpan<byte> data)
        {
            foreach (var b in data)
                CRC32 = CrcArray[(CRC32 ^ b) & 0xFF] ^ ((CRC32 >> 8) & 0x00FFFFFF);
        }
    }
}
