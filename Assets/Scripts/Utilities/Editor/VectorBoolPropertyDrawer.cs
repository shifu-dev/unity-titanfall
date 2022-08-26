using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(VectorBool))]
public class VectorBoolPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float field_width = (position.width - EditorGUIUtility.labelWidth) / 3f;

        Rect rect_l = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
        Rect rect_x = new Rect(rect_l.xMax + 2f, position.y, field_width, position.height);
        Rect rect_y = new Rect(rect_x.xMax, position.y, field_width, position.height);
        Rect rect_z = new Rect(rect_y.xMax, position.y, field_width, position.height);

        SerializedProperty sp_x = property.FindPropertyRelative("x");
        SerializedProperty sp_y = property.FindPropertyRelative("y");
        SerializedProperty sp_z = property.FindPropertyRelative("z");

        EditorGUI.LabelField(rect_l, label);
        EditorGUI.PropertyField(rect_x, sp_x, GUIContent.none);
        EditorGUI.PropertyField(rect_y, sp_y, GUIContent.none);
        EditorGUI.PropertyField(rect_z, sp_z, GUIContent.none);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}