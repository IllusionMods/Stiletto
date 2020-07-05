using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using KKAPI.Chara;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ChaFileDefine;

namespace Stiletto
{
    [BepInPlugin(GUID, nameof(Stiletto), Version)]
    public class Stiletto : BaseUnityPlugin
    {
        private const string GUID = "com.essu.stiletto";
        private const string Version = "1.4.1";

        internal static new ManualLogSource Logger;

        internal static string CONFIG_PATH = Path.Combine(Paths.ConfigPath, "Stiletto");
        private static string FLAG_PATH = Path.Combine(CONFIG_PATH, "_flags.txt");

        private static Dictionary<string, HeelFlags> dictAnimFlags = new Dictionary<string, HeelFlags>();

        private void Awake()
        {
            Logger = base.Logger;

            var dirInfo = new DirectoryInfo(CONFIG_PATH);
            if(!dirInfo.Exists) dirInfo.Create();

            ReloadConfig();

            Harmony.CreateAndPatchAll(GetType());
        }

        private void Start()
        {
            StilettoGui.Init(this);
            CharacterApi.RegisterExtraBehaviour<HeelInfoController>(GUID);
        }

        //[HarmonyPostfix, HarmonyPatch(typeof(OCIChar), nameof(OCIChar.ActiveKinematicMode))]
        //public static void OCIChar_ActiveKinematicModeHook()
        //{
        //    if(heelInfos.Count == 0)
        //        return;

        //    foreach(var cc in heelInfos.Select(x => x.ChaControl).ToArray())
        //        LoadHeelFile(cc);
        //}

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetClothesState))]
        public static void ChaControl_SetClothesStateHook(ChaControl __instance, ref int clothesKind)
        {
            __instance.GetComponent<HeelInfoController>().ClothesStateChangeEvent((ClothesKind)clothesKind);
        }

        //[HarmonyPostfix, HarmonyPatch(typeof(YS_Assist), nameof(YS_Assist.SetActiveControl), new[] { typeof(GameObject), typeof(bool[]) })]
        //public static void YS_Assist_SetActiveControl(ref bool __result, ref GameObject obj, ref bool[] flags)
        //{
        //    if(__result)
        //        if(obj && obj.activeSelf)
        //            if(flags != null && flags.Length > 1 && flags[2])
        //            {
        //                var ind = -1;
        //                if(obj.name == "ct_shoes_inner") ind = (int)ClothesKind.shoes_inner;
        //                if(obj.name == "ct_shoes_outer") ind = (int)ClothesKind.shoes_outer;
        //                if(ind != -1)
        //                {
        //                    var obj_nonRef = obj;
        //                    var ccs = FindObjectsOfType<ChaControl>().Where(x => x.objClothes[ind] == obj_nonRef);
        //                    if(ccs.Any()) ChangeCustomClothesHook(ccs.First(), ref ind);
        //                }
        //            }
        //}

        [HarmonyPostfix, HarmonyPatch(typeof(ChaFileStatus), nameof(ChaFileStatus.shoesType), MethodType.Setter)]
        public static void ChaFileStatus_set_shoesTypeHook(ChaFileStatus __instance)
        {
            var cc = FindObjectsOfType<ChaControl>().FirstOrDefault(x => x?.chaFile?.status == __instance);
            if(cc == null)
                return;

            var ind = cc.fileStatus.shoesType == 0 ? 8 : 7;
            cc.GetComponent<HeelInfoController>().ChangeCustomClothesEvent((ClothesKind)ind);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCustomClothes))]
        public static void ChangeCustomClothesHook(ChaControl __instance, ref int kind)
        {
            __instance.GetComponent<HeelInfoController>().ChangeCustomClothesEvent((ClothesKind)kind);
        }

        internal static HeelFlags FetchFlags(string name)
        {
            if(dictAnimFlags.TryGetValue(name, out HeelFlags heelFlags))
                return heelFlags;

            heelFlags = new HeelFlags();
            dictAnimFlags[name] = heelFlags;
            SaveHeelFlags();
            return heelFlags;
        }

        internal static void SaveHeelFlags()
        {
            File.WriteAllLines(FLAG_PATH, dictAnimFlags.Keys.OrderBy(x => x).Select(x => $"{x}={dictAnimFlags[x]}").ToArray());
        }

        internal void ReloadConfig()
        {
            if(File.Exists(FLAG_PATH))
            {
                foreach(var l in File.ReadAllLines(FLAG_PATH).Where(x => !x.StartsWith(";")))
                {
                    var args = l.Split('=');
                    if(args.Length == 2)
                    {
                        var name = args[0].Trim();
                        var flags = args[1].Trim();
                        dictAnimFlags[name] = HeelFlags.Parse(flags);
                    }
                }
            }
        }
    }
}
