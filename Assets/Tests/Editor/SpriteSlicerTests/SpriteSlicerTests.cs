using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using Staple.EditorScripts;

[TestFixture]
[Category("SpriteSlicer Tests")]
public class SpriteSlicerTests {

    const string pathToSampleTexture = "Assets/Tests/Editor/SpriteSlicerTests/SampleTexture.png";
    const string pathToExpectedNormal = "Assets/Tests/Editor/SpriteSlicerTests/Expected86x86.png";
    const string pathToExpectedOffset = "Assets/Tests/Editor/SpriteSlicerTests/Expected80x80.png";
    Texture2D SampleTexture;
    [SetUp]
    public void Setup ()
    {
        SampleTexture = AssetDatabase.LoadAssetAtPath<Texture2D> (pathToSampleTexture);
    }
    
	[Test]
	public void CorrectlySlices ()
	{
        var expectedImporter = AssetImporter.GetAtPath(pathToExpectedOffset) as TextureImporter;
        var expectedSpritesheet = expectedImporter.spritesheet;
        
        var slicingOptions = new SpriteSlicingOptions ();
        slicingOptions.CellSize = new Vector2 (86, 86);
        slicingOptions.ImportMode = UnityEditor.SpriteImportMode.Multiple;
        slicingOptions.Pivot = SpriteAlignment.Center;
        slicingOptions.SlicingMethod = SpriteSlicingOptions.GridSlicingMethod.SliceAll;
        SpriteMetaData[] createdSpritesheet = SpriteSlicer.CreateSpriteSheetForTexture (SampleTexture, slicingOptions);
        
        Assert.AreEqual (expectedSpritesheet.Length, createdSpritesheet.Length, 
            "Number of Slices not equal to NumExpectedSlices"); 
	}
	[Test]
	public void SlicesTopLeftToBottomRight ()
	{
        var expectedImporter = AssetImporter.GetAtPath(pathToExpectedNormal) as TextureImporter;
        var expectedSpritesheet = expectedImporter.spritesheet;
        
        var slicingOptions = new SpriteSlicingOptions ();
        slicingOptions.CellSize = new Vector2 (86, 86);
        slicingOptions.ImportMode = UnityEditor.SpriteImportMode.Multiple;
        slicingOptions.Pivot = SpriteAlignment.Center;
        slicingOptions.SlicingMethod = SpriteSlicingOptions.GridSlicingMethod.SliceAll;
        SpriteMetaData[] createdSpritesheet = SpriteSlicer.CreateSpriteSheetForTexture (SampleTexture, slicingOptions);
        
        CompareFirstAndLastRects (expectedSpritesheet, createdSpritesheet);
	}
	[Test]
	public void SlicesAwkwardSize ()
	{
        var expectedImporter = AssetImporter.GetAtPath(pathToExpectedOffset) as TextureImporter;
        var expectedSpritesheet = expectedImporter.spritesheet;
        
        var slicingOptions = new SpriteSlicingOptions ();
        slicingOptions.CellSize = new Vector2 (80, 80);
        slicingOptions.ImportMode = UnityEditor.SpriteImportMode.Multiple;
        slicingOptions.Pivot = SpriteAlignment.Center;
        slicingOptions.SlicingMethod = SpriteSlicingOptions.GridSlicingMethod.SliceAll;
        SpriteMetaData[] createdSpritesheet = SpriteSlicer.CreateSpriteSheetForTexture (SampleTexture, slicingOptions);
        
        CompareFirstAndLastRects (expectedSpritesheet, createdSpritesheet);
	}
    
    void CompareFirstAndLastRects (SpriteMetaData[] spritesheetA, SpriteMetaData[] spritesheetB)
    {
        var firstRect = spritesheetA[0].rect;
        var lastRect = spritesheetA[spritesheetA.Length -1].rect;
        
        var firstSlicedRect = spritesheetB[0].rect;
        var lastSlicedRect = spritesheetB[spritesheetB.Length -1].rect;
        
        Assert.AreEqual (firstRect, firstSlicedRect,
            "First sprite rect does not match expected. ExpectedRect: " + firstRect,
            " Actual rect:" + firstSlicedRect);
        Assert.AreEqual (lastRect, lastSlicedRect,
            "Last sprite rect does not match expected. ExpectedRect: " + lastRect,
            " Actual rect:" + lastSlicedRect);
    }
}
