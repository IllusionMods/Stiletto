using KKAPI.Studio;
using RootMotion.FinalIK;
using Stiletto.Configurations;
using System.Linq;
using UnityEngine;
using static ChaFileDefine;

namespace Stiletto
{
    public class HeelInfo : MonoBehaviour
    {
        public HeelFlags flags;
        public string heelName = DisplaySettings.NONE_PLACEHOLDER;
        public string animationName = DisplaySettings.NONE_PLACEHOLDER;
        public string animationPath = DisplaySettings.NONE_PLACEHOLDER;

        public ChaControl chaControl;

        private Vector3 _height;
        private Quaternion _ankleAngleA;
        private Quaternion _ankleAngleB;
        private Quaternion _legAngle;
        private bool _active;

        private Vector3 GetHeight() { 
            return _active && flags.ACTIVE && flags.HEIGHT ? _height : Vector3.zero;
        }

        private Quaternion GetAnkleAngleA() 
        {
            return _active && flags.ACTIVE && flags.ANKLE_ROLL ? _ankleAngleA : Quaternion.identity;
        }

        private Quaternion GetAnkleAngleB()
        {
            return _active && flags.ACTIVE && flags.TOE_ROLL ? _ankleAngleB : Quaternion.identity;
        }

        private Quaternion GetLegAngle() 
        {
            return _active && flags.ACTIVE ? _legAngle : Quaternion.identity;
        }

        public float AnkleAngle
        {
            get => _ankleAngleA.eulerAngles.x;
            set
            {
                _ankleAngleA = Quaternion.Euler(value, 0f, 0f);
                _ankleAngleB = Quaternion.Euler(-value, 0f, 0f);
            }
        }

        public float LegAngle
        {
            get => _legAngle.eulerAngles.x;
            set => _legAngle = Quaternion.Euler(value, 0f, 0f);
        }

        public float Height
        {
            get => _height.y;
            set => _height = new Vector3(0, value, 0);
        }

        public bool HasAnimation => animBody != null;

        private void Awake()
        {
            flags = new HeelFlags();
            chaControl = gameObject.GetComponent<ChaControl>();
        }

        private void Start()
        {
            HeelInfoContext.RegisterHeelInfo(this);
        }

        private void OnDestroy()
        {
            HeelInfoContext.UnregisterHeelInfo(this);
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
            _ankleAngleA = Quaternion.Euler(angleAnkle, 0f, 0f);
            _ankleAngleB = Quaternion.Euler(-angleAnkle, 0f, 0f);
            _legAngle = Quaternion.Euler(angleLeg, 0f, 0f);
            
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

            HookFBBIK();
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
                solver.bodyEffector.positionOffset -= height;
                solver.bodyEffector.positionWeight = 0f;

                solver.rightFootEffector.positionOffset += height;
                solver.rightFootEffector.positionWeight = 0f;

                solver.leftFootEffector.positionOffset += height;
                solver.leftFootEffector.positionWeight = 0f;

                body.localPosition = Vector3.zero;
            }
            else 
            {
                body.localPosition = height;
            }

            var angleAnkleA = GetAnkleAngleA();
            var angleAnkleB = GetAnkleAngleB();
            var angleLeg = GetLegAngle();

            footL.localRotation *= angleAnkleA;
            footR.localRotation *= angleAnkleA;
            
            toesL.localRotation *= angleAnkleB;
            toesR.localRotation *= angleAnkleB;

            leg_L.localRotation *= angleLeg;
            leg_R.localRotation *= angleLeg;
        }

        private void HookFBBIK()
        {
            var fbbik = animBody.GetComponent<FullBodyBipedIK>();

            if (fbbik != null) {
                solver = fbbik.solver;
            }

            if(solver != null)
            {
                if(!StudioAPI.InsideStudio)
                {
                    var currentSceneName = fbbik.gameObject.scene.name;

                    if (!new[] { SceneNames.CustomScene, SceneNames.H, SceneNames.MyRoom }.Contains(currentSceneName))
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

            if(StudioAPI.InsideStudio)
            {
                Update();
                PostUpdate();
            }
        }
    }
}
