﻿using Illusion.Extensions;
using Manager;
using Stiletto.Models;
using Stiletto.Settings;
using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Stiletto
{
    public class StilettoGameGUI
    {
        private const int ScreenOffset = 20;
        private Rect _windowRect;
        private Rect _screenRect;
        private CharaDisplayData[] _characters = new CharaDisplayData[0];
        private CharaDisplayData selectedCharacter;
        private int selectedIndex = -1;
        private bool _show;

        private DateTime? _lastCharaRefresh;
        private int _selectedTab;
        private int _selectedPoseSide;
        private bool _allCharacters = true;

        private static GUIStyle _gsButtonReset;
        private static GUIStyle _gsInput;
        private static GUILayoutOption _GloSmallButtonWidth;
        private static GUILayoutOption _GloHeight;
        private static GUILayoutOption _GloExpand;

        private DisplaySettings _display;

        public StilettoGameGUI()
        {
            _display = StilettoContext._displaySettingsProvider.Value;
        }

        public bool Show
        {
            get => _show; set
            {
                _show = value;

                if (value)
                {
                    SetWindowSizes();
                }
            }
        }

        public void DisplayWindow(int id, GUIStyle gsButtonReset, GUIStyle gsInput)
        {
            if (!_show) return;

            _gsButtonReset = gsButtonReset;
            _gsInput = gsInput;
            _GloSmallButtonWidth = GUILayout.Width(20);
            _GloHeight = GUILayout.Height(23);
            _GloExpand = GUILayout.ExpandWidth(true);

            var skinBack = GUI.skin;
            _windowRect = GUILayout.Window(id, _windowRect, CreateWindowContent, _display.Stiletto);
            KKAPI.Utilities.IMGUIUtils.EatInputInRect(_windowRect);
        }

        public void CreateWindowContent(int id)
        {
            _display = StilettoContext._displaySettingsProvider.Value;
            var settingTabs = new string[] { _display.Animation_Tab, _display.CustomPose_Tab, _display.CustomHeel_Tab, _display.ShoeWarp_Tab };

            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    var buttonDisplay = _allCharacters ? _display.All_Characters : _display.Current_Characters;
                    GUILayout.Label(buttonDisplay, GUILayout.Width(160));
                    if (GUILayout.Button(_display.Switch_Characters, GUILayout.Width(80)))
                    {
                        _allCharacters = !_allCharacters;
                    }
                }
                GUILayout.EndHorizontal();

                var characters = GetCharacters();
                var updatedHeroines = !CompareCharaData(characters, _characters);

                _characters = characters;

                if (_characters.Length > 0)
                {
                    if (selectedCharacter == null)
                    {
                        SelectCharacter(0);
                    }
                    else if (updatedHeroines)
                    {
                        selectedIndex = _characters.ElementAtOrDefault(selectedIndex) == null ? 0 : selectedIndex;
                        SelectCharacter(selectedIndex);
                    }
                    else
                    {
                        SelectCharacter(selectedIndex);
                    }

                    if (selectedCharacter != null)
                    {
                        CreateCurrentCharacterContent();
                        GUILayout.Space(10);
                    }

                    _selectedTab = GUILayout.Toolbar(_selectedTab, settingTabs, GUILayout.Width(300));

                    GUILayout.BeginVertical(GUILayout.Height(160));
                    {
                        if (_selectedTab == 0 && selectedCharacter != null && selectedCharacter.HeelInfo != null)
                        {
                            CreateAnimationSettingsContent();
                        }

                        if (_selectedTab == 1 && selectedCharacter != null)
                        {
                            CreateCustomPoseSettingsContent();
                        }

                        if (_selectedTab == 2 && selectedCharacter != null)
                        {
                            CreateHeelSettingsContent();
                        }

                        if (_selectedTab == 3 && selectedCharacter != null)
                        {
                            CreateShoeWarpSettingsContent();
                        }
                        GUILayout.Space(10);
                    }
                    GUILayout.EndVertical();

                    if (GUILayout.Button(_display.Reload_Configurations))
                    {
                        StilettoContext.ReloadConfigurations();
                    }
                }
                else
                {
                    SelectCharacter(-1);
                }
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void CreateCurrentCharacterContent()
        {
            GUILayout.BeginVertical();
            {
                CreateDisplayLabel($"{_display.Name}:", selectedCharacter.Name ?? _display.None_Placeholder);
                CreateDisplayLabel($"{_display.Heel}:", selectedCharacter.HeelName ?? _display.None_Placeholder);
                CreateDisplayLabel($"{_display.Anim_Path}:", selectedCharacter.AnimationPath ?? _display.None_Placeholder);
                CreateDisplayLabel($"{_display.Anim_Name}:", selectedCharacter.AnimationName ?? _display.None_Placeholder);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label($"{_display.Total}: {_characters.Length}", GUILayout.Width(70));

                    var lastIndex = _characters.Length - 1;

                    if (GUILayout.Button(_display.Previous, GUILayout.Width(80)))
                    {
                        SelectCharacter(selectedIndex == 0 ? lastIndex : selectedIndex - 1);
                    }
                    GUILayout.Space(10);

                    if (GUILayout.Button(_display.Next, GUILayout.Width(80)))
                    {
                        SelectCharacter(selectedIndex == lastIndex ? 0 : selectedIndex + 1);
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void CreateAnimationSettingsContent()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.BeginVertical(GUILayout.Height(125));
                {
                    GUILayout.Label(_display.AnimationSettings);
                    GUILayout.Space(10);

                    GUILayout.BeginHorizontal();
                    {
                        selectedCharacter.HeelInfo.flags.ACTIVE = GUILayout.Toggle(selectedCharacter.HeelInfo.flags.ACTIVE,
                            $" {_display.Active}", GUILayout.Width(120)
                        );

                        selectedCharacter.HeelInfo.flags.HEIGHT = GUILayout.Toggle(selectedCharacter.HeelInfo.flags.HEIGHT,
                            $" {_display.Height}", GUILayout.Width(120)
                        );
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        selectedCharacter.HeelInfo.flags.TOE_ROLL = GUILayout.Toggle(selectedCharacter.HeelInfo.flags.TOE_ROLL,
                            $" {_display.Toe_Roll}", GUILayout.Width(120)
                        );

                        selectedCharacter.HeelInfo.flags.ANKLE_ROLL = GUILayout.Toggle(selectedCharacter.HeelInfo.flags.ANKLE_ROLL,
                            $" {_display.Ankle_Roll}", GUILayout.Width(120)
                        );
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        selectedCharacter.HeelInfo.flags.KNEE_BEND = GUILayout.Toggle(selectedCharacter.HeelInfo.flags.KNEE_BEND,
                            $" {_display.Knee_Bend}", GUILayout.Width(120)
                        );

                        selectedCharacter.HeelInfo.flags.CUSTOM_POSE = GUILayout.Toggle(selectedCharacter.HeelInfo.flags.CUSTOM_POSE,
                            $" {_display.Custom_Pose}", GUILayout.Width(120)
                        );
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Label(_display.Save_Settings);

                    if (GUILayout.Button(_display.Save_For_All, GUILayout.Width(220)))
                    {
                        StilettoContext.AnimationFlagsProvider.Save(null, null, selectedCharacter.HeelInfo.flags);
                    }
                    if (GUILayout.Button(_display.Save_For_Animation_Group, GUILayout.Width(220)))
                    {
                        StilettoContext.AnimationFlagsProvider.Save(selectedCharacter.AnimationPath, null, selectedCharacter.HeelInfo.flags);
                    }
                    if (GUILayout.Button(_display.Save_For_Animation_Frame, GUILayout.Width(220)))
                    {
                        StilettoContext.AnimationFlagsProvider.Save(selectedCharacter.AnimationPath, selectedCharacter.AnimationName, selectedCharacter.HeelInfo.flags);
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }

        private void CreateCustomPoseSettingsContent()
        {
            var customPoseSideOptions = new string[] { _display.Right_Leg, _display.Left_Leg };

            GUILayout.BeginVertical();
            {
                GUILayout.BeginVertical(GUILayout.Height(125));
                {
                    GUILayout.Label(_display.Custom_Pose_Compatibility);
                    GUILayout.Space(10);

                    selectedCharacter.HeelInfo.CustomPose.WaistAngle = CreateSlider(_display.Waist_Angle, selectedCharacter.HeelInfo.CustomPose.WaistAngle, -90, 90, 1);
                    GUILayout.BeginHorizontal();
                    {
                        selectedCharacter.HeelInfo.CustomPose.Split = !GUILayout.Toggle(!selectedCharacter.HeelInfo.CustomPose.Split,
                            $" {_display.Both_Legs}", GUILayout.Width(120)
                        );
                        if (selectedCharacter.HeelInfo.CustomPose.Split)
                        {
                            GUILayout.Space(10);
                            _selectedPoseSide = GUILayout.SelectionGrid(_selectedPoseSide, customPoseSideOptions, customPoseSideOptions.Length, GUI.skin.toggle, GUILayout.Width(120));
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Label($"{_display.Knee_Bend_Settings}: ", GUILayout.Width(120));

                    if (!selectedCharacter.HeelInfo.CustomPose.Split || _selectedPoseSide == 0)
                    {
                        selectedCharacter.HeelInfo.CustomPose.RightThighAngle = CreateSlider(_display.Thigh_Angle, selectedCharacter.HeelInfo.CustomPose.RightThighAngle, -HeelInfo.AngleRange, HeelInfo.AngleRange, 1);
                        selectedCharacter.HeelInfo.CustomPose.RightLegAngle = CreateSlider(_display.Knee_Angle, selectedCharacter.HeelInfo.CustomPose.RightLegAngle, -HeelInfo.AngleRange, HeelInfo.AngleRange, 1);
                        GUILayout.Space(5);

                        GUILayout.Label($"{_display.Heels_Settings}: ", GUILayout.Width(120));
                        selectedCharacter.HeelInfo.CustomPose.RightAnkleAngle = CreateSlider(_display.Ankle_Angle, selectedCharacter.HeelInfo.CustomPose.RightAnkleAngle, -HeelInfo.AngleRange, HeelInfo.AngleRange, 1);
                    }
                    else
                    {
                        selectedCharacter.HeelInfo.CustomPose.LeftThighAngle = CreateSlider(_display.Thigh_Angle, selectedCharacter.HeelInfo.CustomPose.LeftThighAngle, -HeelInfo.AngleRange, HeelInfo.AngleRange, 1);
                        selectedCharacter.HeelInfo.CustomPose.LeftLegAngle = CreateSlider(_display.Knee_Angle, selectedCharacter.HeelInfo.CustomPose.LeftLegAngle, -HeelInfo.AngleRange, HeelInfo.AngleRange, 1);
                        GUILayout.Space(5);

                        GUILayout.Label($"{_display.Heels_Settings}: ", GUILayout.Width(120));
                        selectedCharacter.HeelInfo.CustomPose.LeftAnkleAngle = CreateSlider(_display.Ankle_Angle, selectedCharacter.HeelInfo.CustomPose.LeftAnkleAngle, -HeelInfo.AngleRange, HeelInfo.AngleRange, 1);
                    }

                    GUILayout.Label(_display.Save_Settings);

                    if (GUILayout.Button(_display.Save_For_All, GUILayout.Width(220)))
                    {
                        StilettoContext.CustomPoseProvider.Save(null, null, selectedCharacter.HeelInfo.CustomPose);
                    }
                    if (GUILayout.Button(_display.Save_For_Animation_Group, GUILayout.Width(220)))
                    {
                        StilettoContext.CustomPoseProvider.Save(selectedCharacter.AnimationPath, null, selectedCharacter.HeelInfo.CustomPose);
                    }
                    if (GUILayout.Button(_display.Save_For_Animation_Frame, GUILayout.Width(220)))
                    {
                        StilettoContext.CustomPoseProvider.Save(selectedCharacter.AnimationPath, selectedCharacter.AnimationName, selectedCharacter.HeelInfo.CustomPose);
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }

        private void CreateHeelSettingsContent()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label(_display.Heels_Settings);
                GUILayout.Space(10);

                GUILayout.BeginVertical(GUILayout.Height(125));
                {
                    GUILayout.BeginVertical();
                    {
                        selectedCharacter.HeelInfo.LegAngle = CreateSlider(_display.Leg_Angle, HeelInfo.GetAngleInRange(selectedCharacter.HeelInfo.LegAngle), -HeelInfo.AngleRange, HeelInfo.AngleRange, 1);
                        selectedCharacter.HeelInfo.AnkleAngle = CreateSlider(_display.Ankle_Angle, HeelInfo.GetAngleInRange(selectedCharacter.HeelInfo.AnkleAngle), -HeelInfo.AngleRange, HeelInfo.AngleRange, 1);
                        selectedCharacter.HeelInfo.ToesAngle = CreateSlider(_display.Toes_Angle, HeelInfo.GetAngleInRange(selectedCharacter.HeelInfo.ToesAngle), -HeelInfo.AngleRange, HeelInfo.AngleRange, 1);
                        selectedCharacter.HeelInfo.Height = CreateSlider(_display.Height, selectedCharacter.HeelInfo.Height, -500, 500, 1000);
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();

                if (GUILayout.Button(_display.Save_Heel_Settings))
                {
                    StilettoContext.CustomHeelProvider.Save(selectedCharacter.HeelInfo.heelName, new CustomHeel(selectedCharacter.HeelInfo));
                }
            }
            GUILayout.EndVertical();
        }

        private void CreateShoeWarpSettingsContent()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.BeginVertical(GUILayout.Height(125));
                {
                    GUILayout.Label(_display.Shoe_Warp_Settings);
                    GUILayout.Space(10);

                    selectedCharacter.HeelInfo.ShoeAngle = CreateSlider(_display.Shoe_Angle, selectedCharacter.HeelInfo.ShoeAngle, -HeelInfo.AngleRange, HeelInfo.AngleRange, 1);
                    GUILayout.Space(5);

                    selectedCharacter.HeelInfo.ShoeScaleX = CreateSlider(_display.Shoe_ScaleX, selectedCharacter.HeelInfo.ShoeScaleX, 100, 10000, 100, resetValue: 100);
                    selectedCharacter.HeelInfo.ShoeScaleY = CreateSlider(_display.Shoe_ScaleY, selectedCharacter.HeelInfo.ShoeScaleY, 100, 10000, 100, resetValue: 100);
                    selectedCharacter.HeelInfo.ShoeScaleZ = CreateSlider(_display.Shoe_ScaleZ, selectedCharacter.HeelInfo.ShoeScaleZ, 100, 10000, 100, resetValue: 100);
                    GUILayout.Space(5);


                    selectedCharacter.HeelInfo.ShoeTranslateY = CreateSlider(_display.Shoe_TranslateY, selectedCharacter.HeelInfo.ShoeTranslateY, -500, 500, 1000);
                    selectedCharacter.HeelInfo.ShoeTranslateZ = CreateSlider(_display.Shoe_TranslateZ, selectedCharacter.HeelInfo.ShoeTranslateZ, -500, 500, 1000);
                    GUILayout.Space(5);

                    selectedCharacter.HeelInfo.ShoeShearY = CreateSlider(_display.Shoe_ShearY, selectedCharacter.HeelInfo.ShoeShearY, -HeelInfo.AngleRange, HeelInfo.AngleRange, 1);
                    selectedCharacter.HeelInfo.ShoeShearZ = CreateSlider(_display.Shoe_ShearZ, selectedCharacter.HeelInfo.ShoeShearZ, -HeelInfo.AngleRange, HeelInfo.AngleRange, 1);
                }
                GUILayout.EndVertical();

                if (GUILayout.Button(_display.Save_Heel_Settings))
                {
                    StilettoContext.CustomHeelProvider.Save(selectedCharacter.HeelInfo.heelName, new CustomHeel(selectedCharacter.HeelInfo));
                }
            }
            GUILayout.EndVertical();
        }

        private float CreateSlider(string display, float value, float min, float max, float multiplier, float resetValue = 0)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(display, GUILayout.Width(165), _GloHeight);

                value = GUILayout.HorizontalSlider(value * multiplier, min, max, _gsButtonReset, _gsButtonReset, _GloExpand, _GloHeight);
                float.TryParse(GUILayout.TextField(value.ToString("F0", CultureInfo.InvariantCulture), _gsInput, GUILayout.Width(43), _GloHeight),
                               out value);

                if (GUILayout.Button("-", _gsButtonReset, GUILayout.Width(20), _GloHeight)) value -= 1;
                if (GUILayout.Button("+", _gsButtonReset, GUILayout.Width(20), _GloHeight)) value += 1;

                if (GUILayout.Button("0", _gsButtonReset, _GloSmallButtonWidth, _GloHeight)) value = resetValue;
            }
            GUILayout.EndHorizontal();

            value = Mathf.Round(value); 
            return value / multiplier;
        }

        private float CreateNumberTextField(string display, float value, float multiplier)
        {
            string textValue;
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(display, GUILayout.Width(70));
                textValue = GUILayout.TextField(Math.Round(value * multiplier).ToString(), GUILayout.Width(40));
            }
            GUILayout.EndHorizontal();

            if (float.TryParse(textValue, out var floatValue))
            {
                return floatValue / multiplier;
            }
            return 0;
        }

        private void CreateDisplayLabel(string display, string label)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(display, GUILayout.Width(80));
                GUILayout.Label(label);
            }
            GUILayout.EndHorizontal();
        }

        private void SelectCharacter(int index)
        {
            selectedCharacter = _characters.ElementAtOrDefault(index);
            selectedIndex = index;
        }

        private void SetWindowSizes()
        {
            int w = Screen.width, h = Screen.height;
            _screenRect = new Rect(ScreenOffset, ScreenOffset, w - ScreenOffset * 2, h - ScreenOffset * 2);

            var windowHeight = 450;
            var windowWidth = 450;

            _windowRect = new Rect(_screenRect.xMin, _screenRect.yMax - windowHeight, windowWidth, windowHeight);
        }

        private CharaDisplayData[] GetCharacters()
        {
            if (_allCharacters) 
            {
                return GetAllCharacters();
            }
            return GetCurrentCharacters();
        }

        private CharaDisplayData[] GetAllCharacters() 
        {
            return StilettoContext.HeelInfos.Where(x => x.HasAnimation).Select(x => new CharaDisplayData(x)).ToArray();
        }

        private CharaDisplayData[] GetCurrentCharacters()
        {
            if (_lastCharaRefresh != null && DateTime.Now - _lastCharaRefresh < TimeSpan.FromSeconds(2))
            {
                return _characters;
            }
            else
            {
                _lastCharaRefresh = DateTime.Now;
            }


            var result = UnityEngine.Object.FindObjectOfType<TalkScene>()?.targetHeroine;
            if (result != null)
            {
                return new[] { new CharaDisplayData(result) };
            }

            var hflags = UnityEngine.Object.FindObjectOfType<HFlag>();
            var hHeroines = hflags?.lstHeroine;
            if (hHeroines != null && hHeroines.Count > 0)
            {
                var characters = hHeroines.Select(x => new CharaDisplayData(x)).ToList();
                characters.Push(new CharaDisplayData(hflags.player));
                return characters.ToArray();
            }
#if KKS
            var actScene = ActionScene.instance;

            if (Game.initialized &&
                actScene != null &&
                actScene.AdvScene != null)
            {
                var advScene = actScene.AdvScene;
#else
            if (Game.IsInstance() &&
                Game.Instance.actScene != null &&
                Game.Instance.actScene.AdvScene != null)
            {
                var advScene = Game.Instance.actScene.AdvScene;
#endif
                if (advScene.Scenario?.currentHeroine != null)
                {
                    return new CharaDisplayData[] {
                        new CharaDisplayData(advScene.Scenario.currentHeroine),
                        new CharaDisplayData(advScene.Scenario.player)
                    };
                }

                if (advScene.nowScene is TalkScene s && s.targetHeroine != null)
                {
                    return new CharaDisplayData[] {
                        new CharaDisplayData(s.targetHeroine),
#if KK
                        new CharaDisplayData(Game.Instance.Player)
#else
                        new CharaDisplayData(Game.Player)
#endif
                    };
                }
            }

            return StilettoContext.HeelInfos.Select(x => new CharaDisplayData(x)).ToArray();
        }

        private bool CompareCharaData(CharaDisplayData[] first, CharaDisplayData[] second)
        {
            if (first.Length != second.Length)
                return false;

            for (var i = 0; i < first.Length; i++)
            {
                if (first[i].ChaControl != second[i].ChaControl)
                    return false;
            }

            return true;
        }
    }
}
