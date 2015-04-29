using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace Staple.EditorScripts
{
    public class SpriteSettingsUtility
    {
        public class SpriteSheetData
        {
            public Vector2 Size;
            public uint Frames;
        }

        public static void ApplyDefaultTextureSettings(
            SpriteSettings prefs,
            bool changePivot,
            bool changePackingTag)
        {
            if (prefs == null) return;

            foreach (var obj in Selection.objects)
            {
                if (!AssetDatabase.Contains(obj)) continue;

                string path = AssetDatabase.GetAssetPath(obj);

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                // Try to slice it
                var fileName = Path.GetFileNameWithoutExtension(path);
                SpriteSheetData spriteSheetData = GetSpriteData(path, prefs.SpritesheetDataFile);

                if (spriteSheetData != null)
                {

                    var gridRects = InternalSpriteUtility.GenerateGridSpriteRectangles(
                        AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D,
                        Vector2.zero, spriteSheetData.Size, Vector2.zero);

                    var spriteSheet = importer.spritesheet ?? new SpriteMetaData[gridRects.Length];

                    if (importer.spritesheet != null)
                        spriteSheet = spriteSheet.Concat(new SpriteMetaData[Mathf.Max(0, gridRects.Length - importer.spritesheet.Length)]).ToArray();

                    for (var i = 0; i < spriteSheet.Length; i++)
                    {
                        bool changed = changePivot && (importer.spritesheet == null || i < importer.spritesheet.Length);
                        spriteSheet[i] = new SpriteMetaData
                        {
                            alignment = changed ? (int)prefs.SpriteAlignment : spriteSheet[i].alignment,
                            pivot = changed ? prefs.CustomPivot : spriteSheet[i].pivot,
                            name = fileName + "_" + Array.IndexOf(gridRects, gridRects[i]),
                            rect = gridRects[i]
                        };
                    }

                    // If we don't do this it won't update the new sprite meta data
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spriteImportMode = SpriteImportMode.Multiple;

                    if (spriteSheetData.Frames > 0)
                        importer.spritesheet = spriteSheet.Take((int)spriteSheetData.Frames).ToArray();
                    else
                        importer.spritesheet = spriteSheet;
                }
                else
                    importer.spriteImportMode = SpriteImportMode.Single;

                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);

                settings.filterMode = prefs.FilterMode;
                settings.wrapMode = prefs.WrapMode;
                settings.mipmapEnabled = prefs.GenerateMipMaps;
                settings.textureFormat = prefs.TextureFormat;
                settings.maxTextureSize = prefs.MaxSize;

                settings.spritePixelsPerUnit = prefs.PixelsPerUnit;

                settings.spriteExtrude = prefs.SpriteExtrude;
                settings.spriteMeshType = prefs.SpriteMeshType;

                if (changePivot)
                {
                    settings.spriteAlignment = (int)prefs.SpriteAlignment;
                    if (prefs.SpriteAlignment == SpriteAlignment.Custom)
                        settings.spritePivot = prefs.CustomPivot;
                }

                if (changePackingTag)
                    importer.spritePackingTag = prefs.PackingTag;

                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();

                EditorUtility.SetDirty(obj);
            }
        }


        public static SpriteSheetData GetSpriteData(string path, string dataFileName)
        {
            var spriteSheetDataFile = AssetDatabase.LoadAssetAtPath(
                Path.GetDirectoryName(path) + "/" + dataFileName, typeof(TextAsset)
                ) as TextAsset;

            return GetSpriteData(path, spriteSheetDataFile);
        }

        public static SpriteSheetData GetSpriteData(string path, TextAsset spriteSheetDataFile)
        {
            if (spriteSheetDataFile != null)
            {
                string[] entries = spriteSheetDataFile.text.Split(
                    new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

                string entry = entries.FirstOrDefault(x => x.StartsWith(Path.GetFileName(path)));

                if (!string.IsNullOrEmpty(entry))
                {
                    string[] entryData = entry.Split(',');
                    var data = new SpriteSheetData();
                    try
                    {
                        float width = int.Parse(entryData[1]);
                        float height = int.Parse(entryData[2]);
                        data.Size = new Vector2(width, height);

                        // number of frames is optional
                        uint frames = 0;
                        if (entryData.Length > 3)
                            if (uint.TryParse(entryData[3], out frames))
                                data.Frames = frames;

                        return data;
                    }
                    catch
                    {
                        Debug.LogError("Invalid sprite data at line: " + Array.IndexOf(entries, entry) + ", (" + entry + ")");
                    }
                }
            }

            return null;
        }
    }
}