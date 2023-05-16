using KKAPI.Studio;
using Stiletto.Models;
using Stiletto.Settings;
using UnityEngine;
using static ChaFileDefine;

namespace Stiletto
{
    public class HeelInfo : MonoBehaviour
    {
        public AnimationFlags flags;
        public string heelName;
        public string animationName;
        public string animationPath;
        public ChaControl chaControl;

        private Vector3 _height;
        private Quaternion _ankleAngle;
        private Quaternion _toeAngle;
        private Quaternion _legAngle;

        private Vector3 _shoeScale;
        private float _shoeAngle;
        private Vector3 _shoeTranslate;
        private float _shoeShearY;
        private float _shoeShearZ;

        public CustomPose _customPose;

        private bool _active;

        private HeelInfoAnimation _animation;

        public float AnkleAngle
        {
            get => _ankleAngle.eulerAngles.x;
            set
            {
                _ankleAngle = Quaternion.Euler(value, 0f, 0f);
                _toeAngle = Quaternion.Euler(-value, 0f, 0f);
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

        public float ShoeScaleX
        {
            get => _shoeScale.x;
            set => _shoeScale.x = value;
        }

        public float ShoeScaleY
        {
            get => _shoeScale.y;
            set => _shoeScale.y = value;
        }

        public float ShoeScaleZ
        {
            get => _shoeScale.z;
            set => _shoeScale.z = value;
        }

        public float ShoeAngle
        {
            get => _shoeAngle;
            set => _shoeAngle = value;
        }

        public float ShoeTranslateY
        {
            get => _shoeTranslate.y;
            set => _shoeTranslate.y = value;
        }

        public float ShoeTranslateZ
        {
            get => _shoeTranslate.z;
            set => _shoeTranslate.z = value;
        }

        public float ShoeShearY
        {
            get => _shoeShearY;
            set => _shoeShearY = value;
        }

        public float ShoeShearZ
        {
            get => _shoeShearZ;
            set => _shoeShearZ = value;
        }

        public CustomPose CustomPose => _customPose;

        public bool HasAnimation => _animation.AnimationPath != null && _animation.AnimationName != null;

        public void Setup(ChaControl chaControl, string heelName)
        {
            this.chaControl = chaControl;
            _animation = new HeelInfoAnimation(chaControl);

            if (!string.IsNullOrEmpty(heelName))
            {
                this.heelName = heelName;
                var customHeel = StilettoContext.CustomHeelProvider.Load(heelName);
                Height = customHeel.Height;
                AnkleAngle = customHeel.AnkleAngle;
                LegAngle = customHeel.LegAngle;
                // Work around config parser defaulting to 0.
                ShoeScaleX = customHeel.ShoeScaleX != 0f ? customHeel.ShoeScaleX : 1f;
                ShoeScaleY = customHeel.ShoeScaleY != 0f ? customHeel.ShoeScaleY : 1f;
                ShoeScaleZ = customHeel.ShoeScaleZ != 0f ? customHeel.ShoeScaleZ : 1f;
                ShoeAngle = customHeel.ShoeAngle;
                ShoeTranslateY = customHeel.ShoeTranslateY;
                ShoeTranslateZ = customHeel.ShoeTranslateZ;
                ShoeShearY = customHeel.ShoeShearY;
                ShoeShearZ = customHeel.ShoeShearZ;
            }
            else 
            {
                this.heelName = null;
                Height = 0;
                AnkleAngle = 0;
                LegAngle = 0;
                _shoeScale = Vector3.one;
                ShoeAngle = 0;
                _shoeTranslate = Vector3.zero;
                ShoeShearY = 0;
                ShoeShearZ = 0;
            }


            if (_animation.FullBodyBipedSolver != null)
            {
                _animation.FullBodyBipedSolver.OnPostUpdate = PostUpdate;
            }

            StilettoContext.NotifyHeelInfoUpdate(this);

            if (StudioAPI.InsideStudio)
            {
                Update();
                PostUpdate();
            }
        }

        public void Reload() 
        {
            if (!string.IsNullOrEmpty(heelName)) 
            {
                Setup(chaControl, heelName);
            }

            animationPath = null;
            animationName = null;
        }

        private void Awake()
        {
            flags = new AnimationFlags();
            chaControl = gameObject.GetComponent<ChaControl>();
            _customPose = new CustomPose();
        }

        private void Start()
        {
            StilettoContext.RegisterHeelInfo(this);
        }

        private void OnDestroy()
        {
            StilettoContext.UnregisterHeelInfo(this);
        }

        private void Update()
        {
            var currentShoes = (int)(chaControl.fileStatus.shoesType == 0 ? ClothesKind.shoes_inner : ClothesKind.shoes_outer);
            _active = chaControl.fileStatus.clothesState[currentShoes] == 0;

            var path = _animation.AnimationPath;
            var name = _animation.AnimationName;
            var animationChange = false;

            if (path != animationPath || name != animationName)
            {
                flags = StilettoContext.AnimationFlagsProvider.Load(path, name);
                _customPose = StilettoContext.CustomPoseProvider.Load(path, name);
                animationChange = true;
            }

            animationPath = path;
            animationName = name;

            if (animationChange) 
            {
                StilettoContext.NotifyHeelInfoUpdate(this);
            }

            UpdateShoeWarp();
        }

        private void PostUpdate()
        {
            var height = _active && flags.ACTIVE && flags.HEIGHT ? _height : Vector3.zero;
            var ankleAngle = _active && flags.ACTIVE && flags.ANKLE_ROLL ? _ankleAngle : Quaternion.identity;
            var toeAngle = _active && flags.ACTIVE && flags.TOE_ROLL ? _toeAngle : Quaternion.identity;
            var legAngle = _active && flags.ACTIVE ? _legAngle : Quaternion.identity;
            var customPose = _active && flags.ACTIVE && flags.CUSTOM_POSE ? _customPose : new CustomPose();
            
            _animation?.Update(flags, height, ankleAngle, toeAngle, legAngle, customPose);
        }

        private void LateUpdate()
        {
            if (!_animation.FullBodyBipedEnabled)
            {
                PostUpdate();
            }
        }

        private void UpdateShoeWarp()
        {
            var currentShoes = (int)(chaControl.fileStatus.shoesType == 0 ? ClothesKind.shoes_inner : ClothesKind.shoes_outer);
            _active = chaControl.fileStatus.clothesState[currentShoes] == 0;

            var scale = _active && flags.ACTIVE ? _shoeScale : Vector3.one;
            var angle = _active && flags.ACTIVE ? _shoeAngle : 0;
            var translate = _active && flags.ACTIVE ? _shoeTranslate : Vector3.zero;

            if (chaControl.objClothes == null)
            {
                return;
            }
            var shoesObject = chaControl.objClothes[currentShoes];
            if (shoesObject == null)
            {
                return;
            }

            var shoesRenderer = shoesObject.GetComponentInChildren<SkinnedMeshRenderer>();
            if (shoesRenderer == null)
            {
                return;
            }
            if (shoesRenderer.gameObject == null)
            {
                return;
            }
            if (shoesRenderer.sharedMesh == null)
            {
                return;
            }

            var shoesBones = shoesRenderer.bones;
            if (shoesBones == null)
            {
                return;
            }

            for (int boneIndex = 0; boneIndex < shoesBones.Length; boneIndex++)
            {
                if (shoesBones[boneIndex] == null)
                {
                    continue;
                }

                if (shoesBones[boneIndex].name == "cf_j_foot_L" || shoesBones[boneIndex].name == "cf_j_foot_R")
                {
                    Matrix4x4[] shoesPoses = shoesRenderer.sharedMesh.bindposes;
                    if (shoesPoses == null)
                    {
                        continue;
                    }

                    Matrix4x4 footPose = shoesPoses[boneIndex];
                    if (footPose == null)
                    {
                        continue;
                    }

                    // Key on both the bone name and the shoe name
                    var poseKey = shoesBones[boneIndex].name + " " + heelName;
                    if (StilettoContext._baseShoeBindPoses.ContainsKey(poseKey))
                    {
                        footPose = (Matrix4x4)StilettoContext._baseShoeBindPoses[poseKey];
                    }
                    else
                    {
                        StilettoContext._baseShoeBindPoses[poseKey] = footPose;
                    }

                    // We apply the matrices in a specific order to make UX easier.
                    // Intended workflow: Choose body angles, then shoe angle, then shoe translate+scale.
                    Matrix4x4 matUndoAngle = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-AnkleAngle - LegAngle, 0f, 0f), Vector3.one);
                    Matrix4x4 matTranslate = Matrix4x4.Translate(translate);
                    Matrix4x4 matScale = Matrix4x4.Scale(scale);
                    Matrix4x4 matShear = Matrix4x4.identity;
                    matShear.m12 = Mathf.Tan(Mathf.Deg2Rad * ShoeShearY);
                    matShear.m21 = Mathf.Tan(Mathf.Deg2Rad * ShoeShearZ);
                    Matrix4x4 matRedoAngleAndRotate = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(AnkleAngle + LegAngle + angle, 0f, 0f), Vector3.one);
                    footPose = matUndoAngle * matShear * matTranslate * matScale * matRedoAngleAndRotate * footPose;

                    shoesPoses[boneIndex] = footPose;
                    shoesRenderer.sharedMesh.bindposes = shoesPoses;
                }
            }
        }
    }
}
