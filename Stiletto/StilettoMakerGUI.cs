using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Studio;
using KKAPI.Studio.UI;
using Stiletto.Configurations;
using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Stiletto
{
    internal static class StilettoMakerGUI
    {
        private static MakerText text_heelName;
        private static MakerSlider slider_AngleAnkle;
        private static MakerSlider slider_AngleLeg;
        private static MakerSlider slider_Height;
        private static MakerButton button_HeelSave;

        private static MakerText text_poseName;
        private static MakerToggle toggle_Active;
        private static MakerToggle toggle_Height;
        private static MakerToggle toggle_ToeRool;
        private static MakerToggle toggle_AnkleRool;
        private static MakerToggle toggle_KneeBend;
        private static MakerButton button_FlagsSave;

        private static MakerButton button_Reload;

        internal static void Start()
        {
            if(StudioAPI.InsideStudio) 
            { 
                RegisterStudioControls();
            }
            else 
            { 
                MakerAPI.RegisterCustomSubCategories += RegisterMakerControls;
            }
        }

        private static void RegisterMakerControls(object _, RegisterSubCategoriesEvent e)
        {
            var shoesCategory = MakerConstants.Clothes.OuterShoes;
            var category = new MakerCategory(shoesCategory.CategoryName, "stiletto", shoesCategory.Position + 1, "Stiletto");
            e.AddSubCategory(category);

            text_heelName = e.AddControl(new MakerText("<heel_name>", category, Stiletto.Instance));
            slider_AngleAnkle = e.AddControl(new MakerSlider(category, "AngleAnkle", 0f, 60f, 0f, Stiletto.Instance) { 
                StringToValue = CreateStringToValueFunc(10f), ValueToString = CreateValueToStringFunc(10f), 
            });
            slider_AngleLeg = e.AddControl(new MakerSlider(category, "AngleLeg", 0f, 60f, 0f, Stiletto.Instance) { 
                StringToValue = CreateStringToValueFunc(10f), ValueToString = CreateValueToStringFunc(10f) 
            });
            slider_Height = e.AddControl(new MakerSlider(category, "Height", 0f, 0.5f, 0f, Stiletto.Instance) { 
                StringToValue = CreateStringToValueFunc(1000f), ValueToString = CreateValueToStringFunc(1000f) 
            });
            button_HeelSave = e.AddControl(new MakerButton("Save", category, Stiletto.Instance));

            slider_AngleAnkle.ValueChanged.Subscribe(x => HeelValueChange(nameof(HeelConfig.AngleAnkle), x));
            slider_AngleLeg.ValueChanged.Subscribe(x => HeelValueChange(nameof(HeelConfig.AngleLeg), x));
            slider_Height.ValueChanged.Subscribe(x => HeelValueChange(nameof(HeelConfig.Height), x));
            button_HeelSave.OnClick.AddListener(SaveHeelInfo);

            e.AddControl(new MakerSeparator(category, Stiletto.Instance));

            text_poseName = e.AddControl(new MakerText("<pose_name>", category, Stiletto.Instance));
            
            toggle_Active = e.AddControl(new MakerToggle(category, "Active", Stiletto.Instance));
            toggle_Height = e.AddControl(new MakerToggle(category, "Height", Stiletto.Instance));
            toggle_ToeRool = e.AddControl(new MakerToggle(category, "ToeRool", Stiletto.Instance));
            toggle_AnkleRool = e.AddControl(new MakerToggle(category, "AnkleRool", Stiletto.Instance));
            toggle_KneeBend = e.AddControl(new MakerToggle(category, "KeenBend", Stiletto.Instance));
            button_FlagsSave = e.AddControl(new MakerButton("Save", category, Stiletto.Instance));

            toggle_Active.ValueChanged.Subscribe(x => PoseValueChange(nameof(HeelFlags.ACTIVE), x));
            toggle_Height.ValueChanged.Subscribe(x => PoseValueChange(nameof(HeelFlags.HEIGHT), x));
            toggle_ToeRool.ValueChanged.Subscribe(x => PoseValueChange(nameof(HeelFlags.TOE_ROLL), x));
            toggle_AnkleRool.ValueChanged.Subscribe(x => PoseValueChange(nameof(HeelFlags.ANKLE_ROLL), x));
            toggle_KneeBend.ValueChanged.Subscribe(x => PoseValueChange(nameof(HeelFlags.KNEE_BEND), x));
            button_FlagsSave.OnClick.AddListener(SaveHeelFlags);

            e.AddControl(new MakerSeparator(category, Stiletto.Instance));

            button_Reload = e.AddControl(new MakerButton("Reload Configuration Files", category, Stiletto.Instance));
            button_Reload.OnClick.AddListener(ReloadConfiguration);
        }

        public static void UpdateMakerValues(HeelInfo heelInfo)
        {
            if (MakerAPI.InsideMaker && heelInfo != null)
            {
                if (slider_Height != null) 
                { 
                    text_heelName.Text = heelInfo.heelName;
                    slider_AngleAnkle.Value = heelInfo.AngleAnkle;
                    slider_AngleLeg.Value = heelInfo.AngleLeg;
                    slider_Height.Value = heelInfo.Height;
                }
            }
        }

        public static void UpdateFlagsValues(string path, string name, HeelFlags flags)
        {
            if (MakerAPI.InsideMaker)
            {
                if (toggle_KneeBend != null)
                {
                    text_poseName.Text = $"{path}/{name}";
                    toggle_Active.Value = flags.ACTIVE;
                    toggle_Height.Value = flags.HEIGHT;
                    toggle_ToeRool.Value = flags.TOE_ROLL;
                    toggle_AnkleRool.Value = flags.ANKLE_ROLL;
                    toggle_KneeBend.Value = flags.KNEE_BEND;
                }
            }
        }

        private static void PoseValueChange(string type, bool value)
        {
            var heelInfo = MakerAPI.GetCharacterControl().GetComponent<HeelInfo>();

            if (heelInfo != null)
            {
                switch (type)
                {
                    case nameof(HeelFlags.ACTIVE):
                        heelInfo.SafeProc(x => x.flags.ACTIVE = value);
                        break;
                    case nameof(HeelFlags.ANKLE_ROLL):
                        heelInfo.SafeProc(x => x.flags.ANKLE_ROLL = value);
                        break;
                    case nameof(HeelFlags.TOE_ROLL):
                        heelInfo.SafeProc(x => x.flags.TOE_ROLL = value);
                        break;
                    case nameof(HeelFlags.HEIGHT):
                        heelInfo.SafeProc(x => x.flags.HEIGHT = value);
                        break;
                    case nameof(HeelFlags.KNEE_BEND):
                        heelInfo.SafeProc(x => x.flags.KNEE_BEND = value);
                        break;
                }
            }
        }

        private static void HeelValueChange(string type, float value) 
        {
            var heelInfo = MakerAPI.GetCharacterControl().GetComponent<HeelInfo>();

            if (heelInfo != null) 
            {
                switch (type)
                {
                    case nameof(HeelConfig.AngleAnkle):
                        heelInfo.SafeProc(x => x.AngleAnkle = value);
                        break;
                    case nameof(HeelConfig.AngleLeg):
                        heelInfo.SafeProc(x => x.AngleLeg = value);
                        break;
                    case nameof(HeelConfig.Height):
                        heelInfo.SafeProc(x => x.Height = value);
                        break;
                }
            }
        }

        private static void SaveHeelInfo()
        {
            var heelInfo = MakerAPI.GetCharacterControl().GetComponent<HeelInfo>();

            if (heelInfo != null) 
            {
                HeelConfigProvider.SaveHeelFile(heelInfo.heelName, new HeelConfig
                {
                    AngleAnkle = heelInfo.AngleAnkle, 
                    AngleLeg = heelInfo.AngleLeg, 
                    Height = heelInfo.Height,
                });
            }
        }

        private static void SaveHeelFlags()
        {
            var heelInfo = MakerAPI.GetCharacterControl().GetComponent<HeelInfo>();

            if (heelInfo != null)
            {
                HeelFlagsProvider.SaveFlags(heelInfo.animationPath, heelInfo.animationName, heelInfo.flags);
            }
        }

        private static void ReloadConfiguration() 
        {
            HeelFlagsProvider.ReloadHeelFlags();
        }

        private static Func<string, float> CreateStringToValueFunc(float multi)
        {
            return new Func<string, float>(txt => float.Parse(txt) / multi);
        }

        private static Func<float, string> CreateValueToStringFunc(float multi)
        {
            return new Func<float, string>(f => Mathf.RoundToInt(f * multi).ToString());
        }

        private static void RegisterStudioControls()
        {
            var slider_AngleAnkle = CreateSlider("AngleAnkle", ctrl => ctrl.AngleAnkle, (ctrl, f) => ctrl.AngleAnkle = f, 0f, 60f);
            var slider_AngleLeg = CreateSlider("AngleLeg", ctrl => ctrl.AngleLeg, (ctrl, f) => ctrl.AngleLeg = f, 0f, 60f);
            var slider_Height = CreateSlider("Height", ctrl => ctrl.Height, (ctrl, f) => ctrl.Height = f, 0f, 0.5f);

            StudioAPI.GetOrCreateCurrentStateCategory("Stiletto").AddControls(slider_AngleAnkle, slider_AngleLeg, slider_Height);

            CurrentStateCategorySlider CreateSlider(string name, Func<HeelInfo, float> get, Action<HeelInfo, float> set, float minValue, float maxValue)
            {
                var slider = new CurrentStateCategorySlider(name, (chara) => get(chara.charInfo.GetComponent<HeelInfo>()), minValue, maxValue);
                slider.Value.Subscribe(x =>
                {
                    foreach(var heelInfo in StudioAPI.GetSelectedCharacters().Select(y => y.charInfo.gameObject.GetComponent<HeelInfo>()))
                        set(heelInfo, x);
                });

                return slider;
            }
        }
    }
}
