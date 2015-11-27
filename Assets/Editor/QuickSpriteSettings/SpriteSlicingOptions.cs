using UnityEngine;

[System.Serializable]
public struct SpriteSlicingOptions
{
    public GridSlicingMode SlicingMode;
    public Vector2 CellSize;
    public SpriteAlignment Pivot;
    public Vector2 CustomPivot;
    public int Frames;
    const char delimeterChar = ',';

    public enum GridSlicingMode
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
            delimeterSpace, (int)SlicingMode, delimeterSpace, (int) Pivot, delimeterSpace,
            CustomPivot.x, delimeterSpace, CustomPivot.y);
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
            if (entries.Length >= 7)
            {
                options.SlicingMode = (GridSlicingMode) int.Parse (entries[3]);
                options.Pivot = (SpriteAlignment) int.Parse (entries[4]);
                options.CustomPivot = new Vector2 (int.Parse (entries[5]), int.Parse (entries[6]));
            }
        }
       
        return options;
    }
}