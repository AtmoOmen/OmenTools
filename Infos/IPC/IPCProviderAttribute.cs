namespace OmenTools.Infos;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
public class IPCProviderAttribute(string ipcName) : Attribute
{
    public string  IPCName     { get; } = ipcName;
    public string? Description { get; set; }
} 
