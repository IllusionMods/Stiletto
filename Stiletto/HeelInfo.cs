using KKAPI.Studio;
using Sirenix.OdinInspector.Demos;
using Stiletto.Models;
using UnityEngine;
using static ChaFileDefine;

namespace Stiletto
{
    public class HeelInfo : MonoBehaviour
    {
        public AnimationFlags flags;
        public string heelName = RootSettings.NONE_PLACEHOLDER;
        public string animationName = RootSettings.NONE_PLACEHOLDER;
        public string animationPath = RootSettings.NONE_PLACEHOLDER;
        public ChaControl chaControl;

        private Vector3 _height;
        private Quaternion _ankleAngle;
        private Quaternion _toeAngle;
        private Quaternion _legAngle;
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

        public CustomPose CustomPose => _customPose;

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
            }
            else 
            {
                this.heelName = RootSettings.NONE_PLACEHOLDER;
                Height = 0;
                AnkleAngle = 0;
                LegAngle = 0;
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
            if (!string.IsNullOrEmpty(heelName) && heelName != RootSettings.NONE_PLACEHOLDER) 
            {
                Setup(chaControl, heelName);
                animationPath = null;
                animationName = null;
            }
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
    }
}
