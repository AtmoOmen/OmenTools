using System.Numerics;

namespace OmenTools.ImGuiOm.Markdown;

public readonly record struct MarkdownImageResult
(
    ImTextureID TextureID,
    Vector2     Size
);
