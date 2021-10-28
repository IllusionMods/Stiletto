using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Studio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ChaFileDefine;

namespace Stiletto
{
    [BepInPlugin(GUID, nameof(Stiletto), Version)]
    public class Stiletto : BaseUnityPlugin
    {
        public const string Version = "1.5";

        internal static bool GUI_ACTIVE = false;
        internal static readonly bool InStudio = Paths.ProcessName == "CharaStudio";

        private const string CONFIG_PATH = "BepInEx/Stiletto";
        private const string FLAG_PATH = CONFIG_PATH + "/_flags.txt";
        private const string GUID = "com.essu.stiletto";
        private readonly List<string> defaultScene = new List<string> { "All" };

        private Rect windowRect = new Rect(200, 200, 300, 120);
        private readonly GUILayoutOption _width20 = GUILayout.Width(20);
        private readonly GUILayoutOption _width40 = GUILayout.Width(40);

        private static int GUID_HASH = GUID.GetHashCode();
        private static string heightBuffer = "";
        private static Harmony hi;

        private readonly ConfigEntry<KeyboardShortcut> toggleGuiKey;
        private static readonly Dictionary<string, HeelFlags> dictAnimFlags = new Dictionary<string, HeelFlags>();
        private static readonly ConcurrentList<HeelInfo> heelInfos = new ConcurrentList<HeelInfo>();
        private static readonly ConcurrentList<HeelInfo> currentSceneHeelInfos = new ConcurrentList<HeelInfo>();

        private static int HeelIndex
        {
            get => _heelIndex;
            set
            {
                if (_heelIndex != value)
                {
                    _heelIndex = value;
                    HeelIndexChanged();
                }
            }
        }

        private static int _heelIndex = -1;

        private static int SceneFilterIndex
        {
            get => _sceneFilterIndex;
            set
            {
                if (_sceneFilterIndex != value)
                {
                    _sceneFilterIndex = value;
                    HeelIndex = heelInfos.Count == 0 ? -1 : 0;
                }
            }
        }

        private static int _sceneFilterIndex = -1;

        internal Stiletto()
        {
            var di = new DirectoryInfo(CONFIG_PATH);
            if (!di.Exists) di.Create();

            ReloadConfig();

            toggleGuiKey = Config.Bind(new ConfigDefinition("Keyboard Shortcuts", "GUI Toggle"), new KeyboardShortcut(KeyCode.RightShift), new ConfigDescription("Toggles stiletto UI"));

            hi = new Harmony(nameof(Stiletto));
            hi.PatchAll(typeof(Stiletto));

            // For hot reload.
            int v = (int)ClothesKind.shoes_outer;
            foreach (var cc in GameObject.FindObjectsOfType<ChaControl>())
            {
                foreach (var comp in cc.GetComponents<Component>())
                    if (comp.GetType().Name == nameof(HeelInfo))
                        DestroyImmediate(comp);

                ChangeCustomClothesHook(cc, ref v);
            }
        }

        #region Hooks

        [HarmonyPostfix, HarmonyPatch(typeof(OCIChar), "ActiveKinematicMode")]
        public static void OCIChar_ActiveKinematicModeHook(OCIChar __instance)
        {
            if (HeelIndex == -1 || heelInfos.Count == 0) return;
            foreach (var cc in heelInfos.Select(x => x.ChaControl).ToArray()) LoadHeelFile(cc);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), "SetClothesState")]
        public static void ChaControl_SetClothesStateHook(ChaControl __instance, ref int clothesKind)
        {
            var ck = (ClothesKind)clothesKind;
            if (ck == ClothesKind.shoes_inner || ck == ClothesKind.shoes_outer)
                LoadHeelFile(__instance);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(YS_Assist), "SetActiveControl", new[] { typeof(GameObject), typeof(bool[]) })]
        public static void YS_Assist_SetActiveControl(ref bool __result, ref GameObject obj, ref bool[] flags)
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
                            var ccs = GameObject.FindObjectsOfType<ChaControl>().Where(x => x.objClothes[ind] == obj_nonRef);
                            if (ccs.Any()) ChangeCustomClothesHook(ccs.First(), ref ind);
                        }
                    }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaFileStatus), "set_shoesType")]
        public static void ChaFileStatus_set_shoesTypeHook(ChaFileStatus __instance)
        {
            var cc = FindObjectsOfType<ChaControl>().Where(x => x?.chaFile?.status == __instance).FirstOrDefault();
            if (cc == null) return;
            var ind = cc.fileStatus.shoesType == 0 ? 8 : 7;
            ChangeCustomClothesHook(cc, ref ind);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), "ChangeCustomClothes")]
        public static void ChangeCustomClothesHook(ChaControl __instance, ref int kind)
        {
            var ck = (ClothesKind)kind;
            if (ck == ClothesKind.shoes_inner || ck == ClothesKind.shoes_outer)
                LoadHeelFile(__instance);
        }

        #endregion Hooks

        #region Unity

        private void OnDestroy()
        {
            // For hot reload.
            foreach (var cc in GameObject.FindObjectsOfType<ChaControl>())
                GameObject.DestroyImmediate(cc.GetComponent<HeelInfo>());

            hi.UnpatchAll(nameof(Stiletto));
        }

        internal void Update()
        {
            if (toggleGuiKey.Value.IsDown())
                GUI_ACTIVE = !GUI_ACTIVE;
        }

        internal void OnGUI()
        {
            if (GUI_ACTIVE)
            {
                windowRect = GUILayout.Window(GUID_HASH, windowRect, Window, "Stiletto");
            }
        }

        #endregion Unity

        #region Save/Load

        internal static void SaveHeelFile(HeelInfo hi)
        {
            var configFile = $"{CONFIG_PATH}/{hi.HeelName}.txt";
            File.WriteAllText(configFile, $"angleAnkle={hi.AngleAnkleValue.eulerAngles.x}\r\nangleLeg={hi.AngleLegValue.eulerAngles.x}\r\nheight={hi.HeightValue.y}\r\naughStuff={(hi.LockAnkle ? 1 : 0)}\r\n");
        }

        internal static void LoadHeelFile(ChaControl __instance)
        {
            if (__instance == null) return;
            var fs = __instance.fileStatus;
            if (fs == null) return;

            var currentShoes = (int)(fs.shoesType == 0 ? ClothesKind.shoes_inner : ClothesKind.shoes_outer);
            var ic = __instance.infoClothes;
            if (ic == null || currentShoes > ic.Length) return;
            var fileName = ic[currentShoes]?.Name;
            if (fileName == null) return;

            var configFile = $"{CONFIG_PATH}/{fileName}.txt";

            float angleAnkle = 0f;
            float angleLeg = 0f;
            float height = 0f;
            bool aughStuff = false;

            if (!File.Exists(configFile))
            {
                File.WriteAllText(configFile, $"{nameof(angleAnkle)}=0\r\n{nameof(angleLeg)}=0\r\n{nameof(height)}=0\r\n");
            }

            var lines = File.ReadAllLines(configFile).Select(x => x.Split('='));
            foreach (var line in lines)
            {
                if (line.Length != 2) continue;
                switch (line[0].Trim())
                {
                    case nameof(angleAnkle):
                        float.TryParse(line[1], out angleAnkle);
                        break;

                    case nameof(angleLeg):
                        float.TryParse(line[1], out angleLeg);
                        break;

                    case nameof(height):
                        float.TryParse(line[1], out height);
                        break;

                    case nameof(aughStuff):
                        int.TryParse(line[1], out int augh);
                        aughStuff = augh == 1;
                        break;
                }
            }

            var heelInfo = __instance.gameObject.GetOrAddComponent<HeelInfo>();
            heelInfo.Setup(fileName, __instance, height, angleAnkle, angleLeg, aughStuff);
            heightBuffer = height.ToString("F3");
        }

        internal static void SaveHeelFlags()
        {
            File.WriteAllLines(FLAG_PATH, dictAnimFlags.Keys.OrderBy(x => x).Select(x => $"{x}={dictAnimFlags[x]}").ToArray());
        }

        internal void ReloadConfig()
        {
            if (File.Exists(FLAG_PATH))
            {
                foreach (var l in File.ReadAllLines(FLAG_PATH).Where(x => !x.StartsWith(";")))
                {
                    var args = l.Split('=');
                    if (args.Length == 2)
                    {
                        var name = args[0].Trim();
                        var flags = args[1].Trim();
                        dictAnimFlags[name] = HeelFlags.Parse(flags);
                    }
                }
            }
        }

        #endregion Save/Load

        #region Static Methods

        internal static void RegisterHeelInfo(HeelInfo hi)
        {
            heelInfos.Add(hi);
            if (HeelIndex == -1) HeelIndex = 0;
        }

        internal static void UnregisterHeelInfo(HeelInfo hi)
        {
            heelInfos.Remove(hi);
            if (heelInfos.Count == 0) HeelIndex = -1;
        }

        internal static HeelFlags FetchFlags(string name)
        {
            if (dictAnimFlags.TryGetValue(name, out HeelFlags hf)) return hf;
            hf = new HeelFlags();
            dictAnimFlags[name] = hf;
            SaveHeelFlags();
            return hf;
        }

        private static void HeelIndexChanged()
        {
            if (HeelIndex == -1 || heelInfos.Count == 0) heightBuffer = "0.000";
            else
            {
                var heelInfo = GetHeelInfoFromCurrentSceneIndex(HeelIndex);
                heightBuffer = heelInfo?.HeightValue.y.ToString("F3") ?? "0.000";
            }
        }

        private static HeelInfo GetHeelInfoFromCurrentSceneIndex(int index)
        {
            if (index >= 0 && index < currentSceneHeelInfos.Count)
            {
                ChaControl cc = currentSceneHeelInfos[index].ChaControl;
                return heelInfos.FirstOrDefault(hi => hi.ChaControl.gameObject == cc);
            }
            else
            {
                return heelInfos[index];
            }
        }

        private IEnumerable<string> RefreshSceneNames()
        {
            HashSet<string> uniqueScenes = new HashSet<string>();
            foreach (var heelInfo in heelInfos)
            {
                uniqueScenes.Add(heelInfo.gameObject.scene.name);
            }

            return uniqueScenes;
        }

        #endregion Static Methods

        #region GUI

        internal void Window(int id)
        {
            var sceneNames = InStudio ? defaultScene : RefreshSceneNames();

            currentSceneHeelInfos.Clear();
            if (sceneNames.Count() == 1)
            {
                sceneNames = defaultScene;
                foreach (var heelInfo in heelInfos)
                {
                    currentSceneHeelInfos.Add(heelInfo);
                }
            }
            else
            {
                foreach (var heelInfo in heelInfos)
                {
                    var characterScene = heelInfo.gameObject.scene.name;
                    string currentScene = sceneNames.ElementAt(SceneFilterIndex);
                    if (characterScene.Equals(currentScene))
                    {
                        currentSceneHeelInfos.Add(heelInfo);
                    }
                }
            }

            if (SceneFilterIndex == -1) SceneFilterIndex = 0;
            if (SceneFilterIndex >= sceneNames.Count()) SceneFilterIndex = sceneNames.Count() - 1;

            int count;
            if (HeelIndex == -1 || (count = currentSceneHeelInfos.Count) == 0)
            {
                GUILayout.Label("No characters detected.");
                GUI.DragWindow();
                return;
            }

            var selected = currentSceneHeelInfos[HeelIndex];
            GUILayout.BeginVertical();

            // Scene Filter
            GUILayout.BeginHorizontal();
            GUILayout.Label("Scene filter: ");
            GUI.enabled = SceneFilterIndex > 0;
            if (GUILayout.Button("<", _width20))
            {
                SceneFilterIndex = Math.Max(0, SceneFilterIndex - 1);
                GUI.changed = false;
            }

            GUI.enabled = true;
            var currentSceneName = currentSceneHeelInfos.Count > 0 ? sceneNames.ElementAt(SceneFilterIndex) : "-";
            GUILayout.Label(currentSceneName);

            GUI.enabled = SceneFilterIndex < sceneNames.Count() - 1;
            if (GUILayout.Button(">", _width20))
            {
                SceneFilterIndex = Math.Min(SceneFilterIndex + 1, sceneNames.Count() - 1);
                GUI.changed = false;
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            // Character selector
            GUILayout.BeginHorizontal();
            GUILayout.Label("Character: ");
            GUI.enabled = HeelIndex > 0;
            if (GUILayout.Button("<", _width20))
            {
                HeelIndex = Math.Max(0, HeelIndex - 1);
                GUI.changed = false;
            }
            GUI.enabled = true;

            GUILayout.Label(selected.ChaControl.fileParam.fullname);

            GUI.enabled = HeelIndex < count - 1;
            if (GUILayout.Button(">", _width20))
            {
                HeelIndex = Math.Min(HeelIndex + 1, count - 1);
                GUI.changed = false;
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Path:");
            GUILayout.Label(selected.PathName);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Anim:");
            GUILayout.Label(selected.AnimationName);
            GUILayout.EndHorizontal();

            selected.Flags.ACTIVE = GUILayout.Toggle(selected.Flags.ACTIVE, "Active");
            selected.Flags.HEIGHT = GUILayout.Toggle(selected.Flags.HEIGHT, "Adjust Height");
            selected.Flags.TOE_ROLL = GUILayout.Toggle(selected.Flags.TOE_ROLL, "Toe Roll");
            selected.Flags.ANKLE_ROLL = GUILayout.Toggle(selected.Flags.ANKLE_ROLL, "Ankle Roll");
            selected.Flags.KNEE_BEND = GUILayout.Toggle(selected.Flags.KNEE_BEND, "Knee Bend");

            if (GUILayout.Button("Save Flags"))
            {
                SaveHeelFlags();
                GUI.changed = false;
            }

            if (GUI.changed)
            {
                dictAnimFlags[selected.Key] = selected.Flags;
            }
            GUI.changed = false;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Shoes:");
            GUILayout.Label(selected.HeelName);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Ankle + Toes:");
            if (GUILayout.RepeatButton("-", _width20))
            {
                var newAngleAnkle = selected.AngleAnkleValue.eulerAngles.x - 0.1f;
                selected.UpdateValues(selected.HeightValue.y, newAngleAnkle, selected.AngleLegValue.eulerAngles.x, selected.LockAnkle);
                GUI.changed = false;
            }
            var angleA = GUILayout.TextField(selected.AngleAnkleValue.eulerAngles.x.ToString("F0"), _width40);
            if (GUILayout.RepeatButton("+", _width20))
            {
                var newAngleAnkle = selected.AngleAnkleValue.eulerAngles.x + 0.1f;
                selected.UpdateValues(selected.HeightValue.y, newAngleAnkle, selected.AngleLegValue.eulerAngles.x, selected.LockAnkle);
                GUI.changed = false;
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Lock ankle:");
            var b_aughStuff = GUILayout.Toggle(selected.LockAnkle, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Whole feet:");
            if (GUILayout.RepeatButton("-", _width20))
            {
                var newAngleLeg = selected.AngleLegValue.eulerAngles.x - 0.1f;
                selected.UpdateValues(selected.HeightValue.y, selected.AngleAnkleValue.eulerAngles.x, newAngleLeg, selected.LockAnkle);
                GUI.changed = false;
            }
            var angleLeg = GUILayout.TextField(selected.AngleLegValue.eulerAngles.x.ToString("F0"), _width40);
            if (GUILayout.RepeatButton("+", _width20))
            {
                var newAngleLeg = selected.AngleLegValue.eulerAngles.x + 0.1f;
                selected.UpdateValues(selected.HeightValue.y, selected.AngleAnkleValue.eulerAngles.x, newAngleLeg, selected.LockAnkle);
                GUI.changed = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Height:");
            if (GUILayout.RepeatButton("-", _width20))
            {
                var newHeight = selected.HeightValue.y - 0.001f;
                selected.UpdateValues(newHeight, selected.AngleAnkleValue.eulerAngles.x, selected.AngleLegValue.eulerAngles.x, selected.LockAnkle);
                heightBuffer = selected.HeightValue.y.ToString("F3");
                GUI.changed = false;
            }
            heightBuffer = GUILayout.TextField(heightBuffer, _width40);
            if (GUILayout.RepeatButton("+", _width20))
            {
                var newHeight = selected.HeightValue.y + 0.001f;
                selected.UpdateValues(newHeight, selected.AngleAnkleValue.eulerAngles.x, selected.AngleLegValue.eulerAngles.x, selected.LockAnkle);
                heightBuffer = selected.HeightValue.y.ToString("F3");
                GUI.changed = false;
            }
            GUILayout.EndHorizontal();

            if (GUI.changed)
            {
                if (angleA.Length == 0) angleA = "0";
                if (angleLeg.Length == 0) angleLeg = "0";
                if (heightBuffer.Length == 0) heightBuffer = "0";
                if (
                    float.TryParse(angleA, out float f_angleA) &&
                    float.TryParse(angleLeg, out float f_angleLeg) &&
                    float.TryParse(heightBuffer, out float f_height)
                )
                {
                    selected.UpdateValues(f_height, f_angleA, f_angleLeg, b_aughStuff);
                }
            }

            if (GUILayout.Button("Save Preset"))
            {
                SaveHeelFile(selected);
                foreach (var cc in heelInfos.Select(x => x.ChaControl)) LoadHeelFile(cc);
            }

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        #endregion GUI
    }
}