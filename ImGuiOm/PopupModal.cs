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
                ImGuiWindowFlags.NoTitleBar         |
                ImGuiWindowFlags.NoCollapse         |
                ImGuiWindowFlags.NoScrollbar        |
                ImGuiWindowFlags.NoScrollWithMouse  |
                ImGuiWindowFlags.NoMove             |
                ImGuiWindowFlags.NoResize           |
                ImGuiWindowFlags.NoSavedSettings    |
                ImGuiWindowFlags.NoBackground       |
                ImGuiWindowFlags.NoFocusOnAppearing |
                ImGuiWindowFlags.NoBringToFrontOnFocus
            ))
        {
            var drawList = ImGui.GetWindowDrawList();
            drawList.PushClipRect(viewport.Pos                - Vector2.One, viewport.Pos + viewport.Size + Vector2.One, false);
            drawList.AddRectFilled(viewport.Pos, viewport.Pos + viewport.Size, ImGui.GetColorU32(ImGuiCol.ModalWindowDimBg));
            drawList.PopClipRect();
        }

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

        return new ModalPopupDisposable(began);
    }

    public readonly ref struct ModalPopupDisposable : IDisposable
    {
        public readonly bool Success;

        internal ModalPopupDisposable
        (
            bool began
        ) => Success = began;

        public void Dispose()
        {
            if (Success)
                ImGui.End();
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
