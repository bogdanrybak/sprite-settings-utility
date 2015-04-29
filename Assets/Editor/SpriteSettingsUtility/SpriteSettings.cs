﻿using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Staple.EditorScripts
{
    [Serializable]
    public class SpriteSettings
    {
        public string Name = "Default";

        public int PixelsPerUnit = 100;
        public string PackingTag = "";
        public SpriteAlignment SpriteAlignment;
        public Vector2 CustomPivot;

        public SpriteMeshType SpriteMeshType;
        [Range(0, 32)]
        public uint SpriteExtrude = 1;

        public bool GenerateMipMaps;
        public FilterMode FilterMode;
        public TextureWrapMode WrapMode;
        public TextureImporterFormat TextureFormat = TextureImporterFormat.AutomaticTruecolor;
        public int MaxSize = 2048;

        /// <summary>
        /// The expected format is: fileName,cellWidth,cellHeight,numberOfFrames
        /// for example the specified text file would contain these entries each on new line:
        /// sheet_1.png,25,25,3
        /// sheet_2.png,27,30,5
        /// </summary>
        public string SpritesheetDataFile = "_spritesheets.txt";
    }

    [CustomPropertyDrawer(typeof(SpriteSettings))]
    public class SpriteSettingsEditor : PropertyDrawer
    {
        static readonly string[] dontInclude = new string[] { "Name" };
        static readonly string[] spaceBefore = new string[] { "SpriteMeshType", "GenerateMipMaps", "SpritesheetDataFile" };
        static readonly int[] maxSizes = { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };

        private float lineHeight = EditorGUIUtility.singleLineHeight;
        private float totalHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var pos = position;
            float startingY = pos.y; 
            foreach (SerializedProperty p in property)
            {
                if (dontInclude.Contains(p.name) || p.depth > 2) continue;

                var height = lineHeight;
                if (spaceBefore.Contains(p.name))
                    height *= 2;

                pos = new Rect(pos.x, pos.y + height + 2f, pos.width, lineHeight);
                if (p.name == "MaxSize")
                    EditorGUI.IntPopup(pos, p, maxSizes.Select(x => new GUIContent(x.ToString())).ToArray(), maxSizes);
                else
                {
                    EditorGUI.PropertyField(pos, p);
                    if (p.name == "CustomPivot")
                        pos.y += lineHeight;
                }
            }

            totalHeight = pos.y + lineHeight - startingY;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return totalHeight;
        }
    }
}