namespace OmenTools.Dalamud.Services.AetheryteList.Abstractions;

public interface IAetheryteList : IReadOnlyCollection<IAetheryteEntry>
{
    int Length { get; }

    IAetheryteEntry? this[int index] { get; }
}
