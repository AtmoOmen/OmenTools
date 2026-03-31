namespace OmenTools.OmenService;

public readonly record struct DrawScopesHandle(int ID)
{
    public bool IsValid => ID > 0;
}
