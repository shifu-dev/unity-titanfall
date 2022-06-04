using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Transition))]
public class TransitionPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var rect_curve = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        var sp_curve = property.FindPropertyRelative("curve");
        if (sp_curve.animationCurveValue == null)
        {
            sp_curve.animationCurveValue = new AnimationCurve();
        }

        EditorGUI.PropertyField(rect_curve, sp_curve, label);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}