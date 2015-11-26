using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace Staple.EditorScripts
{
    public class SpriteSettingsConfig : ScriptableObject
    {
        public List<SpriteSettings> SettingsSets;
        public const string DefaultFilename = "DefaultSpriteSettings.asset";

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
            list.onAddCallback = (ReorderableList l) => {
                SpriteSettingsConfig objAsConfig = (SpriteSettingsConfig) serializedObject.targetObject;
                objAsConfig.AddDefaultSpriteSetting ();
                // Predict new size since it's not serialized yet
                int newSize = l.serializedProperty.arraySize + 1;
                SelectSetting (newSize - 1);
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
            
            if (selectedIndex >= 0 && selectedIndex < settingsSets.arraySize) {
                EditorGUILayout.PropertyField (settingsSets.GetArrayElementAtIndex (selectedIndex));
            } else {
                EditorStyles.label.wordWrap = true;
                if (settingsSets.arraySize > 0) {
                    EditorGUILayout.LabelField ("Select a Saved SpriteSetting to edit or remove it.");
                } else {
                    EditorGUILayout.LabelField ("No SpriteSettings have been saved. Create a new SpriteSetting from the list above.");
                }
            }
        }
        
        public void SelectSetting (int settingIndex)
        {
            // Note it's allowed to select indeces outside list bounds since we need to select
            // items before they are serialized sometimes.
            if (settingIndex < 0) {
                return;
            }
            
            list.index = settingIndex;
        }
    }
}