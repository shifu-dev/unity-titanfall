using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CharacterMovementState))]
public class CharacterMovementStatePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var rect_state = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        var sp_state = property.FindPropertyRelative("state");
        EditorGUI.PropertyField(rect_state, sp_state, label);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}