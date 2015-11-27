using UnityEngine;
using UnityEditor;
using NUnit.Framework;

[TestFixture]
[Category("SpriteSlicer Tests")]
public class SpriteSlicerTests {

    const string pathToSampleTexture = "Assets/Tests/Editor/SpriteSlicerTests/SampleTexture.png";
    const string pathToExpected = "Assets/Tests/Editor/SpriteSlicerTests/Expected86x86.png";
    Texture2D SampleTexture;
    [SetUp]
    public void Setup ()
    {
        SampleTexture = AssetDatabase.LoadAssetAtPath<Texture2D> (pathToSampleTexture);
    }
    
	[Test]
	public void CorrectlySlices ()
	{
        var expectedImporter = AssetImporter.GetAtPath(pathToExpected) as TextureImporter;
        var expectedSpritesheet = expectedImporter.spritesheet;
        
        SpriteMetaData[] createdSpritesheet = SpriteSlicer.CreateSpriteSheetForTexture (SampleTexture, new Vector2 (86, 86), 
            SpriteAlignment.Center);
        
        Assert.AreEqual (expectedSpritesheet.Length, createdSpritesheet.Length, 
            "Number of Slices not equal to NumExpectedSlices"); 
	}
	[Test]
	public void SlicesTopLeftToBottomRight ()
	{
        var expectedImporter = AssetImporter.GetAtPath(pathToExpected) as TextureImporter;
        var expectedSpritesheet = expectedImporter.spritesheet;
        var firstRect = expectedSpritesheet[0].rect;
        var lastRect = expectedSpritesheet[expectedSpritesheet.Length -1].rect;
        
        SpriteMetaData[] createdSpritesheet = SpriteSlicer.CreateSpriteSheetForTexture (SampleTexture, new Vector2 (86, 86), 
            SpriteAlignment.Center);
        var firstSlicedRect = createdSpritesheet[0].rect;
        var lastSlicedRect = createdSpritesheet[createdSpritesheet.Length -1].rect;
        
        Assert.AreEqual (firstRect, firstSlicedRect,
            "First sprite rect does not match expected. ExpectedRect: " + firstRect,
            " Actual rect:" + firstSlicedRect);
        Assert.AreEqual (lastRect, lastSlicedRect,
            "Last sprite rect does not match expected. ExpectedRect: " + lastRect,
            " Actual rect:" + lastSlicedRect);
	}
}
