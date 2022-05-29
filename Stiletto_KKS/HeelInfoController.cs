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

        private XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLContainer));
        private XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
        private XmlWriterSettings xmlWriterSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

        private Vector3 HeightInternal => active && flags.ACTIVE && flags.HEIGHT ? height : Vector3.zero;
        private Quaternion AngleAInternal => active && flags.ACTIVE && flags.ANKLE_ROLL ? angleA : Quaternion.identity;
        private Quaternion AngleBInternal => active && flags.ACTIVE && flags.TOE_ROLL ? angleB : Quaternion.identity;
        private Quaternion AngleLegInternal => active && flags.ACTIVE ? angleLeg : Quaternion.identity;

        public string HeelName { get; private set; } = "-- NONE --";
        public int Id { get; private set; } = -1;

        public float AnkleAngle
        {
            get => AngleAInternal.eulerAngles.x;
            set
            {
                angleA = Quaternion.Euler(value, 0f, 0f);
                angleB = Quaternion.Euler(-value, 0f, 0f);
            }
        }

        public float LegAngle
        {
            get => angleLeg.eulerAngles.x;
            set => angleLeg = Quaternion.Euler(value, 0f, 0f);
        }

        public float Height
        {
            get => HeightInternal.y;
            set => height = new Vector3(0, value, 0);
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
        }

        protected override void OnReload(GameMode currentGameMode)
        {
            var currentShoes = (int)(ChaControl.fileStatus.shoesType == 0 ? ChaFileDefine.ClothesKind.shoes_inner : ChaFileDefine.ClothesKind.shoes_outer);
            active = ChaControl.fileStatus.clothesState[currentShoes] == 0;

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

            LoadHeelFile();
        }

        internal void ClothesStateChangeEvent(ChaFileDefine.ClothesKind clothesKind)
        {
            if(clothesKind == ChaFileDefine.ClothesKind.shoes_inner || clothesKind == ChaFileDefine.ClothesKind.shoes_outer)
            {
                LoadHeelFile();
                StilettoGui.UpdateMakerValues(this);
            }
        }

        internal void ChangeCustomClothesEvent(ChaFileDefine.ClothesKind clothesKind)
        {
            if(clothesKind == ChaFileDefine.ClothesKind.shoes_inner || clothesKind == ChaFileDefine.ClothesKind.shoes_outer)
            {
                LoadHeelFile();
                StilettoGui.UpdateMakerValues(this);
            }
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

        private void OnPreRead()
        {
            if(flags.KNEE_BEND && solver != null)
                solver.bodyEffector.positionOffset = -HeightInternal;
            else
                body.localPosition = HeightInternal;
        }

        private void PostUpdate()
        {
            if(flags.KNEE_BEND && solver != null)
            {
                solver.rightFootEffector.target.position += HeightInternal;
                solver.leftFootEffector.target.position += HeightInternal;
                body.localPosition = Vector3.zero;
            }

            footL.localRotation *= AngleAInternal;
            footR.localRotation *= AngleAInternal;
            toesL.localRotation *= AngleBInternal;
            toesR.localRotation *= AngleBInternal;

            leg_L.localRotation *= AngleLegInternal;
            leg_R.localRotation *= AngleLegInternal;
        }

        private void SetFBBIK(FullBodyBipedIK fbbik)
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

        private void SaveHeelFile()
        {
            var configFile = Path.Combine(Stiletto.CONFIG_PATH, $"{HeelName}.xml");
            var shoeConfig = new XMLContainer(Id, AngleAInternal.eulerAngles.x, AngleLegInternal.eulerAngles.x, HeightInternal.y);

            using(var stream = new StreamWriter(configFile))
            using(var writer = XmlWriter.Create(stream, xmlWriterSettings))
                xmlSerializer.Serialize(writer, shoeConfig, xmlSerializerNamespaces);
        }

        private void LoadHeelFile()
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

            HeelName = fileName;
            Id = id;
            body = ChaControl.objBodyBone.transform.parent;

            this.height = new Vector3(0, height, 0);
            angleA = Quaternion.Euler(angleAnkle, 0f, 0f);
            angleB = Quaternion.Euler(-angleAnkle, 0f, 0f);
            this.angleLeg = Quaternion.Euler(angleLeg, 0f, 0f);

            SetFBBIK(ChaControl.animBody.GetComponent<FullBodyBipedIK>());
        }
    }
}
