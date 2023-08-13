using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

using HarmonyLib;

using Studio;
using System.Linq;
using UnityEngine;

using static ChaFileDefine;


namespace Stiletto
{
    [BepInPlugin(GUID, PlugInName, Version)]
    public partial class Stiletto : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger { get; private set; }

        private StilettoGameGUI _gameWindow;
        private StilettoMakerGUI _makerWindow;

        private ConfigEntry<KeyboardShortcut> _showWindowKey;
        private bool _showWindow;

        private void Awake()
        {
            Logger = base.Logger;
            Harmony.CreateAndPatchAll(GetType());
        }

        private void Start()
        {
            StilettoContext.Initalize();

            _makerWindow = new StilettoMakerGUI(this);
            _gameWindow = new StilettoGameGUI();
            _showWindowKey = Config.Bind("Hotkeys", "Toggle stiletto window", new KeyboardShortcut(KeyCode.RightShift));
        }

        private void OnGUI()
        {
            if (_gameWindow != null) 
            {
                _gameWindow?.DisplayWindow(WindowId);
            }
        }

        private void Update() 
        {
            if (_showWindowKey?.Value.IsDown() ?? false)
            {
                _showWindow = !_showWindow;
                _gameWindow.Show = _showWindow;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(OCIChar), nameof(OCIChar.ActiveKinematicMode))]
        private static void OCIChar_ActiveKinematicModeHook()
        {
            if (StilettoContext.Count == 0) return;

            var chaControls = StilettoContext.HeelInfos.Select(x => x.chaControl).ToArray();

            foreach (var chaControl in chaControls)
            {
                LoadHeelFile(chaControl);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetClothesState))]
        private static void ChaControl_SetClothesStateHook(ChaControl __instance, ref int clothesKind)
        {
            HookChaControlClothes(__instance, clothesKind);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCustomClothes))]
        private static void ChaControl_ChangeCustomClothesHook(ChaControl __instance, ref int kind)
        {
            HookChaControlClothes(__instance, kind);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaFileStatus), nameof(ChaFileStatus.shoesType), MethodType.Setter)]
        private static void ChaFileStatus_Set_ShoesTypeHook(ChaFileStatus __instance)
        {
            var chaControl = FindObjectsOfType<ChaControl>().Where(x => x?.chaFile?.status == __instance).FirstOrDefault();
            if (chaControl == null)
            {
                return;
            }

            if (chaControl.fileStatus.shoesType == 0)
            {
                HookChaControlClothes(chaControl, (int)ClothesKind.shoes_outer);
            }
            else 
            {
                HookChaControlClothes(chaControl, (int)ClothesKind.shoes_inner);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(YS_Assist), nameof(YS_Assist.SetActiveControl), new[] { typeof(GameObject), typeof(bool[]) })]
        private static void YS_Assist_SetActiveControl(ref bool __result, ref GameObject obj, ref bool[] flags)
        {
            if (!__result)
                return;

            if (!obj || !obj.activeSelf)
                return;

            if (flags != null && flags.Length > 1 && flags[2])
            {
                var clothesKind = -1;
                if (obj.name == "ct_shoes_inner")
                {
                    clothesKind = (int)ClothesKind.shoes_inner;
                }

                if (obj.name == "ct_shoes_outer")
                {
                    clothesKind = (int)ClothesKind.shoes_outer;
                }

                if (clothesKind != -1)
                {
                    var obj_nonRef = obj;
                    var ccs = FindObjectsOfType<ChaControl>().Where(x => x.objClothes[clothesKind] == obj_nonRef);

                    if (ccs.Any())
                    {
                        HookChaControlClothes(ccs.First(), clothesKind);
                    }
                }
            }
        }

        private static void HookChaControlClothes(ChaControl __instance, int clothesKind) 
        {
            var ck = (ClothesKind)clothesKind;

            if (ck == ClothesKind.shoes_inner || ck == ClothesKind.shoes_outer)
            {
                LoadHeelFile(__instance);
            }
        }

        private static void LoadHeelFile(ChaControl __instance)
        {
            if (__instance == null) return;

            var fs = __instance.fileStatus;
            if (fs == null) return;

            var currentShoes = (int)(fs.shoesType == 0 ? ClothesKind.shoes_inner : ClothesKind.shoes_outer);
            var ic = __instance.infoClothes;
            
            if (ic == null || __instance.infoClothes.ElementAtOrDefault(currentShoes) == null)
            {
                UnloadHeelFile(__instance);
                return;
            }

            var shoeListInfo = ic[currentShoes];
            var fileName = shoeListInfo?.Name;
            if (fileName == null)
            {
                UnloadHeelFile(__instance);
                return;
            }

            var heelInfo = __instance.gameObject.GetOrAddComponent<HeelInfo>();
            heelInfo.Setup(__instance, fileName);
        }

        private static void UnloadHeelFile(ChaControl __instance)
        {
            var heelInfo = __instance.gameObject.GetComponent<HeelInfo>();
            if (heelInfo != null)
            {
                heelInfo.Setup(__instance, null);
            }
        }
    }
}
