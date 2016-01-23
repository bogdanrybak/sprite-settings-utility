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
        
        public void SetConfig (SpriteSettingsConfig config) {
            this.config = config;
            if (this.config != null) {
                configEditor = Editor.CreateEditor (config);
            }
        }
        void OnEnable ()
        {
            // Reload config if this window gets rebuilt (which happens when recompiling with an open window)
            ReloadConfig ();
        }
        
        void ReloadConfig ()
        {
            if (config != null) 
            {
                config = AssetDatabase.LoadAssetAtPath<SpriteSettingsConfig> (AssetDatabase.GetAssetPath (config));
                if (this.config != null ) {
                    configEditor = Editor.CreateEditor (config);
                }
            }
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnGUI()
        {
            if (config == null) {
                EditorGUILayout.HelpBox ("Trying to view Saved SpriteSettings but no settings file exists.", 
                    MessageType.Error);
                return;
            }
            
            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            scrollPos = EditorGUILayout.BeginScrollView (scrollPos);
            EditorGUILayout.Space ();
            
            configEditor.OnInspectorGUI ();
            
            EditorGUILayout.EndScrollView ();
            EditorGUILayout.EndVertical ();
        }
        public void SelectSetting (int settingIndex)
        {
            if (configEditor == null) {
                return;
            }
            ((SpriteSettingsConfigEditor)configEditor).SelectSetting (settingIndex);
        }
    }
}