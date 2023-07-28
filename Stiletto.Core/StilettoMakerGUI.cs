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

        private MakerText text_shoeWarp;
        private MakerSlider slider_ShoeAngle;
        private MakerSlider slider_ShoeScaleX;
        private MakerSlider slider_ShoeScaleY;
        private MakerSlider slider_ShoeScaleZ;
        private MakerSlider slider_ShoeTranslateY;
        private MakerSlider slider_ShoeTranslateZ;
        private MakerSlider slider_ShoeShearY;
        private MakerSlider slider_ShoeShearZ;

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

            text_shoeWarp = e.AddControl(new MakerText(displaySettings.Shoe_Warp, category, plugin));
            slider_ShoeAngle = e.AddControl(new MakerSlider(category, displaySettings.Shoe_Angle, -60f, 60f, 0f, plugin)
            {
                StringToValue = CreateStringToValueFunc(10f),
                ValueToString = CreateValueToStringFunc(10f)
            });
            slider_ShoeScaleX = e.AddControl(new MakerSlider(category, displaySettings.Shoe_ScaleX, 0.1f, 10f, 1f, plugin)
            {
                StringToValue = CreateStringToValueFunc(1000f),
                ValueToString = CreateValueToStringFunc(1000f)
            });
            slider_ShoeScaleY = e.AddControl(new MakerSlider(category, displaySettings.Shoe_ScaleY, 0.1f, 10f, 1f, plugin)
            {
                StringToValue = CreateStringToValueFunc(1000f),
                ValueToString = CreateValueToStringFunc(1000f)
            });
            slider_ShoeScaleZ = e.AddControl(new MakerSlider(category, displaySettings.Shoe_ScaleZ, 0.1f, 10f, 1f, plugin)
            {
                StringToValue = CreateStringToValueFunc(1000f),
                ValueToString = CreateValueToStringFunc(1000f)
            });
            slider_ShoeTranslateY = e.AddControl(new MakerSlider(category, displaySettings.Shoe_TranslateY, -0.5f, 0.5f, 0f, plugin)
            {
                StringToValue = CreateStringToValueFunc(1000f),
                ValueToString = CreateValueToStringFunc(1000f)
            });
            slider_ShoeTranslateZ = e.AddControl(new MakerSlider(category, displaySettings.Shoe_TranslateZ, -0.5f, 0.5f, 0f, plugin)
            {
                StringToValue = CreateStringToValueFunc(1000f),
                ValueToString = CreateValueToStringFunc(1000f)
            });
            slider_ShoeShearY = e.AddControl(new MakerSlider(category, displaySettings.Shoe_ShearY, -60f, 60f, 0f, plugin)
            {
                StringToValue = CreateStringToValueFunc(10f),
                ValueToString = CreateValueToStringFunc(10f)
            });
            slider_ShoeShearZ = e.AddControl(new MakerSlider(category, displaySettings.Shoe_ShearZ, -60f, 60f, 0f, plugin)
            {
                StringToValue = CreateStringToValueFunc(10f),
                ValueToString = CreateValueToStringFunc(10f)
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

            slider_ShoeAngle.ValueChanged.Subscribe(value =>
                MakerHeelInfoProcess(heel => heel.SafeProc(x => x.ShoeAngle = value))
            );

            slider_ShoeScaleX.ValueChanged.Subscribe(value =>
                MakerHeelInfoProcess(heel => heel.SafeProc(x => x.ShoeScaleX = value))
            );

            slider_ShoeScaleY.ValueChanged.Subscribe(value =>
                MakerHeelInfoProcess(heel => heel.SafeProc(x => x.ShoeScaleY = value))
            );

            slider_ShoeScaleZ.ValueChanged.Subscribe(value =>
                MakerHeelInfoProcess(heel => heel.SafeProc(x => x.ShoeScaleZ = value))
            );

            slider_ShoeTranslateY.ValueChanged.Subscribe(value =>
                MakerHeelInfoProcess(heel => heel.SafeProc(x => x.ShoeTranslateY = value))
            );

            slider_ShoeTranslateZ.ValueChanged.Subscribe(value =>
                MakerHeelInfoProcess(heel => heel.SafeProc(x => x.ShoeTranslateZ = value))
            );

            slider_ShoeShearY.ValueChanged.Subscribe(value =>
                MakerHeelInfoProcess(heel => heel.SafeProc(x => x.ShoeShearY = value))
            );

            slider_ShoeShearZ.ValueChanged.Subscribe(value =>
                MakerHeelInfoProcess(heel => heel.SafeProc(x => x.ShoeShearZ = value))
            );

            button_HeelSave.OnClick.AddListener(() =>
                MakerHeelInfoProcess(heel => StilettoContext.CustomHeelProvider.Save(heel.heelName, new CustomHeel(heel))
            ));

            button_Reload.OnClick.AddListener(StilettoContext.ReloadConfigurations);
        }

        public void OnHeelInfoUpdate(object _, HeelInfoEventArgs heelInfo)
        {
            try
            {
                // When using Maker for a second time in a game session there is a
                // [Error  :Null Checks] Swallowing exception to prevent game crash!
                // when accessing heelInfo.CustomHeel.Height. heelInfo is disposed of
                // on the first call before RegisterMakerControls on this occasion.
                // OnHeelInfoUpdate event handler does not seem to be unloaded on 
                // the Dispose method.
                if (MakerAPI.InsideMaker && heelInfo != null)
                {
                    if (slider_Height != null)
                    {
                        slider_Height.Value = heelInfo.CustomHeel.Height;
                        text_heelName.Text = heelInfo.HeelName ?? displaySettings.Default_Heel_Name;
                        slider_AnkleAngle.Value = heelInfo.CustomHeel.AnkleAngle;
                        slider_LegAngle.Value = heelInfo.CustomHeel.LegAngle;

                        slider_ShoeAngle.Value = heelInfo.CustomHeel.ShoeAngle;
                        slider_ShoeScaleX.Value = heelInfo.CustomHeel.ShoeScaleX;
                        slider_ShoeScaleY.Value = heelInfo.CustomHeel.ShoeScaleY;
                        slider_ShoeScaleZ.Value = heelInfo.CustomHeel.ShoeScaleZ;
                        slider_ShoeTranslateY.Value = heelInfo.CustomHeel.ShoeTranslateY;
                        slider_ShoeTranslateZ.Value = heelInfo.CustomHeel.ShoeTranslateZ;
                        slider_ShoeShearY.Value = heelInfo.CustomHeel.ShoeShearY;
                        slider_ShoeShearZ.Value = heelInfo.CustomHeel.ShoeShearZ;
                    }
                }
            }
            catch
            {
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
