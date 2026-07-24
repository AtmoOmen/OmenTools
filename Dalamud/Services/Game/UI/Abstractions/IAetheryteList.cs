namespace OmenTools.Dalamud.Services.Game.UI.Abstractions;

public interface IAetheryteList : IReadOnlyCollection<IAetheryteEntry>, IOmenDalamudService<IAetheryteList>
{
    int Length { get; }

    IAetheryteEntry? this[int index] { get; }
}
