using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Studio;
using KKAPI.Studio.UI;
using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Stiletto
{
    public static class StilettoGui
    {
        private static MakerSlider slider_AngleAnkle;
        private static MakerSlider slider_AngleLeg;
        private static MakerSlider slider_Height;

        public static void Init(Stiletto plugin)
        {
            if(StudioAPI.InsideStudio)
                RegisterStudioControls();
            else
                MakerAPI.RegisterCustomSubCategories += (sender, e) => RegisterMakerControls(plugin, e);
        }

        private static void RegisterMakerControls(Stiletto plugin, RegisterSubCategoriesEvent e)
        {
            // Doesn't apply to male characters
            if(MakerAPI.GetMakerSex() == 0) return;

            var shoesCategory = MakerConstants.Clothes.OuterShoes;
            var category = new MakerCategory(shoesCategory.CategoryName, "stiletto", shoesCategory.Position + 1, "Stiletto");
            e.AddSubCategory(category);

            slider_AngleAnkle = e.AddControl(new MakerSlider(category, "AngleAnkle", 0f, 60f, 0f, plugin) { StringToValue = CreateStringToValueFunc(10f), ValueToString = CreateValueToStringFunc(10f), });
            slider_AngleLeg = e.AddControl(new MakerSlider(category, "AngleLeg", 0f, 60f, 0f, plugin) { StringToValue = CreateStringToValueFunc(10f), ValueToString = CreateValueToStringFunc(10f) });
            slider_Height = e.AddControl(new MakerSlider(category, "Height", 0f, 0.5f, 0f, plugin) { StringToValue = CreateStringToValueFunc(1000f), ValueToString = CreateValueToStringFunc(1000f) });

            slider_AngleAnkle.ValueChanged.Subscribe(x => MakerAPI.GetCharacterControl().GetComponent<HeelInfo>().SafeProc(y => y.AnkleAnglef = x));
            slider_AngleLeg.ValueChanged.Subscribe(x => MakerAPI.GetCharacterControl().GetComponent<HeelInfo>().SafeProc(y => y.LegAnglef = x));
            slider_Height.ValueChanged.Subscribe(x => MakerAPI.GetCharacterControl().GetComponent<HeelInfo>().SafeProc(y => y.Heightf = x));
        }

        public static void UpdateMakerValues(HeelInfo heelInfo)
        {
            if(slider_AngleAnkle != null)
            {
                slider_AngleAnkle.Value = heelInfo.AnkleAnglef;
                slider_AngleLeg.Value = heelInfo.LegAnglef;
                slider_Height.Value = heelInfo.Heightf;
            }
        }

        public static Func<string, float> CreateStringToValueFunc(float multi)
        {
            return new Func<string, float>(txt => float.Parse(txt) / multi);
        }

        public static Func<float, string> CreateValueToStringFunc(float multi)
        {
            return new Func<float, string>(f => Mathf.RoundToInt(f * multi).ToString());
        }

        private static void RegisterStudioControls()
        {
            var slider_AngleAnkle = CreateSlider("AngleAnkle", ctrl => ctrl.AnkleAnglef, (ctrl, f) => ctrl.AnkleAnglef = f, 0f, 60f);
            var slider_AngleLeg = CreateSlider("AngleLeg", ctrl => ctrl.LegAnglef, (ctrl, f) => ctrl.LegAnglef = f, 0f, 60f);
            var slider_Height = CreateSlider("Height", ctrl => ctrl.Heightf, (ctrl, f) => ctrl.Heightf = f, 0f, 0.5f);

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
