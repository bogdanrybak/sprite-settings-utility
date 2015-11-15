using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Staple.EditorScripts
{
    public class SpriteSettingsConfig : ScriptableObject
    {
        // Future ready: Implement sprite settings sets;
        public List<SpriteSettings> SettingsSets;
        public const string ConfigGUIDKey = "SpriteSettingsConfigGUID";

        void OnEnable()
        {
            //hideFlags = HideFlags.DontSaveInBuild;

            SettingsSets = SettingsSets ?? new List<SpriteSettings>();
            if (SettingsSets.Count < 1)
            {
                AddDefaultSpriteSetting ();
            }
        }
        
        public void AddDefaultSpriteSetting ()
        {
            SettingsSets.Add(new SpriteSettings());
        }
        
        public static SpriteSettingsConfig LoadConfig ()
        {
            string configGUID = EditorPrefs.GetString (SpriteSettingsConfig.ConfigGUIDKey);
            string configPath = AssetDatabase.GUIDToAssetPath (configGUID);
            return AssetDatabase.LoadAssetAtPath(configPath,
                typeof(SpriteSettingsConfig)) as SpriteSettingsConfig;
        }
    }

    [CustomEditor(typeof(SpriteSettingsConfig))]
    public class SpriteSettingsConfigEditor : Editor
    {
        private ReorderableList list;

        private void OnEnable() {
            list = new ReorderableList(serializedObject, 
                    serializedObject.FindProperty("SettingsSets"), 
                    true, true, true, true);
            list.drawHeaderCallback = (Rect rect) => {  
                EditorGUI.LabelField(rect, "Saved SpriteSettings");
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            list.DoLayoutList();
            
            EditorGUILayout.Space ();
            EditorGUILayout.LabelField ("Selected Settings", EditorStyles.largeLabel);
            
            DrawSelectedSpriteSetting (serializedObject);
            
            serializedObject.ApplyModifiedProperties();
        }
        
        void DrawSelectedSpriteSetting (SerializedObject serializedObject)
        {
            int selectedIndex = list.index;
            
            SerializedProperty settingsSets = serializedObject.FindProperty ("SettingsSets");
            
            if (selectedIndex >= 0) {
                EditorGUILayout.PropertyField (settingsSets.GetArrayElementAtIndex (selectedIndex));
            } else {
                EditorStyles.label.wordWrap = true;
                if (GetPropertyArraySize (settingsSets) > 0) {
                    EditorGUILayout.LabelField ("Select a Saved SpriteSetting to edit or remove it.");
                } else {
                    EditorGUILayout.LabelField ("No SpriteSettings have been saved. Create a new SpriteSetting from the list above.");
                }
            }
        }
        
        int GetPropertyArraySize (SerializedProperty listAsProperty)
        {
            if (!listAsProperty.isArray) {
                return -1;
            }
        
            // Don't iterate on the original
            SerializedProperty listCopy = listAsProperty.Copy ();
        
            listCopy.Next (true); // Skip generic element
            listCopy.Next (true); // This is the array size
        
            return listCopy.intValue;
		}
        
        public void SelectSetting (int settingIndex)
        {
            if (settingIndex >= list.count || settingIndex < 0) {
                return;
            }
            
            list.index = settingIndex;
        }
    }
}
