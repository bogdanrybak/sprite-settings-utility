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

        void OnEnable()
        {
            //hideFlags = HideFlags.DontSaveInBuild;

            SettingsSets = SettingsSets ?? new List<SpriteSettings>();
            if (SettingsSets.Count < 1)
            {
                SettingsSets.Add(new SpriteSettings());
            }
        }
    }

    [CustomEditor(typeof(SpriteSettingsConfig))]
    public class SpriteSettingsConfigEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector ();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
