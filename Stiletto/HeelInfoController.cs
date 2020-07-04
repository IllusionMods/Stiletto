using KKAPI;
using KKAPI.Chara;
using KKAPI.Studio;
using RootMotion.FinalIK;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace Stiletto
{
    public class HeelInfoController : CharaCustomFunctionController
    {
        private HeelFlags flags = new HeelFlags();
        private bool active = false;

        private IKSolverFullBodyBiped solver;
        private Transform body;
        private Transform leg_L;
        private Transform footL;
        private Transform toesL;
        private Transform leg_R;
        private Transform footR;
        private Transform toesR;

        private Vector3 height;
        private Quaternion angleA;
        private Quaternion angleB;
        private Quaternion angleLeg;

        private static XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLContainer));
        private static XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
        private static XmlWriterSettings xmlWriterSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

        public string HeelName { get; private set; } = "-- NONE --";
        public int Id { get; private set; } = -1;

        public Vector3 Height => active && flags.ACTIVE && flags.HEIGHT ? height : Vector3.zero;
        public Quaternion AngleA => active && flags.ACTIVE && flags.ANKLE_ROLL ? angleA : Quaternion.identity;
        public Quaternion AngleB => active && flags.ACTIVE && flags.TOE_ROLL ? angleB : Quaternion.identity;
        public Quaternion AngleLeg => active && flags.ACTIVE ? angleLeg : Quaternion.identity;

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {

        }

        protected override void OnReload(GameMode currentGameMode)
        {
            LoadHeelFile();
        }

        internal void ClothesStateChangeEvent()
        {
            LoadHeelFile();
        }

        internal void ChangeCustomClothesEvent()
        {
            LoadHeelFile();
        }

        protected override void Update()
        {
            var currentShoes = (int)(ChaControl.fileStatus.shoesType == 0 ? ChaFileDefine.ClothesKind.shoes_inner : ChaFileDefine.ClothesKind.shoes_outer);
            active = ChaControl.fileStatus.clothesState[currentShoes] == 0;

            if(ChaControl.animBody == null)
                return;

            var aci = ChaControl.animBody.GetCurrentAnimatorClipInfo(0);
            if(aci.Length == 0)
                return;

            flags = Stiletto.FetchFlags($"{ChaControl.animBody.runtimeAnimatorController.name}/{aci[0].clip.name}");
        }

        public void Setup(string heelName, int id, float height, float angleAnkle, float angleLeg)
        {
            HeelName = heelName;
            Id = id;
            body = ChaControl.objBodyBone.transform.parent;
            UpdateValues(height, angleAnkle, angleLeg);

            var waist = body.Find("cf_j_root/cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02");
            if(waist == null)
                return;

            var legl3 = waist.Find("cf_j_thigh00_L/cf_j_leg01_L");
            leg_L = legl3.Find("cf_j_leg03_L");
            footL = leg_L.Find("cf_j_foot_L");
            toesL = footL.Find("cf_j_toes_L");

            var legr3 = waist.Find("cf_j_thigh00_R/cf_j_leg01_R");
            leg_R = legr3.Find("cf_j_leg03_R");
            footR = leg_R.Find("cf_j_foot_R");
            toesR = footR.Find("cf_j_toes_R");

            SetFBBIK(ChaControl.animBody.GetComponent<FullBodyBipedIK>());
        }

        public void UpdateValues(float height, float angleAnkle, float angleLeg)
        {
            this.height = new Vector3(0, height, 0);
            angleA = Quaternion.Euler(angleAnkle, 0f, 0f);
            angleB = Quaternion.Euler(-angleAnkle, 0f, 0f);
            this.angleLeg = Quaternion.Euler(angleLeg, 0f, 0f);
        }

        public void UpdateHeight(float value)
        {
            height = new Vector3(0, value, 0);
        }

        public void UpdateAnkleAngle(float value)
        {
            angleA = Quaternion.Euler(value, 0f, 0f);
            angleB = Quaternion.Euler(-value, 0f, 0f);
        }

        public void UpdateLegAngle(float value)
        {
            angleLeg = Quaternion.Euler(value, 0f, 0f);
        }

        internal void OnPreRead()
        {
            if(flags.KNEE_BEND && solver != null)
                solver.bodyEffector.positionOffset = -Height;
            else
                body.localPosition = Height;
        }

        internal void PostUpdate()
        {
            if(flags.KNEE_BEND && solver != null)
            {
                solver.rightFootEffector.target.position += Height;
                solver.leftFootEffector.target.position += Height;
                body.localPosition = Vector3.zero;
            }

            footL.localRotation *= AngleA;
            footR.localRotation *= AngleA;
            toesL.localRotation *= AngleB;
            toesR.localRotation *= AngleB;

            leg_L.localRotation *= AngleLeg;
            leg_R.localRotation *= AngleLeg;
        }

        internal void SetFBBIK(FullBodyBipedIK fbbik)
        {
            if(fbbik != null)
                solver = fbbik.solver;

            if(solver != null)
            {
                if(!StudioAPI.InsideStudio)
                {
                    var currentSceneName = fbbik.gameObject.scene.name;
                    if(!new[] { SceneNames.CustomScene, SceneNames.H, SceneNames.MyRoom }.Contains(currentSceneName))
                    {
                        //Disable arm weights, we only affect feet/knees.
                        fbbik.GetIKSolver().Initiate(fbbik.transform);
                        fbbik.solver.leftHandEffector.positionWeight = 0f;
                        fbbik.solver.rightHandEffector.positionWeight = 0f;
                        fbbik.solver.leftArmChain.bendConstraint.weight = 0f;
                        fbbik.solver.rightArmChain.bendConstraint.weight = 0f;
                        fbbik.solver.leftFootEffector.rotationWeight = 0f;
                        fbbik.solver.rightFootEffector.rotationWeight = 0f;
                    }

                    solver.IKPositionWeight = 1f;
                    fbbik.enabled = true;
                }

                solver.OnPreRead = OnPreRead;
                solver.OnPostUpdate = PostUpdate;
            }

            if(StudioAPI.InsideStudio)
            {
                Update();
                OnPreRead();
                PostUpdate();
            }
        }

        internal void SaveHeelFile()
        {
            var configFile = Path.Combine(Stiletto.CONFIG_PATH, $"{HeelName}.xml");
            var shoeConfig = new XMLContainer(Id, AngleA.eulerAngles.x, AngleLeg.eulerAngles.x, Height.y);

            using(var stream = new StreamWriter(configFile))
            using(var writer = XmlWriter.Create(stream, xmlWriterSettings))
                xmlSerializer.Serialize(writer, shoeConfig, xmlSerializerNamespaces);
        }

        internal void LoadHeelFile()
        {
            var currentShoes = (int)(ChaControl.fileStatus.shoesType == 0 ? ChaFileDefine.ClothesKind.shoes_inner : ChaFileDefine.ClothesKind.shoes_outer);
            var ic = ChaControl.infoClothes;
            if(ic == null || currentShoes > ic.Length)
                return;

            var shoeListInfo = ic[currentShoes];
            var fileName = shoeListInfo?.Name;
            if(fileName == null)
                return;

            float angleAnkle = 0f;
            float angleLeg = 0f;
            float height = 0f;
            int id = -1;

            var configFile = Path.Combine(Stiletto.CONFIG_PATH, $"{fileName}.xml");
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

            Setup(fileName, id, height, angleAnkle, angleLeg);
        }
    }
}
