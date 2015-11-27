using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class SpriteSlicer
{
    public static SpriteMetaData[] CreateSpriteSheetForTexture (Texture2D texture, Vector2 cellSize, 
        SpriteAlignment pivot, Vector2 customPivot = default (Vector2))
    {
        List<SpriteMetaData> sprites = new List<SpriteMetaData> ();
        Rect[] gridRects = GetAllSliceRectsForTexture (texture, cellSize);
        for (int i = 0; i < gridRects.Length; i++) {
            SpriteMetaData spriteMetaData = new SpriteMetaData ();
            spriteMetaData.rect = gridRects[i];
            spriteMetaData.alignment = (int) pivot;
            spriteMetaData.pivot = customPivot;
            spriteMetaData.name = texture.name + "_" + i;
            sprites.Add (spriteMetaData);
        }
        return sprites.ToArray ();
    }
    static Rect[] GetAllSliceRectsForTexture (Texture2D texture, Vector2 cellSize)
    {
        int numSpritesTall = Mathf.FloorToInt (texture.height / cellSize.y);
        int numSpritesWide = Mathf.FloorToInt (texture.width / cellSize.x);
        int i = 0;
        Rect[] rects = new Rect[numSpritesWide * numSpritesTall];
        for (int y = numSpritesTall - 1; y >= 0; y--) {
            for (int x = 0; x < numSpritesWide; x++) {
                Rect rect = new Rect (x * cellSize.x, y * cellSize.y, cellSize.x, cellSize.y);
                rects[i++] = rect;
            }
        }

        return rects;
    }

    public static SpriteMetaData[] CreateSpriteSheetForTextureBogdan (Texture2D texture, Vector2 cellSize, bool forcePivotChange, 
        SpriteAlignment pivot, Vector2 customPivot = default (Vector2), uint frames = 0)
    {
        Rect[] gridRects = InternalSpriteUtility.GenerateGridSpriteRectangles(texture, Vector2.zero, cellSize, Vector2.zero);
        
        string path = AssetDatabase.GetAssetPath(texture);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;

        var spriteSheet = importer.spritesheet ?? new SpriteMetaData[gridRects.Length];

        // Add new sprite meta data to the end for all the newly parsed grid rects?
        if (importer.spritesheet != null)
            spriteSheet = spriteSheet.Concat(new SpriteMetaData[Mathf.Max(0, gridRects.Length - importer.spritesheet.Length)]).ToArray();
        
        for (var i = 0; i < spriteSheet.Length; i++)
        {
            bool sliceExists = importer.spritesheet != null && i < importer.spritesheet.Length;
            bool changePivot = !sliceExists || forcePivotChange;
            spriteSheet[i] = new SpriteMetaData
            {
                alignment = changePivot ? (int) pivot: spriteSheet[i].alignment,
                pivot = changePivot ? customPivot : spriteSheet[i].pivot,
                name = sliceExists ? spriteSheet[i].name : texture.name + "_" + i,
                rect = gridRects[i]
            };
        }
        
        if (frames > 0)
            spriteSheet = spriteSheet.Take((int) frames).ToArray();

        return spriteSheet;
    }
}
