using BepInEx;

using System.Reflection;


#region Assembly information
// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Stiletto")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Stiletto")]
[assembly: AssemblyCopyright("Copyright ©  2020-2023")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]


// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(Stiletto.Stiletto.Version)]
[assembly: AssemblyFileVersion(Stiletto.Stiletto.Version)]
#endregion

namespace Stiletto
{
    public partial class Stiletto : BaseUnityPlugin
    {
        public const string GUID = "com.essu.stiletto";
        public const string Version = "2.4.1";
        public const int WindowId = 670;
#if DEBUG
        public const string PlugInName = "Stiletto (Debug)";
#else
        public const string PlugInName = "Stiletto";
#endif

    }
}
