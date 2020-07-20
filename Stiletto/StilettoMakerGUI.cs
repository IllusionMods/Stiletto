using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Studio;
using KKAPI.Studio.UI;
using Stiletto.Models;
using Stiletto.Settings;
using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Stiletto
{
    public class StilettoMakerGUI : IDisposable
    {
        private MakerText text_heelName;
        private MakerSlider slider_AnkleAngle;
        private MakerSlider slider_LegAngle;
        private MakerSlider slider_Height;
        private MakerButton button_HeelSave;

        private MakerText text_poseName;
        private MakerToggle toggle_Active;
        private MakerToggle toggle_Height;
        private MakerToggle toggle_ToeRoll;
        private MakerToggle toggle_AnkleRoll;
        private MakerToggle toggle_KneeBend;
        private MakerButton button_FlagsSave;

        private MakerButton button_Reload;

        private DisplaySettings displaySettings;

        public StilettoMakerGUI(Stiletto plugin)
        {
            displaySettings = StilettoContext._displaySettingsProvider.Value;


            if (StudioAPI.InsideStudio)
            {
                RegisterStudioControls();
            }
            else
            {
                MakerAPI.RegisterCustomSubCategories += (_, e) => RegisterMakerControls(plugin, e);
            }

            StilettoContext.OnHeelInfoUpdate += OnHeelInfoUpdate;
        }

        private void RegisterMakerControls(Stiletto plugin, RegisterSubCategoriesEvent e)
        {
            displaySettings = StilettoContext._displaySettingsProvider.Value;

            var shoesCategory = MakerConstants.Clothes.OuterShoes;
            var category = new MakerCategory(shoesCategory.CategoryName, "stiletto", shoesCategory.Position + 1, displaySettings.Stiletto);
            e.AddSubCategory(category);

            text_heelName = e.AddControl(new MakerText(displaySettings.Default_Heel_Name, category, plugin));
            slider_AnkleAngle = e.AddControl(new MakerSlider(category, displaySettings.Ankle_Angle, 0f, 60f, 0f, plugin)
            {
                StringToValue = CreateStringToValueFunc(10f),
                ValueToString = CreateValueToStringFunc(10f),
            });
            slider_LegAngle = e.AddControl(new MakerSlider(category, displaySettings.Leg_Angle, 0f, 60f, 0f, plugin)
            {
                StringToValue = CreateStringToValueFunc(10f),
                ValueToString = CreateValueToStringFunc(10f)
            });
            slider_Height = e.AddControl(new MakerSlider(category, displaySettings.Height, 0f, 0.5f, 0f, plugin)
            {
                StringToValue = CreateStringToValueFunc(1000f),
                ValueToString = CreateValueToStringFunc(1000f)
            });
            button_HeelSave = e.AddControl(new MakerButton(displaySettings.Save, category, plugin));

            slider_AnkleAngle.ValueChanged.Subscribe(value =>
                MakerHeelInfoProcess(heel => heel.SafeProc(x => x.AnkleAngle = value))
            );

            slider_LegAngle.ValueChanged.Subscribe(value =>
                MakerHeelInfoProcess(heel => heel.SafeProc(x => x.LegAngle = value))
            );

            slider_Height.ValueChanged.Subscribe(value =>
                MakerHeelInfoProcess(heel => heel.SafeProc(x => x.Height = value))
            );

            button_HeelSave.OnClick.AddListener(() =>
                MakerHeelInfoProcess(heel => StilettoContext.CustomHeelProvider.Save(heel.heelName, new CustomHeel(heel))
            ));

            e.AddControl(new MakerSeparator(category, plugin));

            text_poseName = e.AddControl(new MakerText(displaySettings.Default_Animation_Name, category, plugin));

            toggle_Active = e.AddControl(new MakerToggle(category, displaySettings.Active, plugin));
            toggle_Height = e.AddControl(new MakerToggle(category, displaySettings.Height, plugin));
            toggle_AnkleRoll = e.AddControl(new MakerToggle(category, displaySettings.Ankle_Roll, plugin));
            toggle_ToeRoll = e.AddControl(new MakerToggle(category, displaySettings.Toe_Roll, plugin));
            toggle_KneeBend = e.AddControl(new MakerToggle(category, displaySettings.Knee_Bend, plugin));
            button_FlagsSave = e.AddControl(new MakerButton(displaySettings.Save, category, plugin));

            toggle_Active.ValueChanged.Subscribe(value =>
                MakerHeelInfoProcess(heel => heel.SafeProc(x => x.flags.ACTIVE = value))
            );
            toggle_Height.ValueChanged.Subscribe(value =>
                MakerHeelInfoProcess(heel => heel.SafeProc(x => x.flags.HEIGHT = value))
            );
            toggle_ToeRoll.ValueChanged.Subscribe(value =>
                MakerHeelInfoProcess(heel => heel.SafeProc(x => x.flags.TOE_ROLL = value))
            );
            toggle_AnkleRoll.ValueChanged.Subscribe(value =>
                MakerHeelInfoProcess(heel => heel.SafeProc(x => x.flags.ANKLE_ROLL = value))
            );
            toggle_KneeBend.ValueChanged.Subscribe(value =>
                MakerHeelInfoProcess(heel => heel.SafeProc(x => x.flags.KNEE_BEND = value))
            );
            button_FlagsSave.OnClick.AddListener(() =>
                MakerHeelInfoProcess(heel => StilettoContext.AnimationFlagsProvider.Save(heel.animationPath, heel.animationName, heel.flags))
            );

            e.AddControl(new MakerSeparator(category, plugin));

            button_Reload = e.AddControl(new MakerButton(displaySettings.Reload_Configurations, category, plugin));
            button_Reload.OnClick.AddListener(StilettoContext.ReloadConfigurations);
        }

        public void OnHeelInfoUpdate(object _, HeelInfoEventArgs heelInfo)
        {
            if (MakerAPI.InsideMaker && heelInfo != null)
            {
                if (slider_Height != null)
                {
                    text_heelName.Text = heelInfo.HeelName ?? displaySettings.Default_Heel_Name;
                    slider_AnkleAngle.Value = heelInfo.CustomHeel.AnkleAngle;
                    slider_LegAngle.Value = heelInfo.CustomHeel.LegAngle;
                    slider_Height.Value = heelInfo.CustomHeel.Height;
                }

                if (toggle_KneeBend != null)
                {
                    text_poseName.Text = heelInfo.AnimationKey ?? displaySettings.Default_Animation_Name;
                    toggle_Active.Value = heelInfo.AnimationFlags.ACTIVE;
                    toggle_Height.Value = heelInfo.AnimationFlags.HEIGHT;
                    toggle_ToeRoll.Value = heelInfo.AnimationFlags.TOE_ROLL;
                    toggle_AnkleRoll.Value = heelInfo.AnimationFlags.ANKLE_ROLL;
                    toggle_KneeBend.Value = heelInfo.AnimationFlags.KNEE_BEND;
                }
            }
        }

        public static void MakerHeelInfoProcess(Action<HeelInfo> action)
        {
            var heelInfo = MakerAPI.GetCharacterControl().GetComponent<HeelInfo>();
            action(heelInfo);
        }

        private Func<string, float> CreateStringToValueFunc(float multi)
        {
            return new Func<string, float>(txt => float.Parse(txt) / multi);
        }

        private Func<float, string> CreateValueToStringFunc(float multi)
        {
            return new Func<float, string>(f => Mathf.RoundToInt(f * multi).ToString());
        }

        private void RegisterStudioControls()
        {
            var slider_AngleAnkle = CreateSlider(displaySettings.Ankle_Angle, ctrl => ctrl.AnkleAngle, (ctrl, f) => ctrl.AnkleAngle = f, 0f, 60f);
            var slider_AngleLeg = CreateSlider(displaySettings.Leg_Angle, ctrl => ctrl.LegAngle, (ctrl, f) => ctrl.LegAngle = f, 0f, 60f);
            var slider_Height = CreateSlider(displaySettings.Height, ctrl => ctrl.Height, (ctrl, f) => ctrl.Height = f, 0f, 0.5f);

            StudioAPI.GetOrCreateCurrentStateCategory(displaySettings.Stiletto).AddControls(slider_AngleAnkle, slider_AngleLeg, slider_Height);

            CurrentStateCategorySlider CreateSlider(string name, Func<HeelInfo, float> get, Action<HeelInfo, float> set, float minValue, float maxValue)
            {
                var slider = new CurrentStateCategorySlider(name, (chara) => get(chara.charInfo.GetComponent<HeelInfo>()), minValue, maxValue);
                slider.Value.Subscribe(x =>
                {
                    foreach (var heelInfo in StudioAPI.GetSelectedCharacters().Select(y => y.charInfo.gameObject.GetComponent<HeelInfo>()))
                        set(heelInfo, x);
                });

                return slider;
            }
        }

        public void Dispose()
        {
            StilettoContext.OnHeelInfoUpdate -= OnHeelInfoUpdate;
        }
    }
}
