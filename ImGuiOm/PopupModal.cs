using System.Numerics;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static ModalPopupDisposable PopupModal
    (
        string           title,
        ref bool         open,
        ImGuiWindowFlags flags = ImGuiWindowFlags.None
    )
    {
        if (!open)
            return new(false);

        var viewport = ImGui.GetMainViewport();

        var blockerName = $"OmModalBlocker_{ImGui.GetID(title)}";
        ImGui.SetNextWindowPos(viewport.Pos);
        ImGui.SetNextWindowSize(viewport.Size);

        if (ImGui.Begin
            (
                blockerName,
                ImGuiWindowFlags.NoTitleBar            |
                ImGuiWindowFlags.NoCollapse            |
                ImGuiWindowFlags.NoScrollbar           |
                ImGuiWindowFlags.NoScrollWithMouse     |
                ImGuiWindowFlags.NoMove                |
                ImGuiWindowFlags.NoResize              |
                ImGuiWindowFlags.NoSavedSettings       |
                ImGuiWindowFlags.NoBackground          |
                ImGuiWindowFlags.NoFocusOnAppearing    |
                ImGuiWindowFlags.NoBringToFrontOnFocus |
                ImGuiWindowFlags.NoNavFocus            |
                ImGuiWindowFlags.NoNavInputs           |
                ImGuiWindowFlags.NoDocking
            ))
        {
            ImGui.SetCursorScreenPos(viewport.Pos);
            ImGui.InvisibleButton("##OmModalBlockerSurface", viewport.Size);

            var drawList = ImGui.GetWindowDrawList();
            drawList.PushClipRect(viewport.Pos                - Vector2.One, viewport.Pos + viewport.Size + Vector2.One, false);
            drawList.AddRectFilled(viewport.Pos, viewport.Pos + viewport.Size, ImGui.GetColorU32(ImGuiCol.ModalWindowDimBg));
            drawList.PopClipRect();
        }

        var blockerWindow = ImGuiP.GetCurrentWindow();
        ImGui.End();

        ImGui.SetNextWindowPos(viewport.GetCenter(), ImGuiCond.Appearing, new(0.5f));
        var began = ImGui.Begin
        (
            title,
            ref open,
            flags                            |
            ImGuiWindowFlags.NoCollapse      |
            ImGuiWindowFlags.NoSavedSettings |
            ImGuiWindowFlags.NoDocking
        );

        var modalWindow = ImGuiP.GetCurrentWindow();
        OrderModalWindows(blockerWindow, modalWindow);

        return new(began, true, blockerWindow, modalWindow);
    }

    private static void OrderModalWindows
    (
        ImGuiWindowPtr blockerWindow,
        ImGuiWindowPtr modalWindow
    )
    {
        ImGuiP.BringWindowToDisplayBehind(blockerWindow, modalWindow);

        var modalIndex = ImGuiP.FindWindowDisplayIndex(modalWindow);
        if (modalIndex < 0)
            return;

        ref var windows = ref ImGui.GetCurrentContext().Windows;

        for (var index = modalIndex + 1; index < windows.Size; index++)
        {
            var window = windows[index];
            if (ImGuiP.IsWindowWithinBeginStackOf(window, modalWindow))
                continue;

            ImGuiP.BringWindowToDisplayBehind(window, blockerWindow);
        }
    }

    public readonly ref struct ModalPopupDisposable : IDisposable
    {
        public readonly bool Success;

        private readonly bool           windowBegun;
        private readonly ImGuiWindowPtr blockerWindow;
        private readonly ImGuiWindowPtr modalWindow;

        internal ModalPopupDisposable
        (
            bool success
        )
        {
            Success       = success;
            windowBegun   = false;
            blockerWindow = ImGuiWindowPtr.Null;
            modalWindow   = ImGuiWindowPtr.Null;
        }

        internal ModalPopupDisposable
        (
            bool           success,
            bool           inWindowBegun,
            ImGuiWindowPtr inBlockerWindow,
            ImGuiWindowPtr inModalWindow
        )
        {
            Success       = success;
            windowBegun   = inWindowBegun;
            blockerWindow = inBlockerWindow;
            modalWindow   = inModalWindow;
        }

        public void Dispose()
        {
            if (!windowBegun)
                return;

            ImGui.End();
            OrderModalWindows(blockerWindow, modalWindow);
        }

        public static implicit operator bool
        (
            ModalPopupDisposable value
        ) => value.Success;

        public static bool operator true
        (
            ModalPopupDisposable value
        ) => value.Success;

        public static bool operator false
        (
            ModalPopupDisposable value
        ) => !value.Success;

        public static bool operator !
        (
            ModalPopupDisposable value
        ) => !value.Success;

        public static bool operator &
        (
            ModalPopupDisposable value,
            bool                 other
        ) => value.Success && other;

        public static bool operator |
        (
            ModalPopupDisposable value,
            bool                 other
        ) => value.Success || other;
    }
}
