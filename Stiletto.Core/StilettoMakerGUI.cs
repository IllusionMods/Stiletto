using BepInEx.Logging;

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
        private MakerText text_disclaimer;
        private MakerText text_heelName;
        private MakerSlider slider_AnkleAngle;
        private MakerSlider slider_ToesAngle;
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
        private MakerButton button_GameGui;

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

            text_disclaimer = e.AddControl(new MakerText(displaySettings.Disclaimer, category, plugin));
            e.AddControl(new MakerSeparator(category, plugin));

            e.AddControl(new MakerText(displaySettings.Heels_Settings, category, plugin));
            text_heelName = e.AddControl(new MakerText(displaySettings.Default_Heel_Name, category, plugin));
            slider_LegAngle = e.AddControl(new MakerSlider(category, displaySettings.Leg_Angle, -60f, 60f, 0f, plugin)
            {
                StringToValue = CreateStringToValueFunc(1f),
                ValueToString = CreateValueToStringFunc(1f)
            });
            slider_AnkleAngle = e.AddControl(new MakerSlider(category, displaySettings.Ankle_Angle, -60f, 60f, 0f, plugin)
            {
                StringToValue = CreateStringToValueFunc(1f),
                ValueToString = CreateValueToStringFunc(1f),
            }); 
            slider_ToesAngle = e.AddControl(new MakerSlider(category, displaySettings.Toes_Angle, -60f, 60f, 0f, plugin)
            {
                StringToValue = CreateStringToValueFunc(1f),
                ValueToString = CreateValueToStringFunc(1f),
            });
            slider_Height = e.AddControl(new MakerSlider(category, displaySettings.Height, -0.5f, 0.5f, 0f, plugin)
            {
                StringToValue = CreateStringToValueFunc(1000f),
                ValueToString = CreateValueToStringFunc(1000f)
            });

            e.AddControl(new MakerSeparator(category, plugin));
            text_shoeWarp = e.AddControl(new MakerText(displaySettings.Shoe_Warp, category, plugin));
            slider_ShoeAngle = e.AddControl(new MakerSlider(category, displaySettings.Shoe_Angle, -60f, 60f, 0f, plugin)
            {
                StringToValue = CreateStringToValueFunc(1f),
                ValueToString = CreateValueToStringFunc(1f)
            });
            slider_ShoeScaleX = e.AddControl(new MakerSlider(category, displaySettings.Shoe_ScaleX, 0.1f, 10f, 1f, plugin)
            {
                StringToValue = CreateStringToValueFunc(100f),
                ValueToString = CreateValueToStringFunc(100f)
            });
            slider_ShoeScaleY = e.AddControl(new MakerSlider(category, displaySettings.Shoe_ScaleY, 0.1f, 10f, 1f, plugin)
            {
                StringToValue = CreateStringToValueFunc(100f),
                ValueToString = CreateValueToStringFunc(100f)
            });
            slider_ShoeScaleZ = e.AddControl(new MakerSlider(category, displaySettings.Shoe_ScaleZ, 0.1f, 10f, 1f, plugin)
            {
                StringToValue = CreateStringToValueFunc(100f),
                ValueToString = CreateValueToStringFunc(100f)
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
                StringToValue = CreateStringToValueFunc(1f),
                ValueToString = CreateValueToStringFunc(1f)
            });
            slider_ShoeShearZ = e.AddControl(new MakerSlider(category, displaySettings.Shoe_ShearZ, -60f, 60f, 0f, plugin)
            {
                StringToValue = CreateStringToValueFunc(1f),
                ValueToString = CreateValueToStringFunc(1f)
            });


            button_HeelSave = e.AddControl(new MakerButton(displaySettings.Save_Heel_Settings, category, plugin));
            // TODO: Maybe update the button label when the shortcut changes?
            string suffix_GameGui = "";
            if (plugin._showWindowKey != null)
            {
                suffix_GameGui = " (" + plugin._showWindowKey?.Value.Serialize() + ")";
            }
            button_GameGui = e.AddControl(new MakerButton(displaySettings.Toggle_Game_Gui + suffix_GameGui, category, plugin));

            slider_AnkleAngle.ValueChanged.Subscribe(value =>
                MakerHeelInfoProcess(heel => heel.SafeProc(x => x.AnkleAngle = HeelInfo.AngleDisplayToValue(value)))
            );

            slider_ToesAngle.ValueChanged.Subscribe(value =>
                MakerHeelInfoProcess(heel => heel.SafeProc(x => x.ToesAngle = HeelInfo.AngleDisplayToValue(value)))
            );

            slider_LegAngle.ValueChanged.Subscribe(value =>
                MakerHeelInfoProcess(heel => heel.SafeProc(x => x.LegAngle = HeelInfo.AngleDisplayToValue(value)))
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

            button_GameGui.OnClick.AddListener(plugin.ToggleWindow);
        }

        public void OnHeelInfoUpdate(object _, HeelInfoEventArgs heelInfo)
        {
            try
            {
                if (MakerAPI.InsideAndLoaded && heelInfo != null)
                {
                    if (slider_Height != null)
                    {
                        slider_Height.Value = heelInfo.CustomHeel.Height;
                        text_heelName.Text = displaySettings.Current_Shoes + heelInfo.HeelName ?? displaySettings.Default_Heel_Name;
                        slider_AnkleAngle.Value = HeelInfo.AngleValueToDisplay(heelInfo.CustomHeel.AnkleAngle);
                        slider_ToesAngle.Value = HeelInfo.AngleValueToDisplay(heelInfo.CustomHeel.ToesAngle);
                        slider_LegAngle.Value = HeelInfo.AngleValueToDisplay(heelInfo.CustomHeel.LegAngle);

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
            catch (Exception e)
            {
                Stiletto.Logger.Log(LogLevel.Error, $"OnHeelInfoUpdate: Error={e.Message}");
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
            var slider_AngleAnkle = CreateSlider(displaySettings.Ankle_Angle, ctrl => ctrl.AnkleAngle, (ctrl, f) => ctrl.AnkleAngle = f, -60f, 60f);
            var slider_AngleToes = CreateSlider(displaySettings.Toes_Angle, ctrl => ctrl.ToesAngle, (ctrl, f) => ctrl.ToesAngle = f, -60f, 60f);
            var slider_AngleLeg = CreateSlider(displaySettings.Leg_Angle, ctrl => ctrl.LegAngle, (ctrl, f) => ctrl.LegAngle = f, -60f, 60f);
            var slider_Height = CreateSlider(displaySettings.Height, ctrl => ctrl.Height, (ctrl, f) => ctrl.Height = f, -0.5f, 0.5f);

            StudioAPI.GetOrCreateCurrentStateCategory(displaySettings.Stiletto).AddControls(slider_AngleAnkle, slider_AngleToes, slider_AngleLeg, slider_Height);

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
