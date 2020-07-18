using KKAPI.Studio;
using Stiletto.Configurations;
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
        private Quaternion _ankleAngle;
        private Quaternion _toeAngle;
        private Quaternion _legAngle;
        public CustomPose _customPose;
        private bool _active;

        private HeelInfoAnimation _animation = new HeelInfoAnimation();

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

        internal void Setup(string heelName, ChaControl chaControl, float height, float ankleAngle, float legAngle)
        {
            this.chaControl = chaControl;
            this.heelName = heelName;
            _animation = new HeelInfoAnimation(chaControl);

            Height = height;
            AnkleAngle = ankleAngle;
            LegAngle = legAngle;

            StilettoMakerGUI.UpdateMakerValues(this);

            if (_animation.FullBodyBipedSolver != null)
            {
                _animation.FullBodyBipedSolver.OnPostUpdate = PostUpdate;
            }

            if (StudioAPI.InsideStudio)
            {
                Update();
                PostUpdate();
            }
        }

        private void Awake()
        {
            flags = new HeelFlags();
            chaControl = gameObject.GetComponent<ChaControl>();
            _customPose = new CustomPose();
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

            var path = _animation.AnimationPath;
            var name = _animation.AnimationName;

            if (path != animationPath || name != animationName)
            {
                flags = HeelFlagsProvider.GetFlags(path, name);
                _customPose = CustomPoseProvider.LoadCustomPose(animationPath);
                StilettoMakerGUI.UpdateFlagsValues(path, name, flags);
            }

            animationPath = path;
            animationName = name;
        }

        private void PostUpdate()
        {
            var height = _active && flags.ACTIVE && flags.HEIGHT ? _height : Vector3.zero;
            var ankleAngle = _active && flags.ACTIVE && flags.ANKLE_ROLL ? _ankleAngle : Quaternion.identity;
            var toeAngle = _active && flags.ACTIVE && flags.TOE_ROLL ? _toeAngle : Quaternion.identity;
            var legAngle = _active && flags.ACTIVE ? _legAngle : Quaternion.identity;
            var customPose = _active && flags.ACTIVE && flags.CUSTOM_POSE ? _customPose : new CustomPose();

            if (_active && flags.ACTIVE) 
            { 
                _animation.Update(flags, height, ankleAngle, toeAngle, legAngle, customPose);
            }
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
