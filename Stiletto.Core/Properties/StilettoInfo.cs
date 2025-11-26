using BepInEx;

namespace Stiletto
{
    public partial class Stiletto : BaseUnityPlugin
    {
        public const string GUID = "com.essu.stiletto";
        public const string Version = Constants.Version;
        public const int WindowId = 670;
#if DEBUG
        public const string PlugInName = "Stiletto (Debug)";
#else
        public const string PlugInName = "Stiletto";
#endif

    }
}
