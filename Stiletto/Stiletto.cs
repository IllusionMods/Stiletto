using BepInEx;
using HarmonyLib;
using Stiletto.Configurations;
using Studio;
using System.IO;
using System.Linq;
using UnityEngine;
using static ChaFileDefine;

namespace Stiletto
{
    [BepInPlugin(GUID, nameof(Stiletto), Version)]
    public class Stiletto : BaseUnityPlugin
    {
        public const string GUID = "com.essu.stiletto.custom";
        public const string Version = "1.0.0";

        private static readonly string CONFIG_PATH = Path.Combine(Paths.ConfigPath, "Stiletto");
        private static readonly string FLAG_PATH = Path.Combine(CONFIG_PATH, "_flags.txt");

        private static ConcurrentList<HeelInfo> heelInfos = new ConcurrentList<HeelInfo>();

        public static Stiletto Instance;

        private void Awake()
        {
            var di = new DirectoryInfo(CONFIG_PATH);
            if(!di.Exists) di.Create();
            Harmony.CreateAndPatchAll(GetType());
        }

        private void Start()
        {
            Instance = this;
            StilettoMakerGUI.Start();
            HeelFlagsProvider.ReloadHeelFlags();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(OCIChar), nameof(OCIChar.ActiveKinematicMode))]
        private static void OCIChar_ActiveKinematicModeHook()
        {
            if(heelInfos.Count == 0) return;
            
            foreach(var chaControl in heelInfos.Select(x => x.chaControl).ToArray()) 
            { 
                LoadHeelFile(chaControl);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetClothesState))]
        private static void ChaControl_SetClothesStateHook(ChaControl __instance, ref int clothesKind)
        {
            var ck = (ClothesKind)clothesKind;
            if (ck == ClothesKind.shoes_inner || ck == ClothesKind.shoes_outer) { 
                LoadHeelFile(__instance);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(YS_Assist), nameof(YS_Assist.SetActiveControl), new[] { typeof(GameObject), typeof(bool[]) })]
        private static void YS_Assist_SetActiveControl(ref bool __result, ref GameObject obj, ref bool[] flags)
        {
            if(__result)
                if(obj && obj.activeSelf)
                    if(flags != null && flags.Length > 1 && flags[2])
                    {
                        var ind = -1;
                        if(obj.name == "ct_shoes_inner") ind = (int)ClothesKind.shoes_inner;
                        if(obj.name == "ct_shoes_outer") ind = (int)ClothesKind.shoes_outer;
                        if(ind != -1)
                        {
                            var obj_nonRef = obj;
                            var ccs = FindObjectsOfType<ChaControl>().Where(x => x.objClothes[ind] == obj_nonRef);
                            if(ccs.Any()) ChangeCustomClothesHook(ccs.First(), ref ind);
                        }
                    }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaFileStatus), nameof(ChaFileStatus.shoesType), MethodType.Setter)]
        private static void ChaFileStatus_set_shoesTypeHook(ChaFileStatus __instance)
        {
            var cc = FindObjectsOfType<ChaControl>().Where(x => x?.chaFile?.status == __instance).FirstOrDefault();
            if(cc == null) return;
            var ind = cc.fileStatus.shoesType == 0 ? 8 : 7;
            ChangeCustomClothesHook(cc, ref ind);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCustomClothes))]
        private static void ChangeCustomClothesHook(ChaControl __instance, ref int kind)
        {
            var ck = (ClothesKind)kind;
            if(ck == ClothesKind.shoes_inner || ck == ClothesKind.shoes_outer) 
            { 
                LoadHeelFile(__instance);
            }
        }

        private static void LoadHeelFile(ChaControl __instance)
        {
            if(__instance == null) return;

            var fs = __instance.fileStatus;
            if(fs == null) return;

            var currentShoes = (int)(fs.shoesType == 0 ? ClothesKind.shoes_inner : ClothesKind.shoes_outer);
            var ic = __instance.infoClothes;
            if(ic == null || currentShoes > ic.Length) return;

            var shoeListInfo = ic[currentShoes];
            var fileName = shoeListInfo?.Name;
            if(fileName == null) return;

            var shoeConfig = HeelConfigProvider.LoadHeelFile(fileName);

            var heelInfo = __instance.gameObject.GetOrAddComponent<HeelInfo>();
            heelInfo.Setup(fileName, __instance, shoeConfig.Height, shoeConfig.AngleAnkle, shoeConfig.AngleLeg);
        }


        internal static void RegisterHeelInfo(HeelInfo hi)
        {
            heelInfos.Add(hi);
        }

        internal static void UnregisterHeelInfo(HeelInfo hi)
        {
            heelInfos.Remove(hi);
        }
    }
}
