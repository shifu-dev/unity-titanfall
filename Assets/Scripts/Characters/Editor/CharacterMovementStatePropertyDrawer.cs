using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CharacterMovementStateImpl))]
public class CharacterMovementStatePropertyDrawer : PropertyDrawer
{
    protected float height = 0;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        height = 0;

        var rect_previous = new Rect(position.x, position.y + height, position.width, EditorGUIUtility.singleLineHeight);
        height += EditorGUIUtility.singleLineHeight;

        var rect_current = new Rect(position.x, position.y + height, position.width, EditorGUIUtility.singleLineHeight);
        height += EditorGUIUtility.singleLineHeight;

        var rect_weight = new Rect(position.x, position.y + height, position.width, EditorGUIUtility.singleLineHeight);
        height += EditorGUIUtility.singleLineHeight;

        var sp_previous = property.FindPropertyRelative("m_previous");
        var sp_current = property.FindPropertyRelative("m_current");
        var sp_weight = property.FindPropertyRelative("m_weight");

        EditorGUI.TextField(rect_previous, "Previous", new CharacterMovementStateImpl((uint)sp_previous.intValue).ToString());
        EditorGUI.TextField(rect_current, "Current", new CharacterMovementStateImpl((uint)sp_current.intValue).ToString());
        EditorGUI.Slider(rect_weight, "Weight", sp_weight.floatValue, 0f, 1f);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return height;
    }
}