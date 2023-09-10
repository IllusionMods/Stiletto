using Illusion.Extensions;
using Manager;
using Stiletto.Models;
using Stiletto.Settings;
using System;
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

        public void DisplayWindow(int id)
        {
            if (!_show) return;

            var skinBack = GUI.skin;
            _windowRect = GUILayout.Window(id, _windowRect, CreateWindowContent, _display.Stiletto);
        }

        public void CreateWindowContent(int id)
        {
            _display = StilettoContext._displaySettingsProvider.Value;
            var settingTabs = new string[] { _display.Animation_Tab, _display.CustomPose_Tab, _display.CustomHeel_Tab };

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

                    _selectedTab = GUILayout.Toolbar(_selectedTab, settingTabs, GUILayout.Width(250));

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
                }
                GUILayout.EndVertical();
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(_display.Save_For_All, GUILayout.Width(115)))
                    {
                        StilettoContext.AnimationFlagsProvider.Save(null, null, selectedCharacter.HeelInfo.flags);
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(_display.Save_For_Animation_Group, GUILayout.Width(115)))
                    {
                        StilettoContext.AnimationFlagsProvider.Save(selectedCharacter.AnimationPath, null, selectedCharacter.HeelInfo.flags);
                    }
                    GUILayout.Space(10);
                    if (GUILayout.Button(_display.Save_For_Animation_Frame, GUILayout.Width(115)))
                    {
                        StilettoContext.AnimationFlagsProvider.Save(selectedCharacter.AnimationPath, selectedCharacter.AnimationName, selectedCharacter.HeelInfo.flags);
                    }
                }
                GUILayout.EndHorizontal();
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

                    GUILayout.BeginHorizontal();
                    {
                        selectedCharacter.HeelInfo.CustomPose.WaistAngle = CreateNumberTextField(_display.Waist_Angle, selectedCharacter.HeelInfo.CustomPose.WaistAngle, 10);
                        GUILayout.Space(10);
                        selectedCharacter.HeelInfo.CustomPose.Split = !GUILayout.Toggle(!selectedCharacter.HeelInfo.CustomPose.Split,
                            $" {_display.Both_Legs}", GUILayout.Width(120)
                        );
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label($"{_display.Knee_Bend_Settings}: ", GUILayout.Width(120));
                        if (selectedCharacter.HeelInfo.CustomPose.Split)
                        {
                            GUILayout.Space(10);
                            _selectedPoseSide = GUILayout.SelectionGrid(_selectedPoseSide, customPoseSideOptions, customPoseSideOptions.Length, GUI.skin.toggle, GUILayout.Width(120));
                        }
                    }
                    GUILayout.EndHorizontal();

                    if (!selectedCharacter.HeelInfo.CustomPose.Split || _selectedPoseSide == 0)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            selectedCharacter.HeelInfo.CustomPose.RightThighAngle = CreateNumberTextField(_display.Thigh_Angle, selectedCharacter.HeelInfo.CustomPose.RightThighAngle, 10);
                            GUILayout.Space(10);
                            selectedCharacter.HeelInfo.CustomPose.RightLegAngle = CreateNumberTextField(_display.Leg_Angle, selectedCharacter.HeelInfo.CustomPose.RightLegAngle, 10);
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        {
                            selectedCharacter.HeelInfo.CustomPose.RightAnkleAngle = CreateNumberTextField(_display.Ankle_Angle, selectedCharacter.HeelInfo.CustomPose.RightAnkleAngle, 10);
                        }
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        {
                            selectedCharacter.HeelInfo.CustomPose.LeftThighAngle = CreateNumberTextField(_display.Thigh_Angle, selectedCharacter.HeelInfo.CustomPose.LeftThighAngle, 10);
                            GUILayout.Space(10);
                            selectedCharacter.HeelInfo.CustomPose.LeftLegAngle = CreateNumberTextField(_display.Leg_Angle, selectedCharacter.HeelInfo.CustomPose.LeftLegAngle, 10);
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        {
                            selectedCharacter.HeelInfo.CustomPose.LeftAnkleAngle = CreateNumberTextField(_display.Ankle_Angle, selectedCharacter.HeelInfo.CustomPose.LeftAnkleAngle, 10);
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndVertical();

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(_display.Save_For_All, GUILayout.Width(115)))
                    {
                        StilettoContext.CustomPoseProvider.Save(null, null, selectedCharacter.HeelInfo.CustomPose);
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(_display.Save_For_Animation_Group, GUILayout.Width(115)))
                    {
                        StilettoContext.CustomPoseProvider.Save(selectedCharacter.AnimationPath, null, selectedCharacter.HeelInfo.CustomPose);
                    }
                    GUILayout.Space(10);
                    if (GUILayout.Button(_display.Save_For_Animation_Frame, GUILayout.Width(115)))
                    {
                        StilettoContext.CustomPoseProvider.Save(selectedCharacter.AnimationPath, selectedCharacter.AnimationName, selectedCharacter.HeelInfo.CustomPose);
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void CreateHeelSettingsContent()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.BeginVertical(GUILayout.Height(125));
                {
                    GUILayout.Label(_display.Heel_Settings);

                    GUILayout.BeginHorizontal();
                    {
                        selectedCharacter.HeelInfo.AnkleAngle = CreateNumberTextField(_display.Ankle_Angle, selectedCharacter.HeelInfo.AnkleAngle, 10);
                        selectedCharacter.HeelInfo.LegAngle = CreateNumberTextField(_display.Leg_Angle, selectedCharacter.HeelInfo.LegAngle, 10);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        selectedCharacter.HeelInfo.Height = CreateNumberTextField(_display.Height, selectedCharacter.HeelInfo.Height, 1000);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

                if (GUILayout.Button(_display.Save_Heel_Settings))
                {
                    StilettoContext.CustomHeelProvider.Save(selectedCharacter.HeelInfo.heelName, new CustomHeel(selectedCharacter.HeelInfo));
                }
            }
            GUILayout.EndVertical();
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

            var windowHeight = 400;
            var windowWidth = 270;

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
