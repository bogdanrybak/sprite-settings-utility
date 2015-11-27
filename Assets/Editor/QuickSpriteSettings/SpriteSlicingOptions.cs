using UnityEngine;

[System.Serializable]
public struct SpriteSlicingOptions
{
    public GridSlicingMethod GridSlicing;
    public Vector2 CellSize;
    public SpriteAlignment Pivot;
    public Vector2 CustomPivot;
    public int Frames;
    public UnityEditor.SpriteImportMode SpriteImportMode;
    const char delimeterChar = ',';

    public enum GridSlicingMethod
    {
        Bogdan,
        SliceAll,
    };
    
    public string ToDisplayString ()
    {
        if (CellSize == Vector2.zero) 
        {
            return "Default Settings";
        }
        
        var displayString = string.Concat ("Cell Size: ", CellSize.x, ",", CellSize.y, " Frames: ", Frames );
        return displayString;
    }
    
    public override string ToString ()
    {
        string delimeterSpace = delimeterChar + " ";
        string serialized = string.Concat (CellSize.x, delimeterSpace, CellSize.y, delimeterSpace, Frames,
            delimeterSpace, (int) SpriteImportMode, delimeterSpace, (int)GridSlicing, delimeterSpace,
            (int) Pivot, delimeterSpace, CustomPivot.x, delimeterSpace, CustomPivot.y);
        return serialized;
    }
    
    public static SpriteSlicingOptions FromString (string serialized)
    {
        var options = new SpriteSlicingOptions ();
        string[] entries = serialized.Split (delimeterChar);
        options.CellSize = new Vector2 (int.Parse (entries[0]), int.Parse (entries[1]));
        if (entries.Length >= 3)
        {
            int.TryParse (entries[2], out options.Frames);
            if (entries.Length >= 8)
            {
                options.SpriteImportMode = (UnityEditor.SpriteImportMode) int.Parse (entries[3]);
                options.GridSlicing = (GridSlicingMethod) int.Parse (entries[4]);
                options.Pivot = (SpriteAlignment) int.Parse (entries[5]);
                options.CustomPivot = new Vector2 (int.Parse (entries[6]), int.Parse (entries[7]));
            }
        }
       
        return options;
    }
}