using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Hooking;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using FFXIVClientStructs.FFXIV.Client.UI;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService.KeyboardShortcut;

public sealed unsafe partial class KeyboardShortcutManager : OmenServiceBase<KeyboardShortcutManager>
{
    private Hook<KeyboardDevice.Delegates.ProcessKeyboardInputMessage>? ProcessKeyboardInputMessageHook;

    private readonly Dictionary<int, int> suppressedKeys = [];

    private ImmutableArray<ShortcutScope> scopes = [];

    private int nextScopeID;

    protected override void Init()
    {
        ProcessKeyboardInputMessageHook ??= IGameInteropProvider.Instance().HookFromMemberFunction
        (
            typeof(KeyboardDevice.MemberFunctionPointers),
            nameof(KeyboardDevice.ProcessKeyboardInputMessage),
            (KeyboardDevice.Delegates.ProcessKeyboardInputMessage)ProcessKeyboardInputMessageDetour
        );
        ProcessKeyboardInputMessageHook.Enable();
    }

    protected override void Uninit()
    {
        ProcessKeyboardInputMessageHook?.Dispose();
        ProcessKeyboardInputMessageHook = null;

        scopes = [];
        suppressedKeys.Clear();
    }

    /// <summary>
    ///     为指定快捷键拦截作用域续期或创建新作用域，仅在当前帧与下一帧内保持生效。
    /// </summary>
    /// <param name="scopeID">作用域标识引用；首次调用时自动分配。</param>
    /// <param name="matcher">判断快捷键是否需要被拦截并消费的匹配委托。</param>
    public void KeepAlive
    (
        ref int                           scopeID,
        Func<KeyboardShortcutEvent, bool> matcher
    )
    {
        ArgumentNullException.ThrowIfNull(matcher);

        var currentFrame = ImGui.GetFrameCount();

        if (scopeID > 0 &&
            TryGetScope(scopeID, out var existingScope))
        {
            Volatile.Write(ref existingScope.Matcher,     matcher);
            Volatile.Write(ref existingScope.ActiveFrame, currentFrame);
            return;
        }

        var id    = Interlocked.Increment(ref nextScopeID);
        var scope = new ShortcutScope(id, matcher, currentFrame);

        ImmutableInterlocked.Update
        (
            ref scopes,
            static (current, item) => current.Add(item),
            scope
        );

        scopeID = id;
    }

    /// <summary>
    ///     立即释放指定的快捷键拦截作用域，将按键归还给游戏。
    /// </summary>
    /// <param name="scopeID">作用域标识引用；释放后重置为 0。</param>
    public void Release(ref int scopeID)
    {
        var id = scopeID;
        scopeID = 0;

        if (id <= 0)
            return;

        ImmutableInterlocked.Update
        (
            ref scopes,
            static (current, targetID) => current.RemoveAll(scope => scope.ID == targetID),
            id
        );
    }

    /// <summary>
    ///     取出并清空指定作用域自上次消费以来拦截到的所有快捷键事件。
    /// </summary>
    /// <param name="scopeID">作用域标识。</param>
    /// <param name="destination">接收事件的目标集合。</param>
    public void Drain
    (
        int                         scopeID,
        List<KeyboardShortcutEvent> destination
    )
    {
        ArgumentNullException.ThrowIfNull(destination);

        if (scopeID <= 0 ||
            !TryGetScope(scopeID, out var scope))
            return;

        while (scope.Events.TryDequeue(out var shortcutEvent))
            destination.Add(shortcutEvent);
    }

    private bool TryGetScope
    (
        int               scopeID,
        out ShortcutScope scope
    )
    {
        var snapshot = scopes;

        for (var i = snapshot.Length - 1; i >= 0; i--)
        {
            if (snapshot[i].ID == scopeID)
            {
                scope = snapshot[i];
                return true;
            }
        }

        scope = null!;
        return false;
    }

    private void PruneExpiredScopes(int currentFrame) =>
        ImmutableInterlocked.Update
        (
            ref scopes,
            static (current, frame) => current.RemoveAll(scope => frame - Volatile.Read(ref scope.ActiveFrame) > SCOPE_FRAME_TTL),
            currentFrame
        );

    private void ProcessKeyboardInputMessageDetour
    (
        nint hWnd,
        uint uMsg,
        nint wParam,
        nint lParam
    )
    {
        var snapshot = scopes;

        if (snapshot.IsEmpty &&
            suppressedKeys.Count == 0)
        {
            ProcessKeyboardInputMessageHook.Original(hWnd, uMsg, wParam, lParam);
            return;
        }

        switch (uMsg)
        {
            case WM_ACTIVATE when (wParam & 0xFFFF) == 0:
            case WM_ENTERMENULOOP:
            case WM_ENTERSIZEMOVE:
                suppressedKeys.Clear();
                break;

            case WM_CHAR:
            case WM_SYSCHAR:
            {
                var charScanCode = (int)((lParam >> 16) & 0xFF);
                if (suppressedKeys.Count > 0 &&
                    suppressedKeys.ContainsValue(charScanCode))
                    return;

                break;
            }

            case WM_KEYUP:
            case WM_SYSKEYUP:
            {
                var upScanCode = (int)((lParam >> 16) & 0xFF);
                var releasedVK = ResolveVirtualKey(wParam, upScanCode);
                if (suppressedKeys.Remove(releasedVK))
                    return;

                break;
            }

            case WM_KEYDOWN:
            case WM_SYSKEYDOWN:
            {
                var scanCode          = (int)((lParam >> 16) & 0xFF);
                var vk                = ResolveVirtualKey(wParam, scanCode);
                var alreadySuppressed = suppressedKeys.ContainsKey(vk);

                if (!snapshot.IsEmpty                 &&
                    !ImGui.GetCurrentContext().IsNull &&
                    !IsGameTextInputActive())
                {
                    var key = ImGuiHelpers.VirtualKeyToImGuiKey((VirtualKey)vk);

                    if (key != ImGuiKey.None)
                    {
                        var isRepeat     = (lParam & KEY_REPEAT_FLAG) != 0;
                        var modifiers    = GetCurrentModifiers();
                        var shortcut     = new KeyboardShortcutEvent(key, modifiers, isRepeat);
                        var currentFrame = ImGui.GetFrameCount();
                        var hasExpired   = false;
                        var handled      = false;

                        for (var i = snapshot.Length - 1; i >= 0; i--)
                        {
                            var scope = snapshot[i];

                            if (currentFrame - Volatile.Read(ref scope.ActiveFrame) > SCOPE_FRAME_TTL)
                            {
                                hasExpired = true;
                                continue;
                            }

                            var matcher = Volatile.Read(ref scope.Matcher);
                            if (!matcher(shortcut))
                                continue;

                            scope.Events.Enqueue(shortcut);
                            suppressedKeys[vk] = scanCode;
                            handled            = true;
                            break;
                        }

                        if (hasExpired)
                            PruneExpiredScopes(currentFrame);

                        if (handled)
                            return;
                    }
                }

                if (alreadySuppressed)
                    return;

                break;
            }
        }

        ProcessKeyboardInputMessageHook.Original(hWnd, uMsg, wParam, lParam);
    }

    private static int ResolveVirtualKey
    (
        nint wParam,
        int  scanCode
    )
    {
        var vk = (int)wParam;

        if (vk       == VK_PROCESSKEY &&
            scanCode != 0)
        {
            var mapped = (int)MapVirtualKeyW((uint)scanCode, MAPVK_VSC_TO_VK);
            if (mapped != 0)
                return mapped;
        }

        return vk;
    }

    private static KeyboardShortcutModifiers GetCurrentModifiers()
    {
        var modifiers = KeyboardShortcutModifiers.None;

        if ((GetKeyState(VK_CONTROL) & KEY_DOWN_MASK) != 0)
            modifiers |= KeyboardShortcutModifiers.Ctrl;

        if ((GetKeyState(VK_SHIFT) & KEY_DOWN_MASK) != 0)
            modifiers |= KeyboardShortcutModifiers.Shift;

        if ((GetKeyState(VK_MENU) & KEY_DOWN_MASK) != 0)
            modifiers |= KeyboardShortcutModifiers.Alt;

        return modifiers;
    }

    private static bool IsGameTextInputActive()
    {
        var atkModule = RaptureAtkModule.Instance();
        return atkModule != null &&
               atkModule->AtkModule.IsTextInputActive();
    }

    [LibraryImport(USER32)]
    private static partial short GetKeyState(int nVirtKey);

    [LibraryImport(USER32)]
    private static partial uint MapVirtualKeyW
    (
        uint uCode,
        uint uMapType
    );

    private sealed class ShortcutScope
    (
        int                               id,
        Func<KeyboardShortcutEvent, bool> matcher,
        int                               activeFrame
    )
    {
        public int ID { get; } = id;

        public ConcurrentQueue<KeyboardShortcutEvent> Events { get; } = new();

        public Func<KeyboardShortcutEvent, bool> Matcher = matcher;

        public int ActiveFrame = activeFrame;
    }

    #region 常量

    private const string USER32 = "user32.dll";

    private const int SCOPE_FRAME_TTL = 1;

    private const uint WM_ACTIVATE      = 0x0006;
    private const uint WM_KEYDOWN       = 0x0100;
    private const uint WM_KEYUP         = 0x0101;
    private const uint WM_CHAR          = 0x0102;
    private const uint WM_SYSKEYDOWN    = 0x0104;
    private const uint WM_SYSKEYUP      = 0x0105;
    private const uint WM_SYSCHAR       = 0x0106;
    private const uint WM_ENTERMENULOOP = 0x0211;
    private const uint WM_ENTERSIZEMOVE = 0x0231;

    private const int  VK_SHIFT        = 0x10;
    private const int  VK_CONTROL      = 0x11;
    private const int  VK_MENU         = 0x12;
    private const int  VK_PROCESSKEY   = 0xE5;
    private const uint MAPVK_VSC_TO_VK = 1;

    private const int  KEY_DOWN_MASK   = 0x8000;
    private const nint KEY_REPEAT_FLAG = 0x40000000;

    #endregion
}
