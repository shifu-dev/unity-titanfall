using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(weight))]
public class WeightPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Rect rect_value = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        SerializedProperty sp_value = property.FindPropertyRelative("m_value");
        EditorGUI.PropertyField(rect_value, sp_value, new GUIContent(label));
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}