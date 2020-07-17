using Illusion.Extensions;
using Manager;
using Stiletto.Configurations;
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
                    // GUILayout.Space(10);
                    // CreateHeelSettingsContent();

                    if (selectedCharacter != null && selectedCharacter.HeelInfo != null)
                    {
                        CreateAnimationSettingsContent();
                        GUILayout.Space(10);
                    }

                    if (GUILayout.Button("Reload Configuration"))
                    {
                        HeelFlagsProvider.ReloadHeelFlags();
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

        public void UpdateCharacterList() 
        { 
            
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
                    selectedCharacter.HeelInfo.flags.ACTIVE = GUILayout.Toggle(selectedCharacter.HeelInfo.flags.ACTIVE, " Active");
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
                        HeelFlagsProvider.SaveFlags(selectedCharacter.AnimationPath, null, selectedCharacter.HeelInfo.flags);
                    }
                    GUILayout.Space(10);
                    if (GUILayout.Button("Save Path+Name", GUILayout.Width(115)))
                    {
                        HeelFlagsProvider.SaveFlags(selectedCharacter.AnimationPath, selectedCharacter.AnimationName, selectedCharacter.HeelInfo.flags);
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
                GUILayout.Label("Heel Settings");

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Ankle Angle", GUILayout.Width(80));
                    selectedCharacter.HeelInfo.AnkleAngle = GUILayout.HorizontalSlider(selectedCharacter.HeelInfo.AnkleAngle, 0f, 60f);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Leg Angle", GUILayout.Width(80));
                    selectedCharacter.HeelInfo.LegAngle = GUILayout.HorizontalSlider(selectedCharacter.HeelInfo.LegAngle, 0f, 60f);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Height", GUILayout.Width(80));
                    selectedCharacter.HeelInfo.Height = GUILayout.HorizontalSlider(selectedCharacter.HeelInfo.Height, 0f, 0.5f);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
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

            var windowHeight = 340;
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

            return HeelInfoContext.HeelInfos.Select(x => new CharaDisplayData(x)).ToArray();
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
