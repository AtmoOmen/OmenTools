namespace OmenTools.Dalamud.Services.AetheryteList.Abstractions;

public interface IAetheryteList : IReadOnlyCollection<IAetheryteEntry>, IOmenDalamudService<IAetheryteList>
{
    int Length { get; }

    IAetheryteEntry? this[int index] { get; }
}
