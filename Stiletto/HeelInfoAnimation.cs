using KKAPI.Studio;
using RootMotion.FinalIK;
using Stiletto.Models;
using Stiletto.Settings;
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

        private float _bodyPositionWeight;
        private float _leftFootPositionWeight;
        private float _rightFootPositionWeight;

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

            if (FullBodyBipedSolver != null)
            {
                _bodyPositionWeight = FullBodyBipedSolver.bodyEffector.positionWeight;
                _leftFootPositionWeight = FullBodyBipedSolver.leftFootEffector.positionWeight;
                _rightFootPositionWeight = FullBodyBipedSolver.rightFootEffector.positionWeight;
            }
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

        public void Update(AnimationFlags flags, Vector3 height, Quaternion ankleAngle, Quaternion toeAngle, Quaternion legAngle, CustomPose customPose)
        {
            _generalSetting = StilettoContext._generalSettingsProvider.Value;

            // Reset position weight
            if (FullBodyBipedSolver != null)
            {
                FullBodyBipedSolver.bodyEffector.positionWeight = _bodyPositionWeight;
                FullBodyBipedSolver.rightFootEffector.positionWeight = _rightFootPositionWeight;
                FullBodyBipedSolver.leftFootEffector.positionWeight = _leftFootPositionWeight;
            }

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
                var halfHeight = height * _generalSetting.KneeBend_HeelHeightMultiplier;

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

            _leftFoot.localRotation *= ankleAngle;
            _rightFoot.localRotation *= ankleAngle;

            _leftToe.localRotation *= toeAngle;
            _rightToe.localRotation *= toeAngle;

            _leftLowLeg.localRotation *= legAngle;
            _rightLowLeg.localRotation *= legAngle;
        }

        private float GetBodyLengthDelta(float length)
        { 
            return (float)Math.Pow(length, _generalSetting.CustomPose_BodyLengthPower) * _generalSetting.CustomPose_BodyLengthMultiplier;
        }

        private float GetHeelHeightDelta(float height)
        { 
            return (float)Math.Pow(height, _generalSetting.CustomPose_HeelHeightPower) * _generalSetting.CustomPose_HeelHeightMultiplier;
        }
    }
}
