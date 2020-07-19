using KKAPI.Studio;
using RootMotion.FinalIK;
using Stiletto.Configurations;
using Stiletto.Models;
using System;
using System.Linq;
using UnityEngine;

namespace Stiletto
{
    public class HeelInfoAnimation
    {
        private Transform _body;
        private Transform _highWaist;
        private Transform _lowWaist;

        private Transform _leftThigh;
        private Transform _leftHighLeg;
        private Transform _leftLowLeg;
        private Transform _leftFoot;
        private Transform _leftToe;

        private Transform _rightThigh;
        private Transform _rightHighLeg;
        private Transform _rightLowLeg;
        private Transform _rightFoot;
        private Transform _rightToe;

        private Animator _animationBody;
        private FullBodyBipedIK _fullBodyBiped;
        private IKSolverFullBody _backupSolver;

        public HeelInfoAnimation() { }

        public HeelInfoAnimation(ChaControl chaControl)
        {
            _animationBody = chaControl.animBody;
            _body = chaControl.objBodyBone.transform.parent;

            _highWaist = _body.Find("cf_j_root/cf_n_height/cf_j_hips/cf_j_waist01");
            _lowWaist = _highWaist.Find("cf_j_waist02");

            if (_lowWaist == null) return;

            _leftThigh = _lowWaist.Find("cf_j_thigh00_L");
            _leftHighLeg = _leftThigh.Find("cf_j_leg01_L");
            _leftLowLeg = _leftHighLeg.Find("cf_j_leg03_L");
            _leftFoot = _leftLowLeg.Find("cf_j_foot_L");
            _leftToe = _leftFoot.Find("cf_j_toes_L");

            _rightThigh = _lowWaist.Find("cf_j_thigh00_R");
            _rightHighLeg = _rightThigh.Find("cf_j_leg01_R");
            _rightLowLeg = _rightHighLeg.Find("cf_j_leg03_R");
            _rightFoot = _rightLowLeg.Find("cf_j_foot_R");
            _rightToe = _rightFoot.Find("cf_j_toes_R");

            _fullBodyBiped = _animationBody.GetComponent<FullBodyBipedIK>();

            var spine = _body.Find("cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01");
            _backupSolver = new IKSolverFullBody();
            _backupSolver.Initiate(spine);

            SetupMotionWeights();
        }

        public IKSolverFullBodyBiped FullBodyBipedSolver => _fullBodyBiped?.solver;

        public bool FullBodyBipedEnabled => (_fullBodyBiped?.enabled ?? false) && FullBodyBipedSolver != null;

        public string AnimationPath => _animationBody?.runtimeAnimatorController?.name;

        public string AnimationName
        {
            get
            {
                var clipInfos = _animationBody?.GetCurrentAnimatorClipInfo(0);
                if (clipInfos == null || clipInfos.Length == 0)
                {
                    return null;
                }

                return clipInfos[0].clip.name;
            }
        }

        public void Update(AnimationFlags flags, Vector3 height, Quaternion ankle, Quaternion toe, Quaternion leg, CustomPose customPose)
        {
            if (flags.CUSTOM_POSE) 
            {
                var heelHeight = (float)Math.Pow(height.y * 100, 0.75);

                _lowWaist.localRotation *= Quaternion.Euler(-customPose.WaistAngle * heelHeight, 0, 0);

                _rightThigh.localRotation *= Quaternion.Euler(-customPose.RightThighAngle * heelHeight, 0, 0);
                _leftThigh.localRotation *= Quaternion.Euler(-customPose.LeftThighAngle * heelHeight, 0, 0);

                _rightHighLeg.localRotation *= Quaternion.Euler((customPose.RightThighAngle + customPose.RightLegAngle) * heelHeight, 0, 0);
                _leftHighLeg.localRotation *= Quaternion.Euler((customPose.LeftThighAngle + customPose.LeftLegAngle) * heelHeight, 0, 0);

                _rightLowLeg.localRotation *= Quaternion.Euler(-customPose.RightLegAngle * heelHeight, 0, 0);
                _leftLowLeg.localRotation *= Quaternion.Euler(-customPose.LeftLegAngle * heelHeight, 0, 0);

                _rightFoot.localRotation *= Quaternion.Euler(customPose.RightAnkleAngle, 0, 0);
                _leftFoot.localRotation *= Quaternion.Euler(customPose.LeftAnkleAngle, 0, 0);

                _body.localPosition = Vector3.zero;
            }
            else if (flags.KNEE_BEND && FullBodyBipedEnabled)
            {
                var halfHeight = height / 2;

                FullBodyBipedSolver.bodyEffector.positionOffset -= halfHeight;
                FullBodyBipedSolver.bodyEffector.positionWeight = 0f;

                FullBodyBipedSolver.rightFootEffector.positionOffset += halfHeight;
                FullBodyBipedSolver.rightFootEffector.positionWeight = 0f;

                FullBodyBipedSolver.leftFootEffector.positionOffset += halfHeight;
                FullBodyBipedSolver.leftFootEffector.positionWeight = 0f;

                _body.localPosition = Vector3.zero;
            }
            else
            {
                _body.localPosition = height;
            }

            _leftFoot.localRotation *= ankle;
            _rightFoot.localRotation *= ankle;

            _leftToe.localRotation *= toe;
            _rightToe.localRotation *= toe;

            _leftLowLeg.localRotation *= leg;
            _rightLowLeg.localRotation *= leg;
        }

        private void SetupMotionWeights() 
        {
            if (FullBodyBipedSolver != null && !StudioAPI.InsideStudio)
            {
                var currentSceneName = _fullBodyBiped.gameObject.scene.name;

                if (!new[] { SceneNames.CustomScene, SceneNames.H, SceneNames.MyRoom }.Contains(currentSceneName))
                {
                    //Disable arm weights, we only affect feet/knees.
                    _fullBodyBiped.GetIKSolver().Initiate(_fullBodyBiped.transform);
                    _fullBodyBiped.solver.leftHandEffector.positionWeight = 0f;
                    _fullBodyBiped.solver.rightHandEffector.positionWeight = 0f;
                    _fullBodyBiped.solver.leftArmChain.bendConstraint.weight = 0f;
                    _fullBodyBiped.solver.rightArmChain.bendConstraint.weight = 0f;
                    _fullBodyBiped.solver.leftFootEffector.rotationWeight = 0f;
                    _fullBodyBiped.solver.rightFootEffector.rotationWeight = 0f;
                }

                FullBodyBipedSolver.IKPositionWeight = 1f;
            }
        }
    }
}
