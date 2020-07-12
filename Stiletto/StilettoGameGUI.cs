using Manager;
using Stiletto.Configurations;
using System.Linq;
using UnityEngine;
using static SaveData;

namespace Stiletto
{
    public class StilettoGameGUI
    {
        private const int ScreenOffset = 20;
        private Rect _windowRect;
        private Rect _screenRect;
        private Heroine[] _currentHeroines = new Heroine[0];

        private HeelInfo selectedHeelInfo;
        private Heroine selectedHeroine;
        private int selectedIndex = -1;

        public void Start(Stiletto plugin)
        {
            SetWindowSizes();
        }

        public void DisplayWindow(int id)
        {
            var skinBack = GUI.skin;
            _windowRect = GUILayout.Window(id, _windowRect, CreateWindowContent, "Stiletto");
        }

        public void CreateWindowContent(int id)
        {
            GUILayout.BeginVertical();
            {
                var currentHeroines = GetCurrentVisibleGirls();
                var updatedHeroines = !CompareHeroines(currentHeroines, _currentHeroines);

                _currentHeroines = currentHeroines;

                if (_currentHeroines.Length > 0)
                {
                    if (selectedHeroine == null)
                    {
                        SelectHeelInfo(0);
                    }
                    else if (updatedHeroines)
                    {
                        selectedIndex = _currentHeroines.ElementAtOrDefault(selectedIndex) == null ? 0 : selectedIndex;
                        SelectHeelInfo(selectedIndex);
                    }
                    else 
                    {
                        SelectHeelInfo(selectedIndex);
                    }

                    CreateCurrentHeroineContent();

                    GUILayout.Space(10);

                    CreateAnimationSettingsContent();

                    // GUILayout.Space(10);
                    // CreateHeelSettingsContent();

                    GUILayout.Space(10);

                    if (GUILayout.Button("Reload Configuration"))
                    {
                        HeelFlagsProvider.ReloadHeelFlags();
                    }

                    GUILayout.EndHorizontal();

                } 
                else 
                {
                    SelectHeelInfo(-1);
                }
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void CreateCurrentHeroineContent() 
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label("Current Heroine");
                CreateDisplayLabel("Name:", selectedHeroine.Name);
                CreateDisplayLabel("Heel:", selectedHeelInfo.heelName);
                CreateDisplayLabel("Anim Path:", selectedHeelInfo.animationPath);
                CreateDisplayLabel("Anim Name:", selectedHeelInfo.animationName);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label($"Total: {_currentHeroines.Length}", GUILayout.Width(70));

                    var lastIndex = _currentHeroines.Length - 1;

                    if (GUILayout.Button("Privous", GUILayout.Width(80)))
                    {
                        SelectHeelInfo(selectedIndex == 0 ? lastIndex : selectedIndex - 1);
                    }
                    GUILayout.Space(10);

                    if (GUILayout.Button("Next", GUILayout.Width(80)))
                    {
                        SelectHeelInfo(selectedIndex == lastIndex ? 0 : selectedIndex + 1);
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
                    selectedHeelInfo.flags.ACTIVE = GUILayout.Toggle(selectedHeelInfo.flags.ACTIVE, " Active");
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    selectedHeelInfo.flags.TOE_ROLL = GUILayout.Toggle(selectedHeelInfo.flags.TOE_ROLL, " Toe Roll", GUILayout.Width(120));
                    selectedHeelInfo.flags.ANKLE_ROLL = GUILayout.Toggle(selectedHeelInfo.flags.ANKLE_ROLL, " Ankle Roll", GUILayout.Width(120));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    selectedHeelInfo.flags.HEIGHT = GUILayout.Toggle(selectedHeelInfo.flags.HEIGHT, " Height", GUILayout.Width(120));
                    selectedHeelInfo.flags.KNEE_BEND = GUILayout.Toggle(selectedHeelInfo.flags.KNEE_BEND, " Keen Bend", GUILayout.Width(120));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Save PathOnly", GUILayout.Width(115)))
                    {
                        HeelFlagsProvider.SaveFlags(selectedHeelInfo.animationPath, null, selectedHeelInfo.flags);
                    }
                    GUILayout.Space(10);
                    if (GUILayout.Button("Save Path+Name", GUILayout.Width(115)))
                    {
                        HeelFlagsProvider.SaveFlags(selectedHeelInfo.animationPath, selectedHeelInfo.animationName, selectedHeelInfo.flags);
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
                    selectedHeelInfo.AnkleAngle = GUILayout.HorizontalSlider(selectedHeelInfo.AnkleAngle, 0f, 60f);
                }
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Leg Angle", GUILayout.Width(80));
                    selectedHeelInfo.LegAngle = GUILayout.HorizontalSlider(selectedHeelInfo.LegAngle, 0f, 60f);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Height", GUILayout.Width(80));
                    selectedHeelInfo.Height = GUILayout.HorizontalSlider(selectedHeelInfo.Height, 0f, 0.5f);
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

        private void SelectHeelInfo(int index)
        {
            selectedHeroine = _currentHeroines.ElementAtOrDefault(index);
            selectedIndex = index;

            if (selectedHeroine != null)
            {
                selectedHeelInfo = HeelInfoContext.HeelInfos.FirstOrDefault(x => x.chaControl == selectedHeroine.chaCtrl);
            }
            else 
            {
                selectedHeelInfo = null;
            }
        }

        private void SetWindowSizes()
        {
            int w = Screen.width, h = Screen.height;
            _screenRect = new Rect(ScreenOffset, ScreenOffset, w - ScreenOffset * 2, h - ScreenOffset * 2);

            const int windowHeight = 310;
            _windowRect = new Rect(_screenRect.xMin, _screenRect.yMax - windowHeight, 270, windowHeight);
        }

        private Heroine[] GetCurrentVisibleGirls()
        {
            var result = Object.FindObjectOfType<TalkScene>()?.targetHeroine;
            if (result != null) return new[] { result };

            var hHeroines = Object.FindObjectOfType<HFlag>()?.lstHeroine;
            if (hHeroines != null && hHeroines.Count > 0) return hHeroines.ToArray();

            if (Game.IsInstance() &&
                Game.Instance.actScene != null &&
                Game.Instance.actScene.AdvScene != null)
            {
                var advScene = Game.Instance.actScene.AdvScene;
                if (advScene.Scenario?.currentHeroine != null)
                    return new[] { advScene.Scenario.currentHeroine };
                if (advScene.nowScene is TalkScene s && s.targetHeroine != null)
                    return new[] { s.targetHeroine };
            }

            return new Heroine[0];
        }

        private bool CompareHeroines(Heroine[] first, Heroine[] second)
        {
            if (first.Length != second.Length)
                return false;

            for (var i = 0; i < first.Length; i++)
            {
                if (first[i].chaCtrl != second[i].chaCtrl)
                    return false;
            }

            return true;
        }
    }
}
