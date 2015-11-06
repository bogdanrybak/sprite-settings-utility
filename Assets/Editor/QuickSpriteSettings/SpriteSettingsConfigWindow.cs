using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Staple.EditorScripts
{
    public class SpriteSettingsConfigWindow : EditorWindow
    {
        const string SettingsPath = "Assets/Editor/QuickSpriteSettings/DefaultSpriteSettings.asset";
        Editor configEditor;
        private SpriteSettingsConfig config;
        private Vector2 scrollPos;

        void OnEnable()
        {
            config = 
                AssetDatabase.LoadAssetAtPath(SettingsPath, typeof(SpriteSettingsConfig)) as SpriteSettingsConfig;

            if (config == null)
                config = ScriptableObjectUtility.CreateAssetAtPath<SpriteSettingsConfig>(SettingsPath);
                
            configEditor = Editor.CreateEditor (config);
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            scrollPos = EditorGUILayout.BeginScrollView (scrollPos);
            EditorGUILayout.Space ();
            
            configEditor.OnInspectorGUI ();
            
            EditorGUILayout.EndScrollView ();
            EditorGUILayout.EndVertical ();
        }
        public void SelectSetting (int settingIndex)
        {
            ((SpriteSettingsConfigEditor)configEditor).SelectSetting (settingIndex);
        }
    }
}