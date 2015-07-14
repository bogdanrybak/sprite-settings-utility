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
            if (config == null || config.Settings == null) return;

            EditorGUILayout.BeginVertical(EditorStyles.inspectorFullWidthMargins);
            EditorGUILayout.Space();

            DrawApplyButton();

            EditorGUILayout.Space();

            DrawQuickSettings();

            EditorGUILayout.Space();

            bodyScrollPos = EditorGUILayout.BeginScrollView(bodyScrollPos);

            DrawSelectedTextureInfo();            

            EditorGUILayout.Space();

            showSettings = EditorGUILayout.Foldout(showSettings, "Default Texture Settings");

            if (showSettings)
            {
                textureSettingsEditor = Editor.CreateEditor(config);
                textureSettingsEditor.OnInspectorGUI();
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        void DrawApplyButton()
        {
            Color defaultBg = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Apply"))
            {
                SpriteSettingsUtility.ApplyDefaultTextureSettings(config.Settings, changePivot, changePackingTag);
                
				if (config.Settings.PackOnApply)
				{
                	UnityEditor.Sprites.Packer.RebuildAtlasCacheIfNeeded(
                	EditorUserBuildSettings.activeBuildTarget, true);
				}
                
                Close();
            }
            GUI.backgroundColor = defaultBg;
        }

        void DrawQuickSettings()
        {
            changePivot = EditorGUILayout.BeginToggleGroup("Apply Pivot", changePivot);

            config.Settings.Pivot = (SpriteAlignment)EditorGUILayout.EnumPopup(config.Settings.Pivot);

            if (config.Settings.Pivot == SpriteAlignment.Custom)
                config.Settings.CustomPivot = EditorGUILayout.Vector2Field("Custom Pivot", config.Settings.CustomPivot);

            EditorGUILayout.EndToggleGroup();

            EditorGUILayout.Space();

            changePackingTag = EditorGUILayout.BeginToggleGroup("Apply Packing Tag", changePackingTag);

            config.Settings.PackingTag = EditorGUILayout.TextField(config.Settings.PackingTag);

            if (changePackingTag)
            {
                foreach (var name in UnityEditor.Sprites.Packer.atlasNames)
                {
                    if (GUILayout.Button(name, EditorStyles.miniButton))
                    {
                        config.Settings.PackingTag = name;
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

                    var spriteSheetData = SpriteSettingsUtility.GetSpriteData(path, config.Settings.SpritesheetDataFile);
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