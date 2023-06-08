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
            slider_AnkleAngle = e.AddControl(new MakerSlider(category, displaySettings.Ankle_Angle, -60f, 60f, 0f, plugin)
            {
                StringToValue = CreateStringToValueFunc(10f),
                ValueToString = CreateValueToStringFunc(10f),
            });
            slider_LegAngle = e.AddControl(new MakerSlider(category, displaySettings.Leg_Angle, -60f, 60f, 0f, plugin)
            {
                StringToValue = CreateStringToValueFunc(10f),
                ValueToString = CreateValueToStringFunc(10f)
            });
            slider_Height = e.AddControl(new MakerSlider(category, displaySettings.Height, -0.5f, 0.5f, 0f, plugin)
            {
                StringToValue = CreateStringToValueFunc(1000f),
                ValueToString = CreateValueToStringFunc(1000f)
            });
            button_HeelSave = e.AddControl(new MakerButton(displaySettings.Save_Heel_Settings, category, plugin));
            button_Reload = e.AddControl(new MakerButton(displaySettings.Reload_Configurations, category, plugin));

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

            button_Reload.OnClick.AddListener(StilettoContext.ReloadConfigurations);
        }

        public void OnHeelInfoUpdate(object _, HeelInfoEventArgs heelInfo)
        {
            if (MakerAPI.InsideMaker && heelInfo != null)
            {
                if (slider_Height != null)
                {
                    slider_Height.Value = heelInfo.CustomHeel.Height;
                    text_heelName.Text = heelInfo.HeelName ?? displaySettings.Default_Heel_Name;
                    slider_AnkleAngle.Value = heelInfo.CustomHeel.AnkleAngle;
                    slider_LegAngle.Value = heelInfo.CustomHeel.LegAngle;
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
