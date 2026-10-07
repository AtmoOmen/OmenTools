namespace OmenTools.Extensions;

public static class GCExtension
{
    extension
    (
        GC
    )
    {
        public static void FullCollect()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, false);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, false);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, false);
        }
    }
}
