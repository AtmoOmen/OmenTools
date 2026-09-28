using System.Numerics;
using Dalamud.Interface.ManagedFontAtlas;
using OmenTools.ImGuiOm.Markdown.Parsing;
using OmenTools.ImGuiOm.Markdown.Rendering;

namespace OmenTools.ImGuiOm.Markdown;

public sealed record MarkdownConfig
{
    // Sizes that FontManager builds into the atlas, so every heading resolves to a real font rather
    // than a scaled one. Discrete handles stay crisp; scaling a single handle blurs the glyphs.
    public static IReadOnlyList<float> DefaultHeadingScales { get; } = [1.6F, 1.4F, 1.2F, 1.0F, 1.0F, 0.9F];

    public float BaseFontScale { get; init; } = 1.0F;

    public Func<MarkdownFontRole, float>? FontScaleResolver { get; init; }

    public Action<string>? OnLinkClicked { get; init; }

    public Func<string, MarkdownImageResult?>? ImageResolver { get; init; }

    // Prepended to image and link sources that start with '/', so a document can reference site
    // assets by site-absolute path without repeating the host.
    public string? BaseURL { get; init; }

    public Action<string?, string>? CodeBlockRenderer { get; init; }

    public IReadOnlyList<float> HeadingScales { get; init; } = DefaultHeadingScales;

    public float? WrapWidth { get; init; }

    public float ListIndentPixels { get; init; } = 20.0F;

    public Vector4? LinkColor { get; init; }

    public IFontHandle ResolveFont
    (
        MarkdownFontRole role,
        float            headingScale = 1.0F
    ) =>
        FontManager.Instance().GetUIFont(FontScaleResolver?.Invoke(role) ?? BaseFontScale * headingScale);

    public float ResolveHeadingScale
    (
        int level
    )
    {
        var clamped = Math.Clamp(level, 1, 6);
        return clamped - 1 < HeadingScales.Count ?
                   HeadingScales[clamped - 1] :
                   1.0F;
    }

    // The gap between two blocks is measured in line heights so it scales with the font size and UI
    // scale, and so the value is not tied to a pixel count that only looks right at one size.
    // ImGuiMarkdown zeroes ItemSpacing for the whole render, so this distance is the whole gap.
    internal static float ResolveGap
    (
        MarkdownBlockKind previous,
        MarkdownBlockKind next
    ) => ImGui.GetTextLineHeight() * BlockSpacing.Gap(previous, next);

    public MarkdownImageResult? ResolveImage
    (
        string source
    )
    {
        if (ImageResolver is not null)
            return ImageResolver(source);

        var texture = ImageHelper.Instance().GetImage(ResolveURL(source));
        return texture is null ?
                   null :
                   new MarkdownImageResult(texture.Handle, texture.Size);
    }

    public string ResolveURL
    (
        string url
    )
    {
        if (BaseURL is not { Length: > 0 } || !url.StartsWith('/'))
            return url;

        return BaseURL.TrimEnd('/') + url;
    }
}
