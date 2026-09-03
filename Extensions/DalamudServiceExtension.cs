namespace OmenTools.Extensions;

public static class DalamudServiceExtension
{
    extension<TService>(TService) where TService : IDalamudService
    {
        public static TService Instance() =>
            (TService)IDalamudPluginInterface.Instance().GetService(typeof(TService));
    }
    
    extension(IDalamudPluginInterface)
    {
        public static IDalamudPluginInterface Instance() =>
            DService.Instance().PI;
    }
}

public static class DalamudLeftServiceExtension
{
    extension(IUiBuilder)
    {
        public static IUiBuilder Instance() =>
            DService.Instance().UIBuilder;
    }
}
