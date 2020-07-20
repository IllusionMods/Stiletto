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

        private GeneralSettings _generalSetting;

        public HeelInfoAnimation(ChaControl chaControl)
        {
            _generalSetting = StilettoContext._generalSettingsProvider.Value;
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
            _generalSetting = StilettoContext._generalSettingsProvider.Value;

            if (flags.CUSTOM_POSE)
            {
                // Get Leg Heights
                var rightThighLength = Vector3.Distance(_lowWaist.position, _rightHighLeg.position);
                var leftThighLength = Vector3.Distance(_lowWaist.position, _leftHighLeg.position);
                var rightLegLength = Vector3.Distance(_rightHighLeg.position, _rightLowLeg.position);
                var leftLegLength = Vector3.Distance(_leftHighLeg.position, _leftLowLeg.position);

                // Calculate Deltas
                var heelHeightDelta = GetHeelHeightDelta(height.y);
                var rightThighDelta = GetBodyLengthDelta(rightThighLength);
                var leftThighDelta = GetBodyLengthDelta(leftThighLength);
                var rightLegDelta = GetBodyLengthDelta(rightLegLength);
                var leftLegDelta = GetBodyLengthDelta(leftLegLength);
                
                // Calcuate adjustment angles
                var waistAngle = customPose.WaistAngle * heelHeightDelta;

                var rightThighAngle = customPose.RightThighAngle * heelHeightDelta * rightThighDelta;
                var leftThighAngle = customPose.LeftThighAngle * heelHeightDelta * leftThighDelta;

                var rightLowLegAngle = customPose.RightLegAngle * heelHeightDelta * rightLegDelta;
                var leftLowLegAngle = customPose.LeftLegAngle * heelHeightDelta * leftLegDelta;

                var rightHighLegAngle = rightThighAngle + rightLowLegAngle;
                var leftHighLegAngle = leftThighAngle + leftLowLegAngle;

                // Apply adjustments
                _lowWaist.localRotation *= Quaternion.Euler(-waistAngle, 0, 0);

                _rightThigh.localRotation *= Quaternion.Euler(-rightThighAngle, 0, 0);
                _leftThigh.localRotation *= Quaternion.Euler(-leftThighAngle, 0, 0);

                _rightHighLeg.localRotation *= Quaternion.Euler(rightHighLegAngle, 0, 0);
                _leftHighLeg.localRotation *= Quaternion.Euler(leftHighLegAngle, 0, 0);

                _rightLowLeg.localRotation *= Quaternion.Euler(-rightLowLegAngle, 0, 0);
                _leftLowLeg.localRotation *= Quaternion.Euler(-leftLowLegAngle, 0, 0);

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

        private float GetBodyLengthDelta(float length)
        { 
            return (float)Math.Pow(length, _generalSetting.CustomPose_BodyLengthPower) * _generalSetting.CustomPose_BodyLengthMultiplier;
        }

        private float GetHeelHeightDelta(float height)
        { 
            return (float)Math.Pow(height, _generalSetting.CustomPose_HeelHeightPower) * _generalSetting.CustomPose_HeelHeightMultiplier;
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
