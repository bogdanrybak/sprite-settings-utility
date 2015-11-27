using UnityEngine;

[System.Serializable]
public struct SpriteSlicingOptions
{
    public GridSlicingMode SlicingMode;
    public Vector2 CellSize;
    public SpriteAlignment Pivot;
    public Vector2 CustomPivot;

    public enum GridSlicingMode
    {
        Default,
        SliceAll,
    };
}