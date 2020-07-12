using Stiletto.Configurations;

namespace Stiletto
{
    public static class HeelInfoContext
    {
        public static void RegisterHeelInfo(HeelInfo heelInfo)
        {
            HeelInfos.Add(heelInfo);
        }

        public static void UnregisterHeelInfo(HeelInfo heelInfo)
        {
            HeelInfos.Remove(heelInfo);
        }

        public static int Count => HeelInfos.Count;

        public static int LastIndex => HeelInfos.Count - 1;

        public static ConcurrentList<HeelInfo> HeelInfos { get; } = new ConcurrentList<HeelInfo>();
    }
}
