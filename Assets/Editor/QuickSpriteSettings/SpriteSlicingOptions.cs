using UnityEngine;
using UnityEditor;
using System.Collections;

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

    [CustomPropertyDrawer(typeof(SpriteSlicingOptions))]
    public class SpriteSlicingOptionsDrawer : PropertyDrawer
    {
        private float lineHeight = EditorGUIUtility.singleLineHeight;
        private float padding = 2.0f;
        float totalHeight;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var pos = position;
            float startingY = pos.y;
            
            pos = CalculateNextRect (pos);
            EditorGUI.LabelField (pos, property.displayName, EditorStyles.boldLabel);
            
            var slicingModeProperty = property.FindPropertyRelative ("SlicingMode");
            pos = CalculateNextRect (pos);
            var vector2Height = lineHeight * 2.0f;            
            EditorGUI.PropertyField (pos, slicingModeProperty);
            
            var cellSizeProperty = property.FindPropertyRelative ("CellSize");
            pos = CalculateNextRect (pos);
            EditorGUI.PropertyField (pos, cellSizeProperty);
            // Leave space for shrinking windows when Vectors collapse
            pos = CalculateNextRect (pos);
            
            var pivotProperty = property.FindPropertyRelative ("Pivot");
            pos = CalculateNextRect (pos);
            EditorGUI.PropertyField (pos, pivotProperty);
            
            bool enableCustomPivot = pivotProperty.intValue == (int) SpriteAlignment.Custom;
            EditorGUI.BeginDisabledGroup (!enableCustomPivot);
            var customPivotProperty = property.FindPropertyRelative ("CustomPivot");
            pos = CalculateNextRect (pos);
            EditorGUI.PropertyField (pos, customPivotProperty);
            // Leave space for shrinking windows when Vectors collapse
            pos = CalculateNextRect (pos);
            EditorGUI.EndDisabledGroup ();

            totalHeight = pos.y + lineHeight - startingY;
        }
        
        Rect CalculateNextRect (Rect previousRect)
        {
            return new Rect(previousRect.x, previousRect.y + lineHeight + padding, previousRect.width, lineHeight);
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return totalHeight;
        }
    }
