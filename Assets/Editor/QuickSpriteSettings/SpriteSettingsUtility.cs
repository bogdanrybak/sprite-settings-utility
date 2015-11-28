using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace Staple.EditorScripts
{
    public class SpriteSettingsUtility
    {
        public static void ApplyDefaultTextureSettings(Texture2D texture,
            SpriteSettings prefs, SpriteSlicingOptions slicingOptions,
            bool changePivot,
            bool changePackingTag)
        {
            if (prefs == null) return;

            string path = AssetDatabase.GetAssetPath (texture);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            // When we have text file data
            if (slicingOptions.ImportMode == SpriteImportMode.Multiple)
            {
                // Clamp cellSize to texture width and height
                slicingOptions.CellSize.x = Mathf.Min (texture.width, slicingOptions.CellSize.x);
                slicingOptions.CellSize.y = Mathf.Min (texture.height, slicingOptions.CellSize.y);
                
                SpriteMetaData[] spriteSheet;
                if (slicingOptions.GridSlicing == SpriteSlicingOptions.GridSlicingMethod.SliceAll) {
                    spriteSheet = SpriteSlicer.CreateSpriteSheetForTexture (AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D,
                        slicingOptions.CellSize, prefs.Pivot);
                } else {
                    spriteSheet = SpriteSlicer.CreateSpriteSheetForTextureBogdan (AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D,
                        slicingOptions.CellSize, changePivot, prefs.Pivot, prefs.CustomPivot, (uint)slicingOptions.Frames);
                }

                // If we don't do this it won't update the new sprite meta data
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spriteImportMode = SpriteImportMode.Multiple;

                importer.spritesheet = spriteSheet;
            }
            else if (slicingOptions.ImportMode == SpriteImportMode.Single) 
            {
                importer.spriteImportMode = SpriteImportMode.Single;
            } else if (slicingOptions.ImportMode == SpriteImportMode.None)
            {
                // Do nothing for None mode for now.
            } else
            {
                throw new System.NotSupportedException ("Encountered unsupported SpriteImportMode:" 
                    + importer.spriteImportMode);
            }

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);

            settings.filterMode = prefs.FilterMode;
            settings.wrapMode = prefs.WrapMode;
            settings.mipmapEnabled = prefs.GenerateMipMaps;
            settings.textureFormat = prefs.TextureFormat;
            settings.maxTextureSize = prefs.MaxSize;

            settings.spritePixelsPerUnit = prefs.PixelsPerUnit;

            settings.spriteExtrude = (uint)Mathf.Clamp(prefs.ExtrudeEdges, 0, 32);
            settings.spriteMeshType = prefs.SpriteMeshType;

            if (changePivot)
            {
                settings.spriteAlignment = (int)prefs.Pivot;
                if (prefs.Pivot == SpriteAlignment.Custom)
                    settings.spritePivot = prefs.CustomPivot;
            }

            if (changePackingTag)
                importer.spritePackingTag = prefs.PackingTag;

            importer.SetTextureSettings(settings);
#if UNITY_5_0
            importer.SaveAndReimport();
#else
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
#endif
            EditorUtility.SetDirty(texture);
            WriteSlicingOptions (path, prefs.SpritesheetDataFile, texture.name + ".png", slicingOptions);
        }


        public static SpriteSlicingOptions  GetSlicingOptions(string path, string dataFileName)
        {
            var spriteSheetDataFile = AssetDatabase.LoadAssetAtPath(
                Path.GetDirectoryName(path) + "/" + dataFileName, typeof(TextAsset)
                ) as TextAsset;

            return GetSlicingOptions(path, spriteSheetDataFile);
        }

        public static SpriteSlicingOptions GetSlicingOptions(string path, TextAsset slicingOptionsDataFile)
        {
            if (slicingOptionsDataFile != null)
            {
                string[] entries = slicingOptionsDataFile.text.Split(
                    new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

                string entry = entries.FirstOrDefault(x => x.StartsWith(Path.GetFileName(path)));

                if (!string.IsNullOrEmpty(entry))
                {
                    try {
                        // Strip filename
                        int firstIndex = entry.IndexOf (',') + 1;
                        int lastIndex = entry.Length - 1;
                        var slicingData = entry.Substring (firstIndex, lastIndex - firstIndex + 1);
                        return SpriteSlicingOptions.FromString (slicingData);
                    } catch (SystemException e)
                    {
                        Debug.LogError (string.Format ("Encountered error in saved slicing options file. Entry: " + entry
                            + "\n Error: " + e));
                    }
                }
            }

            return new SpriteSlicingOptions ();
        }
        public static void WriteSlicingOptions(string path, string dataFileName, string key, SpriteSlicingOptions slicingOptions)
        {
            string textAssetPath = Path.GetDirectoryName(path) + "/" + dataFileName;
            var spriteSheetDataFile = AssetDatabase.LoadAssetAtPath(textAssetPath, typeof(TextAsset)) as TextAsset;
            
            string newEntry = string.Concat (key, ", ", slicingOptions.ToString ());
            
            // Create new file if none exists
            if (spriteSheetDataFile == null) 
            {
                File.WriteAllText (textAssetPath, newEntry);
            } else
            {
                string existing = File.ReadAllText (textAssetPath);
                if (existing.Contains (key))
                {
                    string[] entries = spriteSheetDataFile.text.Split(
                        new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < entries.Length; i++)
                    {
                        if (entries[i].Contains(key))
                        {
                            entries[i] = newEntry;
                        }
                    }
                    File.WriteAllLines (textAssetPath, entries);
                } else 
                {
                    File.AppendAllText (textAssetPath, "\n" + newEntry);
                }
            }
            
            AssetDatabase.ImportAsset(textAssetPath, ImportAssetOptions.ForceUpdate);
            if (spriteSheetDataFile != null)
            {
                EditorUtility.SetDirty(spriteSheetDataFile);
            }
        }
    }
}