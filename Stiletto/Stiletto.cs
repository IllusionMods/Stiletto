using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Studio;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using static ChaFileDefine;

namespace Stiletto
{
    [BepInPlugin(GUID, nameof(Stiletto), Version)]
    public class Stiletto : BaseUnityPlugin
    {
        public const string GUID = "com.essu.stiletto";
        public const string Version = "1.4.1";

        internal static new ManualLogSource Logger;

        private static readonly string CONFIG_PATH = Path.Combine(Paths.ConfigPath, "Stiletto");
        private static readonly string FLAG_PATH = Path.Combine(CONFIG_PATH, "_flags.txt");

        private static Dictionary<string, HeelFlags> dictAnimFlags = new Dictionary<string, HeelFlags>();
        private static ConcurrentList<HeelInfo> heelInfos = new ConcurrentList<HeelInfo>();

        private static XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLContainer));
        private static XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
        private static XmlWriterSettings xmlWriterSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

        private void Awake()
        {
            Logger = base.Logger;

            var di = new DirectoryInfo(CONFIG_PATH);
            if(!di.Exists) di.Create();

            ReloadConfig();

            Harmony.CreateAndPatchAll(GetType());
        }

        private void Start()
        {
            StilettoGui.Init(this);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(OCIChar), nameof(OCIChar.ActiveKinematicMode))]
        private static void OCIChar_ActiveKinematicModeHook()
        {
            if(heelInfos.Count == 0) return;
            foreach(var cc in heelInfos.Select(x => x.cc).ToArray()) LoadHeelFile(cc);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetClothesState))]
        private static void ChaControl_SetClothesStateHook(ChaControl __instance, ref int clothesKind)
        {
            var ck = (ClothesKind)clothesKind;
            if(ck == ClothesKind.shoes_inner || ck == ClothesKind.shoes_outer)
                LoadHeelFile(__instance);
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
                LoadHeelFile(__instance);
        }

        private static void SaveHeelFile(HeelInfo hi)
        {
            var configFile = Path.Combine(CONFIG_PATH, $"{hi.heelName}.xml");
            var shoeConfig = new XMLContainer(hi.id, hi.angleA.eulerAngles.x, hi.angleLeg.eulerAngles.x, hi.height.y);

            using(var stream = new StreamWriter(configFile))
            using(var writer = XmlWriter.Create(stream, xmlWriterSettings))
                xmlSerializer.Serialize(writer, shoeConfig, xmlSerializerNamespaces);
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

            float angleAnkle = 0f;
            float angleLeg = 0f;
            float height = 0f;
            int id = -1;

            var configFile = Path.Combine(CONFIG_PATH, $"{fileName}.xml");
            if(File.Exists(configFile))
            {
                using(var fileStream = new FileStream(configFile, FileMode.Open))
                {
                    var shoeConfig = ((XMLContainer)xmlSerializer.Deserialize(fileStream)).ShoeConfig.First();
                    angleAnkle = shoeConfig.AngleAnkle;
                    angleLeg = shoeConfig.AngleLeg;
                    height = shoeConfig.Height;
                }
            }
            else
            {
                var resolveInfo = Sideloader.AutoResolver.UniversalAutoResolver.TryGetResolutionInfo(ChaListDefine.CategoryNo.co_shoes, shoeListInfo.Id);
                if(resolveInfo != null)
                {
                    var stilettoXml = Sideloader.Sideloader.GetManifest(resolveInfo.GUID).manifestDocument.Root.Element("Stiletto");
                    id = resolveInfo.Slot;

                    if(stilettoXml != null)
                    {
                        using(var reader = new StringReader(stilettoXml.ToString()))
                        {
                            var xmlContainer = (XMLContainer)xmlSerializer.Deserialize(reader);
                            var shoeConfig = xmlContainer.ShoeConfig.First(x => x.Id == resolveInfo.Slot);
                            angleAnkle = shoeConfig.AngleAnkle;
                            angleLeg = shoeConfig.AngleLeg;
                            height = shoeConfig.Height;
                        }
                    }
                }
            }

            var heelInfo = __instance.gameObject.GetOrAddComponent<HeelInfo>();
            heelInfo.Setup(fileName, id, __instance, height, angleAnkle, angleLeg);
        }

        internal static HeelFlags FetchFlags(string name)
        {
            if(dictAnimFlags.TryGetValue(name, out HeelFlags hf)) return hf;
            hf = new HeelFlags();
            dictAnimFlags[name] = hf;
            SaveHeelFlags();
            return hf;
        }

        private static void SaveHeelFlags()
        {
            File.WriteAllLines(FLAG_PATH, dictAnimFlags.Keys.OrderBy(x => x).Select(x => $"{x}={dictAnimFlags[x]}").ToArray());
        }

        private void ReloadConfig()
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
