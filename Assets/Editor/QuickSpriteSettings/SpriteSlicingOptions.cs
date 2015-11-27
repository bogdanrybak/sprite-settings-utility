using UnityEngine;

[System.Serializable]
public struct SpriteSlicingOptions
{
    public GridSlicingMode SlicingMode;
    public Vector2 CellSize;
    public SpriteAlignment Pivot;
    public Vector2 CustomPivot;
    public int Frames;
    const char seperationToken = ',';

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
        string serialized = string.Concat (CellSize.x, seperationToken, CellSize.y, seperationToken, Frames,
            (int)SlicingMode, seperationToken, (int) Pivot, seperationToken, CustomPivot.x, seperationToken,
            CustomPivot.y);
        return serialized;
    }
    
    public static SpriteSlicingOptions FromString (string serialized)
    {
        var options = new SpriteSlicingOptions ();
        string[] entries = serialized.Split (seperationToken);
        options.CellSize = new Vector2 (int.Parse (entries[0]), int.Parse (entries[1]));
        if (entries.Length >= 3)
        {
            int.TryParse (entries[2], out options.Frames);
            if (entries.Length >= 4)
            {
                options.SlicingMode = (GridSlicingMode) int.Parse (entries[3]);
                options.Pivot = (SpriteAlignment) int.Parse (entries[4]);
                options.CustomPivot = new Vector2 (int.Parse (entries[5]), int.Parse (entries[6]));
            }
        }
       
        return options;
    }
}