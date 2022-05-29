using System;
using System.Collections;
using System.Linq;
using ActionGame.Chara;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ChaFileDefine;

namespace Stiletto
{
    internal class HeelInfo : MonoBehaviour
    {
        #region Fields

        private IKSolverFullBodyBiped _solver;
        private Transform _body;

        private Transform _legL;
        private Transform _footL;
        private Transform _toesL;

        private Transform _legR;
        private Transform _footR;
        private Transform _toesR;

        private bool _active;

        #endregion Fields

        public HeelFlags Flags;
        public ChaControl ChaControl;

        internal Animator AnimBody;
        internal Vector3 HeightValue;
        internal Quaternion AngleAnkleValue;
        internal Quaternion AngleLegValue;
        internal Quaternion AngleToesValue;
        internal bool LockAnkle;

        #region Properties

        public string HeelName { get; private set; } = "-- NONE --";

        public string AnimationName { get; private set; } = "-- NONE --";

        public string PathName { get; private set; } = "-- NONE --";

        public string Key => $"{PathName}/{AnimationName}";

        public Vector3 Height => _active && Flags.ACTIVE && Flags.HEIGHT ? HeightValue : Vector3.zero;

        public Quaternion AngleA => _active && Flags.ACTIVE && (Flags.ANKLE_ROLL && !LockAnkle) ? AngleAnkleValue : Quaternion.identity;

        public Quaternion AngleB => _active && Flags.ACTIVE && Flags.TOE_ROLL ? AngleToesValue : Quaternion.identity;

        public Quaternion AngleLeg => _active && Flags.ACTIVE ? AngleLegValue : Quaternion.identity;

        #endregion Properties

        #region Unity

        private void Start()
        {
            Stiletto.RegisterHeelInfo(this);
        }

        private void OnDestroy()
        {
            Stiletto.UnregisterHeelInfo(this);
        }

        private void Awake()
        {
            Flags = new HeelFlags();
            ChaControl = gameObject.GetComponent<ChaControl>();
        }

        internal void Update()
        {
            var currentShoes = (int)(ChaControl.fileStatus.shoesType == 0 ? ClothesKind.shoes_inner : ClothesKind.shoes_outer);
            _active = ChaControl.fileStatus.clothesState[currentShoes] == 0;

            if (AnimBody == null) return;

            var aci = AnimBody.GetCurrentAnimatorClipInfo(0);
            if (aci.Length == 0) return;

            var first = aci[0];
            AnimationName = first.clip.name;

            PathName = AnimBody.runtimeAnimatorController.name;

            // TODO from previous developper: #warning optimise by detecting animation change?
            Flags = Stiletto.FetchFlags(Key);
        }

        internal void LateUpdate()
        {
            if (_solver == null)
            {
                OnPreRead();
                OnPostUpdate();
            }
        }

        #endregion Unity

        internal void UpdateValues(float height, float angleAnkle, float angleLeg, bool aughStuff)
        {
            HeightValue = new Vector3(0, height, 0);
            AngleAnkleValue = Quaternion.Euler(angleAnkle, 0f, 0f);
            AngleToesValue = Quaternion.Euler(-angleAnkle, 0f, 0f);
            AngleLegValue = Quaternion.Euler(angleLeg, 0f, 0f);
            LockAnkle = aughStuff;
        }

        internal void Setup(string heelName, ChaControl chaControl, float height, float angleAnkle, float angleLeg, bool aughStuff)
        {
            AnimBody = chaControl.animBody;
            HeelName = heelName;
            ChaControl = chaControl;
            _body = ChaControl.objBodyBone.transform.parent;
            UpdateValues(height, angleAnkle, angleLeg, aughStuff);

            var waist = _body.Find("cf_j_root/cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02");
            if (waist == null) return;

            var legl3 = waist.Find("cf_j_thigh00_L/cf_j_leg01_L");
            _legL = legl3.Find("cf_j_leg03_L");
            _footL = _legL.Find("cf_j_foot_L");
            _toesL = _footL.Find("cf_j_toes_L");

            var legr3 = waist.Find("cf_j_thigh00_R/cf_j_leg01_R");
            _legR = legr3.Find("cf_j_leg03_R");
            _footR = _legR.Find("cf_j_foot_R");
            _toesR = _footR.Find("cf_j_toes_R");

            SetFBBIK(AnimBody.GetComponent<FullBodyBipedIK>());
        }

        internal void OnPreRead()
        {
            // TODO from previous developper: #warning fix this
            if (Flags.KNEE_BEND && _solver != null)
            {
                _solver.bodyEffector.positionOffset = -Height;
            }
            else
            {
                _body.localPosition = Height;
            }
        }

        internal void OnPostUpdate()
        {
            if (Flags.KNEE_BEND && _solver != null)
            {
                _solver.rightFootEffector.target.position += Height;
                _solver.leftFootEffector.target.position += Height;
                _body.localPosition = Vector3.zero;
            }

            _footL.localRotation *= AngleA;
            _footR.localRotation *= AngleA;
            _toesL.localRotation *= AngleB;
            _toesR.localRotation *= AngleB;
            _legL.localRotation *= AngleLeg;
            _legR.localRotation *= AngleLeg;
        }

        internal void SetFBBIK(FullBodyBipedIK fbbik)
        {
            if (fbbik != null) _solver = fbbik.solver;

            if (_solver != null)
            {
                if (!Stiletto.InStudio)
                {
                    var currentSceneName = fbbik.gameObject.scene.name;
                    if (!new[] { SceneNames.CustomScene, SceneNames.H }.Contains(currentSceneName))
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
                    _solver.IKPositionWeight = 1f;
                    fbbik.enabled = true;
                }
                _solver.OnPreRead = OnPreRead;
                _solver.OnPostUpdate = OnPostUpdate;
            }

            // TODO from previous developper: #warning Detect overworld and adjust weights?
            if (!Stiletto.InStudio) return;

            Update();
            OnPreRead();
            OnPostUpdate();
        }
    }
}