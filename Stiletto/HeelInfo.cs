using KKAPI.Studio;
using RootMotion.FinalIK;
using Stiletto.Configurations;
using System;
using System.Linq;
using UnityEngine;
using static ChaFileDefine;

namespace Stiletto
{
    public class HeelInfo : MonoBehaviour
    {
        public HeelFlags flags;
        public string heelName = "-- NONE --";
        public string animationName = "-- NONE --";
        public string animationPath = "-- NONE --";

        public ChaControl chaControl;

        private Vector3 _height;
        private Quaternion _angleAnkleA;
        private Quaternion _angleAnkleB;
        private Quaternion _angleLeg;
        private bool _active;

        private Vector3 GetHeight() { 
            return _active && flags.ACTIVE && flags.HEIGHT ? _height : Vector3.zero;
        }

        private Quaternion GetAngleAnkleA() 
        {
            return _active && flags.ACTIVE && flags.ANKLE_ROLL ? _angleAnkleA : Quaternion.identity;
        }

        private Quaternion GetAngleAnkleB()
        {
            return _active && flags.ACTIVE && flags.TOE_ROLL ? _angleAnkleB : Quaternion.identity;
        }

        private Quaternion GetAngleLeg() 
        {
            return _active && flags.ACTIVE ? _angleLeg : Quaternion.identity;
        }

        public float AngleAnkle
        {
            get => _angleAnkleA.eulerAngles.x;
            set
            {
                _angleAnkleA = Quaternion.Euler(value, 0f, 0f);
                _angleAnkleB = Quaternion.Euler(-value, 0f, 0f);
            }
        }

        public float AngleLeg
        {
            get => _angleLeg.eulerAngles.x;
            set => _angleLeg = Quaternion.Euler(value, 0f, 0f);
        }

        public float Height
        {
            get => _height.y;
            set => _height = new Vector3(0, value, 0);
        }

        private void Awake()
        {
            flags = new HeelFlags();
            chaControl = gameObject.GetComponent<ChaControl>();

            //Stiletto.RegisterHeelInfo(this);
        }

        //void OnDisable()
        //{
        //    //Stiletto.UnregisterHeelInfo(this);
        //}

        //private bool registered = false;
        //private NPC npc = null;

        private void Start()
        {
            //npc = Singleton<Manager.Game>.Instance.actScene.npcList.FirstOrDefault(x => x.chaCtrl.chaID == cc.chaID);
            //if (!npc)
            //TODO: cry
            Stiletto.RegisterHeelInfo(this);
        }

        private void OnDestroy()
        {
            //Console.WriteLine("ONDESTROY - " + cc.fileParam.fullname);
            Stiletto.UnregisterHeelInfo(this);
        }

        private void Update()
        {
            var currentShoes = (int)(chaControl.fileStatus.shoesType == 0 ? ClothesKind.shoes_inner : ClothesKind.shoes_outer);
            _active = chaControl.fileStatus.clothesState[currentShoes] == 0;

            if (animBody == null)
            {
                return;
            }

            var clipInfos = animBody.GetCurrentAnimatorClipInfo(0);
            if (clipInfos.Length == 0)
            {
                return;
            }

            var name = clipInfos[0].clip.name;
            var path = animBody.runtimeAnimatorController.name;

            if (name != animationName || path != animationPath)
            {
                flags = HeelFlagsProvider.GetFlags(path, name);
                StilettoMakerGUI.UpdateFlagsValues(path, name, flags);
            }

            animationName = name;
            animationPath = path;
        }

        private void UpdateValues(float height, float angleAnkle, float angleLeg)
        {
            _height = new Vector3(0, height, 0);
            _angleAnkleA = Quaternion.Euler(angleAnkle, 0f, 0f);
            _angleAnkleB = Quaternion.Euler(-angleAnkle, 0f, 0f);
            _angleLeg = Quaternion.Euler(angleLeg, 0f, 0f);
            
            StilettoMakerGUI.UpdateMakerValues(this);
        }

        internal void Setup(string heelName, ChaControl chaControl, float height, float angleAnkle, float angleLeg)
        {
            this.chaControl = chaControl;
            this.animBody = chaControl.animBody;
            this.heelName = heelName;
            this.body = chaControl.objBodyBone.transform.parent;

            UpdateValues(height, angleAnkle, angleLeg);

            var waist = body.Find("cf_j_root/cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02");
            if(waist == null) return;

            var legl3 = waist.Find("cf_j_thigh00_L/cf_j_leg01_L");
            leg_L = legl3.Find("cf_j_leg03_L");
            footL = leg_L.Find("cf_j_foot_L");
            toesL = footL.Find("cf_j_toes_L");

            var legr3 = waist.Find("cf_j_thigh00_R/cf_j_leg01_R");
            leg_R = legr3.Find("cf_j_leg03_R");
            footR = leg_R.Find("cf_j_foot_R");
            toesR = footR.Find("cf_j_toes_R");

            SetFBBIK(animBody.GetComponent<FullBodyBipedIK>());
        }

        private IKSolverFullBodyBiped solver;
        private Transform body = null;

        private Transform leg_L = null;
        private Transform footL = null;
        private Transform toesL = null;

        private Transform leg_R = null;
        private Transform footR = null;
        private Transform toesR = null;

        private Animator animBody;

        private void PostUpdate()
        {
            var height = GetHeight();

            if (flags.KNEE_BEND && solver != null)
            {
                solver.rightFootEffector.target.position += height;
                solver.leftFootEffector.target.position += height;
                solver.bodyEffector.positionOffset = -height;
                body.localPosition = Vector3.zero;
            }
            else 
            {
                body.localPosition = height;
            }

            var angleAnkleA = GetAngleAnkleA();
            var angleAnkleB = GetAngleAnkleB();
            var angleLeg = GetAngleLeg();

            footL.localRotation *= angleAnkleA;
            footR.localRotation *= angleAnkleA;
            
            toesL.localRotation *= angleAnkleB;
            toesR.localRotation *= angleAnkleB;

            leg_L.localRotation *= angleLeg;
            leg_R.localRotation *= angleLeg;
        }

        private void SetFBBIK(FullBodyBipedIK fbbik)
        {
            if (fbbik != null) {
                solver = fbbik.solver;
            }

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
                solver.OnPostUpdate = PostUpdate;
            }
#warning Detect overworld and adjust weights?

            if(StudioAPI.InsideStudio)
            {
                Update();
                PostUpdate();
            }
        }
    }
}
