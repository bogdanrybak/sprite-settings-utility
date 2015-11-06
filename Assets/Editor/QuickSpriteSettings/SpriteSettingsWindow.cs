using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Staple.EditorScripts
{
    public class SpriteSettingsWindow : EditorWindow
    {
        const string SettingsPath = "Assets/Editor/QuickSpriteSettings/DefaultSpriteSettings.asset";

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

        private Editor textureSettingsEditor;
        private SpriteSettings currentSelectedSettings;
        private int selectedSettingIndex;
        private bool showSettings;
        private bool changePivot = true;
        private bool changePackingTag;
        private SpriteSettingsConfig config;
        private Vector2 bodyScrollPos;

        void OnEnable()
        {
            config = 
                AssetDatabase.LoadAssetAtPath(SettingsPath, typeof(SpriteSettingsConfig)) as SpriteSettingsConfig;

            if (config == null)
                config = ScriptableObjectUtility.CreateAssetAtPath<SpriteSettingsConfig>(SettingsPath);
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnGUI()
        {
            if (config == null) return;
            
            if (config.SettingsSets == null || config.SettingsSets.Count == 0) {
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.inspectorFullWidthMargins);
            EditorGUILayout.Space();

            DrawApplyButton();

            EditorGUILayout.Space();
            
            DrawSaveSettingSelect ();

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
            }
            GUI.backgroundColor = defaultBg;
        }
        
        void DrawSaveSettingSelect ()
        {
            string[] savedSetNames = new string[config.SettingsSets.Count];
            int[] savedSetIndeces = new int[savedSetNames.Length];
            for (int i = 0; i < savedSetNames.Length; i++) {
                savedSetNames[i] = config.SettingsSets[i].Name;
                savedSetIndeces[i] = i;
            }
            selectedSettingIndex = EditorGUILayout.IntPopup ("Saved Settings", selectedSettingIndex, 
                                                        savedSetNames, savedSetIndeces);
            currentSelectedSettings = config.SettingsSets [selectedSettingIndex];
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