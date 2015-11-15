using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Staple.EditorScripts
{
    public class SpriteSettingsConfigWindow : EditorWindow
    {
        Editor configEditor;
        private SpriteSettingsConfig config;
        private Vector2 scrollPos;

        void OnEnable()
        {
            config = SpriteSettingsConfig.LoadConfig ();
            if (config != null) {
                configEditor = Editor.CreateEditor (config);
            }
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnGUI()
        {
            if (config == null) return;
            
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