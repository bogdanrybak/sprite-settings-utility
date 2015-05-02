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

        public SpriteSettings Settings { get; private set; }

        void OnEnable()
        {
            //hideFlags = HideFlags.DontSaveInBuild;

            SettingsSets = SettingsSets ?? new List<SpriteSettings>();
            if (SettingsSets.Count < 1)
            {
                SettingsSets.Add(new SpriteSettings());
            }

            Settings = SettingsSets[0];
        }
    }

    [CustomEditor(typeof(SpriteSettingsConfig))]
    public class SpriteSettingsConfigEditor : Editor
    {
        SerializedProperty settings;

        void Awake()
        {
            settings = serializedObject.FindProperty("SettingsSets");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (settings.isArray)
            {
                settings.Next(true); // generic field
                settings.Next(true); // array size field

                // first array index
                settings.Next(true);

                EditorGUILayout.PropertyField(settings);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
