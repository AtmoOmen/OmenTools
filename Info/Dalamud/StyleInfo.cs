using Dalamud.Interface.Style;

namespace OmenTools.Info.Dalamud;

public struct StyleInfo
{
    public string Name;
    public string Data;

    private readonly StyleModelV1 built;

    private readonly StyleModelWrapper wrapper;

    public StyleInfo(string name, string data)
    {
        Name    = name;
        Data    = data;
        built   = (StyleModelV1)StyleModel.Deserialize(data);
        wrapper = new(built);
    }

    public IDisposable Push()
    {
        built.Push();
        return wrapper;
    }

    private class StyleModelWrapper
    (
        StyleModelV1 styleModel
    ) : IDisposable
    {
        public void Dispose() =>
            styleModel.Pop();
    }
}
