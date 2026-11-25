namespace OmenTools.OmenService.KeyboardShortcut;

public readonly record struct KeyboardShortcutEvent
(
    ImGuiKey                  Key,
    KeyboardShortcutModifiers Modifiers,
    bool                      IsRepeat
);
