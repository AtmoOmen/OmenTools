using System.Reflection;
using OmenTools.Dalamud;
using OmenTools.Dalamud.Attributes;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService;

public class IPCManager : OmenServiceBase<IPCManager>
{
    private static readonly Type[] StaticIPCTypes =
        typeof(IPCManager).Assembly
                          .GetTypes()
                          .Where(HasIPCAttributeMember)
                          .ToArray();

    protected override void Init()
    {
        foreach (var type in StaticIPCTypes)
            IPCAttributeRegistry.RegStaticIPCs(type);
    }

    protected override void Uninit()
    {
        foreach (var type in StaticIPCTypes)
            IPCAttributeRegistry.UnregStaticIPCs(type);
    }

    private static bool HasIPCAttributeMember(Type type) =>
        type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .Any
            (member => member.IsDefined(typeof(IPCSubscriberAttribute), false) ||
                       member.IsDefined(typeof(IPCProviderAttribute),   false)
            );
}
