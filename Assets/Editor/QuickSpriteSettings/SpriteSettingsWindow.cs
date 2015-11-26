using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Staple.EditorScripts
{
    public class SpriteSettingsWindow : EditorWindow
    {
        [MenuItem("Assets/Quick Sprite Settings")]
        public static void Init()
        {
            EditorWindow.GetWindow<SpriteSettingsWindow>("Sprite Settings", true);
        }

        [MenuItem("Assets/Quick Sprite Settings", true)]
        public static bool TextureSelected()
        {
            foreach (var obj in Selection.objects)
            {
                if (!AssetDatabase.Contains(obj)) continue;

                var asset =
                    AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(obj), typeof(Texture2D)) as Texture2D;

                if (asset != null)
                    return true;
            }

            return false;
        }

        private SpriteSettings currentSelectedSettings;
        private int selectedSettingIndex;
        private bool showSettings;
        private bool changePivot = true;
        private bool changePackingTag;
        private SpriteSettingsConfig config;
        private Vector2 bodyScrollPos;
        SpriteSettingsConfigWindow configWindow;

        void OnEnable()
        {
            config = AssetDatabase.LoadAssetAtPath(GetPathToConfig (), typeof(SpriteSettingsConfig)) as SpriteSettingsConfig;
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnGUI()
        {
            if (config == null || config.SettingsSets == null || config.SettingsSets.Count == 0) {
                DrawEmptySaveSettings ();
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.inspectorFullWidthMargins);
            EditorGUILayout.Space();

            DrawApplyButton();

            EditorGUILayout.Space();
            
            DrawSaveSettingSelect ();
            
            EditorGUILayout.Space ();

            DrawQuickSettings();

            EditorGUILayout.Space();

            bodyScrollPos = EditorGUILayout.BeginScrollView(bodyScrollPos);

            DrawSelectedTextureInfo();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        void DrawApplyButton()
        {
            Color defaultBg = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Apply"))
            {
                SpriteSettingsUtility.ApplyDefaultTextureSettings(currentSelectedSettings, changePivot, changePackingTag);
                
				if (currentSelectedSettings.PackOnApply)
				{
                	UnityEditor.Sprites.Packer.RebuildAtlasCacheIfNeeded(
                	EditorUserBuildSettings.activeBuildTarget, true);
				}
                
                Close();
                if (configWindow != null) 
                {
                    configWindow.Close ();
                }
            }
            GUI.backgroundColor = defaultBg;
        }
        
        void DrawEmptySaveSettings ()
        {
            EditorGUILayout.Space ();
            EditorGUILayout.LabelField ("Create a Saved SpriteSetting to start applying SpriteSettings.");
            if (GUILayout.Button ("Create Setting")) {
                if (config == null) {
                    CreateConfig ();
                }
                
                ShowConfigWindow (0);
                
                if (config.SettingsSets.Count == 0) {
                    config.AddDefaultSpriteSetting ();
                }
            }
            EditorGUILayout.Space ();
        }
        
        void CreateConfig ()
        {
            config = ScriptableObjectUtility.CreateAssetAtPath<SpriteSettingsConfig>(GetPathToConfig ());
        }
        
        string GetPathToConfig ()
        {
            MonoScript script = MonoScript.FromScriptableObject (this);
            string scriptPath = AssetDatabase.GetAssetPath (script);
            string scriptDirectory = System.IO.Path.GetDirectoryName (scriptPath);
            string filename = SpriteSettingsConfig.DefaultFilename;
            return scriptDirectory + System.IO.Path.DirectorySeparatorChar + filename;
        }
        
        void ShowConfigWindow (int indexToFocus)
        {
                configWindow = EditorWindow.GetWindow<SpriteSettingsConfigWindow>("Saved SpriteSettings", true);
                configWindow.SetConfig (config);
                configWindow.SelectSetting (indexToFocus);
        }
        
        void DrawSaveSettingSelect ()
        {
            string[] savedSetNames = new string[config.SettingsSets.Count];
            int[] savedSetIndeces = new int[savedSetNames.Length];
            for (int i = 0; i < savedSetNames.Length; i++) {
                savedSetNames[i] = config.SettingsSets[i].Name;
                savedSetIndeces[i] = i;
            }
            // Make sure a previously selected index still exists
            if (selectedSettingIndex >= savedSetNames.Length) {
                selectedSettingIndex = savedSetNames.Length -1;
            }
            EditorGUILayout.BeginHorizontal ();
            selectedSettingIndex = EditorGUILayout.IntPopup ("Setting to Apply", selectedSettingIndex, 
                                                        savedSetNames, savedSetIndeces);
            currentSelectedSettings = config.SettingsSets [selectedSettingIndex];
            if (GUILayout.Button ("Edit", GUILayout.MaxWidth(80.0f))) {
                ShowConfigWindow (selectedSettingIndex);
            }
            EditorGUILayout.EndHorizontal ();
        }

        void DrawQuickSettings()
        {
            changePivot = EditorGUILayout.BeginToggleGroup("Apply Pivot", changePivot);

            currentSelectedSettings.Pivot = (SpriteAlignment)EditorGUILayout.EnumPopup(currentSelectedSettings.Pivot);

            if (currentSelectedSettings.Pivot == SpriteAlignment.Custom)
                currentSelectedSettings.CustomPivot = EditorGUILayout.Vector2Field("Custom Pivot", currentSelectedSettings.CustomPivot);

            EditorGUILayout.EndToggleGroup();

            EditorGUILayout.Space();

            changePackingTag = EditorGUILayout.BeginToggleGroup("Apply Packing Tag", changePackingTag);

            currentSelectedSettings.PackingTag = EditorGUILayout.TextField(currentSelectedSettings.PackingTag);

            if (changePackingTag)
            {
                foreach (var name in UnityEditor.Sprites.Packer.atlasNames)
                {
                    if (GUILayout.Button(name, EditorStyles.miniButton))
                    {
                        currentSelectedSettings.PackingTag = name;
                        EditorGUI.FocusTextInControl(name);
                    }
                }
            }
            
            EditorGUILayout.EndToggleGroup();
        }

        void DrawSelectedTextureInfo()
        {
            if (Selection.objects.Length > 0)
            {
                EditorGUILayout.LabelField("Selected textures");
                EditorGUILayout.Space();

                Color defaultColor = GUI.color;
                GUI.color = Color.yellow;

                foreach (var obj in Selection.objects)
                {
                    if (!AssetDatabase.Contains(obj)) continue;

                    string path = AssetDatabase.GetAssetPath(obj);

                    var asset = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
                    if (asset == null) continue;

                    string metaData = "";

                    var spriteSheetData = SpriteSettingsUtility.GetSpriteData(path, currentSelectedSettings.SpritesheetDataFile);
                    if (spriteSheetData != null)
                    {
                        metaData = " " + spriteSheetData.Size + ", " + spriteSheetData.Frames + " frames";
                    }

                    EditorGUILayout.LabelField(path + metaData);
                }

                GUI.color = defaultColor;
            }
        }
    }
}