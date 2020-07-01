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
        public HeelFlags flags;

        public string heelName { get; private set; } = "-- NONE --";
        public ChaControl cc;
        internal Vector3 height;
        internal Quaternion angleA;
        private Quaternion angleB;
        internal Quaternion angleLeg;
        internal bool aughStuff;

        private bool active;

        internal Vector3 Height
        {
            get => active && flags.ACTIVE && flags.HEIGHT ? height : Vector3.zero;
        }

        internal Quaternion AngleA
        {
            get => active && flags.ACTIVE && (flags.ANKLE_ROLL && !aughStuff) ? angleA : Quaternion.identity;
        }

        internal Quaternion AngleB
        {
            get => active && flags.ACTIVE && flags.TOE_ROLL ? angleB : Quaternion.identity;
        }

        internal Quaternion AngleLeg
        {
            get => active && flags.ACTIVE ? angleLeg : Quaternion.identity;
        }

        void Awake()
        {
            flags = new HeelFlags();
            cc = gameObject.GetComponent<ChaControl>();

            //Stiletto.RegisterHeelInfo(this);
        }

        //void OnDestroy()
        //{
        //}

        //void OnDisable()
        //{
        //    //Stiletto.UnregisterHeelInfo(this);
        //}

        //private bool registered = false;
        //private NPC npc = null;

        void Start()
        {
            //npc = Singleton<Manager.Game>.Instance.actScene.npcList.FirstOrDefault(x => x.chaCtrl.chaID == cc.chaID);
            //if (!npc)
            //TODO: cry
            Stiletto.RegisterHeelInfo(this);
        }

        void OnDestroy()
        {
            //Console.WriteLine("ONDESTROY - " + cc.fileParam.fullname);
            Stiletto.UnregisterHeelInfo(this);
        }

        internal void Update()
        {
            var currentShoes = (int)(cc.fileStatus.shoesType == 0 ? ClothesKind.shoes_inner : ClothesKind.shoes_outer);
            active = (cc.fileStatus.clothesState[currentShoes] == 0);
            if (animBody == null) return;
            var aci = animBody.GetCurrentAnimatorClipInfo(0);
            if (aci.Length == 0) return;


            var first = aci[0];
            animationName = first.clip.name;
            
            pathName = animBody.runtimeAnimatorController.name;
#warning optimise by detecting animation change?
            flags = Stiletto.FetchFlags(key);
        }

        internal void LateUpdate()
        {
            if (solver == null)
            {
                OnPreRead();
                PostUpdate();
            }
        }

        internal void UpdateValues(float height, float angleAnkle, float angleLeg, bool aughStuff)
        {
            this.height = new Vector3(0, height, 0);
            angleA = Quaternion.Euler(angleAnkle, 0f, 0f);
            angleB = Quaternion.Euler(-angleAnkle, 0f, 0f);
            this.angleLeg = Quaternion.Euler(angleLeg, 0f, 0f);
            this.aughStuff = aughStuff;
        }

        internal void Setup(string heelName, ChaControl chaControl, float height, float angleAnkle, float angleLeg, bool aughStuff)
        {
            animBody = chaControl.animBody;
            this.heelName = heelName;
            cc = chaControl;
            body = cc.objBodyBone.transform.parent;
            UpdateValues(height, angleAnkle, angleLeg, aughStuff);

            var waist = body.Find("cf_j_root/cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02");
            if (waist == null) return;

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

        internal Animator animBody;

        public string animationName { get; private set; } = "-- NONE --";
        public string pathName { get; private set; } = "-- NONE --";

        public string key
        {
            get => $"{pathName}/{animationName}";
        }

        internal void OnPreRead()
        {
#warning fix this
            if (flags.KNEE_BEND && solver != null)
            {
                solver.bodyEffector.positionOffset = -Height;
            }
            else
            {
                body.localPosition = Height;
            }
        }

        internal void PostUpdate()
        {
            if (flags.KNEE_BEND && solver != null)
            {
                solver.rightFootEffector.target.position += Height;
                solver.leftFootEffector.target.position += Height;
                body.localPosition = Vector3.zero;
            }

            footL.localRotation *= AngleA;
            footR.localRotation *= AngleA;
            toesL.localRotation *= AngleB;
            toesR.localRotation *= AngleB;

            //leg_L.localRotation = Quaternion.identity;
            //leg_R.localRotation = Quaternion.identity;

            leg_L.localRotation *= AngleLeg;
            leg_R.localRotation *= AngleLeg;
        }


        internal void SetFBBIK(FullBodyBipedIK fbbik)
        {
            if (fbbik != null) solver = fbbik.solver;

            if (solver != null)
            {
                
                if (!Stiletto.InStudio)
                {
                    var currentSceneName = fbbik.gameObject.scene.name;
                    if (! new[] { SceneNames.CustomScene, SceneNames.H, SceneNames.MyRoom }.Contains(currentSceneName))
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
#warning Detect overworld and adjust weights?

            if (Stiletto.InStudio)
            {
                Update();
                OnPreRead();
                PostUpdate();
            }
        }
    }
}
