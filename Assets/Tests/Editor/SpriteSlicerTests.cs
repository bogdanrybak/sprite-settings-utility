using UnityEngine;
using UnityEditor;
using NUnit.Framework;

[TestFixture]
[Category("SpriteSlicer Tests")]
public class SpriteSlicerTests {

    Texture2D SampleTexture;
    [SetUp]
    public void Setup ()
    {
        SampleTexture = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Tests/Editor/SampleTexture.png");
    }
    
	[Test]
	public void CorrectlySlices ()
	{
        string pathToExpected = "Assets/Tests/Editor/Expected86x86.png";
        var expectedImporter = AssetImporter.GetAtPath(pathToExpected) as TextureImporter;
        
        SpriteMetaData[] createdSpritesheet = SpriteSlicer.CreateSpriteSheetForTexture (SampleTexture, new Vector2 (86, 86), 
            SpriteAlignment.Center);
        
        Assert.AreEqual (expectedImporter.spritesheet.Length, createdSpritesheet.Length, 
            "Number of Slices not equal to NumExpectedSlices"); 
	}
}
