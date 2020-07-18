using Illusion.Extensions;
using Manager;
using Stiletto.Models;
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
        private CharaDisplayData[] _currentCharacters = new CharaDisplayData[0];
        private CharaDisplayData selectedCharacter;
        private int selectedIndex = -1;
        private bool _show;
        private DateTime? _lastCharaRefersh;
        private int _selectedTab;

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
            _windowRect = GUILayout.Window(id, _windowRect, CreateWindowContent, "Stiletto");
        }

        public void CreateWindowContent(int id)
        {
            GUILayout.BeginVertical();
            {
                var currentCharacters = GetCurrentVisibleCharacters();
                var updatedHeroines = !CompareCharaData(currentCharacters, _currentCharacters);

                _currentCharacters = currentCharacters;

                if (_currentCharacters.Length > 0)
                {
                    if (selectedCharacter == null)
                    {
                        SelectCharacter(0);
                    }
                    else if (updatedHeroines)
                    {
                        selectedIndex = _currentCharacters.ElementAtOrDefault(selectedIndex) == null ? 0 : selectedIndex;
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

                    _selectedTab = GUILayout.Toolbar(_selectedTab, new string[] {
                        "Settings", "Advanced", "Heel"
                    }, GUILayout.Width(250));

                    GUILayout.BeginVertical(GUILayout.Height(120));
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
                    
                    if (GUILayout.Button("Reload Configuration"))
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
                GUILayout.Label("Current Character");
                CreateDisplayLabel("Name:", selectedCharacter.Name);
                CreateDisplayLabel("Heel:", selectedCharacter.HeelName);
                CreateDisplayLabel("Anim Path:", selectedCharacter.AnimationPath);
                CreateDisplayLabel("Anim Name:", selectedCharacter.AnimationName);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label($"Total: {_currentCharacters.Length}", GUILayout.Width(70));

                    var lastIndex = _currentCharacters.Length - 1;

                    if (GUILayout.Button("Privous", GUILayout.Width(80)))
                    {
                        SelectCharacter(selectedIndex == 0 ? lastIndex : selectedIndex - 1);
                    }
                    GUILayout.Space(10);

                    if (GUILayout.Button("Next", GUILayout.Width(80)))
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
                GUILayout.Label("Animation Settings");

                GUILayout.BeginHorizontal();
                {
                    selectedCharacter.HeelInfo.flags.ACTIVE = GUILayout.Toggle(selectedCharacter.HeelInfo.flags.ACTIVE, " Active", GUILayout.Width(120));
                    selectedCharacter.HeelInfo.flags.CUSTOM_POSE = GUILayout.Toggle(selectedCharacter.HeelInfo.flags.CUSTOM_POSE, " Custom Pose", GUILayout.Width(120));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    selectedCharacter.HeelInfo.flags.TOE_ROLL = GUILayout.Toggle(selectedCharacter.HeelInfo.flags.TOE_ROLL, " Toe Roll", GUILayout.Width(120));
                    selectedCharacter.HeelInfo.flags.ANKLE_ROLL = GUILayout.Toggle(selectedCharacter.HeelInfo.flags.ANKLE_ROLL, " Ankle Roll", GUILayout.Width(120));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    selectedCharacter.HeelInfo.flags.HEIGHT = GUILayout.Toggle(selectedCharacter.HeelInfo.flags.HEIGHT, " Height", GUILayout.Width(120));
                    selectedCharacter.HeelInfo.flags.KNEE_BEND = GUILayout.Toggle(selectedCharacter.HeelInfo.flags.KNEE_BEND, " Keen Bend", GUILayout.Width(120));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Save PathOnly", GUILayout.Width(115)))
                    {
                        StilettoContext.AnimationFlagsProvider.Save(selectedCharacter.AnimationPath, null, selectedCharacter.HeelInfo.flags);
                    }
                    GUILayout.Space(10);
                    if (GUILayout.Button("Save Path+Name", GUILayout.Width(115)))
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
            GUILayout.BeginVertical();
            {
                GUILayout.Label("Custom Pose (Compatibility)");

                GUILayout.BeginHorizontal();
                {
                    selectedCharacter.HeelInfo.CustomPose.ThighAngle = CreateNumberTextField("Thigh Angle", selectedCharacter.HeelInfo.CustomPose.ThighAngle, 10);
                    GUILayout.Space(10);
                    selectedCharacter.HeelInfo.CustomPose.LegAngle = CreateNumberTextField("Leg Angle", selectedCharacter.HeelInfo.CustomPose.LegAngle, 10);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    selectedCharacter.HeelInfo.CustomPose.WaistAngle = CreateNumberTextField("Waist Angle", selectedCharacter.HeelInfo.CustomPose.WaistAngle, 10);
                    GUILayout.Space(10);
                    selectedCharacter.HeelInfo.CustomPose.AnkleAngle = CreateNumberTextField("Ankle Angle", selectedCharacter.HeelInfo.CustomPose.AnkleAngle, 10);
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Save Custom Pose"))
                {
                    StilettoContext.CustomPoseProvider.Save(selectedCharacter.AnimationPath, selectedCharacter.HeelInfo.CustomPose);
                }
            }
            GUILayout.EndVertical();
        }

        private void CreateHeelSettingsContent()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label("Heel Configuration");

                GUILayout.BeginHorizontal();
                {
                    selectedCharacter.HeelInfo.AnkleAngle = CreateNumberTextField("Ankle Angle", selectedCharacter.HeelInfo.AnkleAngle, 10);
                    selectedCharacter.HeelInfo.LegAngle = CreateNumberTextField("Leg Angle", selectedCharacter.HeelInfo.LegAngle, 10);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    selectedCharacter.HeelInfo.Height = CreateNumberTextField("Height", selectedCharacter.HeelInfo.Height, 1000);
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Save Heel Settings"))
                {
                    StilettoContext.CustomHeelProvider.Save(selectedCharacter.HeelInfo.heelName, new CustomHeel(selectedCharacter.HeelInfo));
                }
            }
            GUILayout.EndVertical();
        }

        private float CreateNumberTextField(string display, float value, float multiplier)
        {
            var textValue = "0";
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(display, GUILayout.Width(70));
                textValue = GUILayout.TextField(Math.Round((value * multiplier)).ToString(), GUILayout.Width(40));
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
            selectedCharacter = _currentCharacters.ElementAtOrDefault(index);
            selectedIndex = index;
        }

        private void SetWindowSizes()
        {
            int w = Screen.width, h = Screen.height;
            _screenRect = new Rect(ScreenOffset, ScreenOffset, w - ScreenOffset * 2, h - ScreenOffset * 2);

            var windowHeight = 360;
            var windowWidth = 270;

            _windowRect = new Rect(_screenRect.xMin, _screenRect.yMax - windowHeight, windowWidth, windowHeight);
        }

        private CharaDisplayData[] GetCurrentVisibleCharacters()
        {
            if (_lastCharaRefersh != null && DateTime.Now - _lastCharaRefersh < TimeSpan.FromSeconds(2))
            {
                return _currentCharacters;
            }
            else
            {
                _lastCharaRefersh = DateTime.Now;
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

            if (Game.IsInstance() &&
                Game.Instance.actScene != null &&
                Game.Instance.actScene.AdvScene != null)
            {
                var advScene = Game.Instance.actScene.AdvScene;
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
                        new CharaDisplayData(Game.Instance.Player)
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
