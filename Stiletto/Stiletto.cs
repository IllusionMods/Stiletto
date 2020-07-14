using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Stiletto.Configurations;
using Studio;
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using static ChaFileDefine;

namespace Stiletto
{
    [BepInPlugin(GUID, nameof(Stiletto), Version)]
    public class Stiletto : BaseUnityPlugin
    {
        public const string GUID = "com.essu.stiletto.custom";
        public const string Version = "1.3.0";
        public const int WindowId = 670;

        private StilettoGameGUI _gameWindow;
        private ConfigEntry<KeyboardShortcut> _showWindowKey;
        private bool _showWindow;

        private void Awake()
        {
            var directories = new DirectoryInfo[] {
                new DirectoryInfo(ConfigPaths.CONFIG_PATH),
                new DirectoryInfo(ConfigPaths.HEEL_PATH),
                new DirectoryInfo(ConfigPaths.FLAGS_PATH),
            };

            foreach (var directory in directories)
            {
                if (!directory.Exists) directory.Create();
            }

            Harmony.CreateAndPatchAll(GetType());
        }

        private void Start()
        {
            HeelFlagsProvider.ReloadHeelFlags();
            _showWindowKey = Config.Bind("Hotkeys", "Toggle stiletto window", new KeyboardShortcut(KeyCode.RightShift));

            StilettoMakerGUI.Start(this);
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
            if (_showWindowKey.Value.IsDown())
            {
                _showWindow = !_showWindow;

                if (_showWindow)
                {
                    if (_gameWindow == null)
                    {
                        _gameWindow = new StilettoGameGUI();
                    }

                    _gameWindow.Show = true;
                }
                else 
                {
                    _gameWindow.Show = false;
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(OCIChar), nameof(OCIChar.ActiveKinematicMode))]
        private void OCIChar_ActiveKinematicModeHook()
        {
            if (HeelInfoContext.Count == 0) return;

            var chaControls = HeelInfoContext.HeelInfos.Select(x => x.chaControl).ToArray();

            foreach (var chaControl in chaControls)
            {
                LoadHeelFile(chaControl);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetClothesState))]
        private void ChaControl_SetClothesStateHook(ChaControl __instance, ref int clothesKind)
        {
            var ck = (ClothesKind)clothesKind;

            if (ck == ClothesKind.shoes_inner || ck == ClothesKind.shoes_outer)
            {
                LoadHeelFile(__instance);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(YS_Assist), nameof(YS_Assist.SetActiveControl), new[] { typeof(GameObject), typeof(bool[]) })]
        private void YS_Assist_SetActiveControl(ref bool __result, ref GameObject obj, ref bool[] flags)
        {
            if (__result)
                if (obj && obj.activeSelf)
                    if (flags != null && flags.Length > 1 && flags[2])
                    {
                        var ind = -1;
                        if (obj.name == "ct_shoes_inner") ind = (int)ClothesKind.shoes_inner;
                        if (obj.name == "ct_shoes_outer") ind = (int)ClothesKind.shoes_outer;
                        if (ind != -1)
                        {
                            var obj_nonRef = obj;
                            var ccs = FindObjectsOfType<ChaControl>().Where(x => x.objClothes[ind] == obj_nonRef);
                            if (ccs.Any()) ChangeCustomClothesHook(ccs.First(), ref ind);
                        }
                    }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaFileStatus), nameof(ChaFileStatus.shoesType), MethodType.Setter)]
        private static void ChaFileStatus_set_shoesTypeHook(ChaFileStatus __instance)
        {
            var cc = FindObjectsOfType<ChaControl>().Where(x => x?.chaFile?.status == __instance).FirstOrDefault();
            if (cc == null) return;
            var ind = cc.fileStatus.shoesType == 0 ? 8 : 7;
            ChangeCustomClothesHook(cc, ref ind);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCustomClothes))]
        private static void ChangeCustomClothesHook(ChaControl __instance, ref int kind)
        {
            var ck = (ClothesKind)kind;
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
            if (ic == null || currentShoes > ic.Length) return;

            var shoeListInfo = ic[currentShoes];
            var fileName = shoeListInfo?.Name;
            if (fileName == null) return;

            var shoeConfig = HeelConfigProvider.LoadHeelFile(fileName);

            var heelInfo = __instance.gameObject.GetOrAddComponent<HeelInfo>();
            heelInfo.Setup(fileName, __instance, shoeConfig.Height, shoeConfig.AnkleAngle, shoeConfig.LegAngle);
        }
    }
}
