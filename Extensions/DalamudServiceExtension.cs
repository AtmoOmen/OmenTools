namespace OmenTools.Extensions;

public static class DalamudServiceExtension
{
    extension<TService>(TService) where TService : IDalamudService
    {
        public static TService Instance() =>
            (TService)DService.Instance().PI.GetService(typeof(TService));
    }
}
