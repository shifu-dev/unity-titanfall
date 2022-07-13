using System;
using UnityEngine;
using UnityEditor;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class Vector3RangeAttribute : PropertyAttribute
{
    public Vector3 min { get; protected set; }
    public Vector3 max { get; protected set; }

    public Vector3RangeAttribute(float min, float max)
    {
        this.min = new Vector3(min, min, min);
        this.max = new Vector3(max, max, max);

        Normalize();
    }

    public Vector3RangeAttribute(float min_x, float max_x, float min_y, float max_y, float min_z, float max_z)
    {
        min = new Vector3(min_x, min_y, min_z);
        max = new Vector3(max_x, max_y, max_z);

        Normalize();
    }

    public Vector3RangeAttribute(Vector3 min, Vector3 max)
    {
        this.min = min;
        this.max = max;

        Normalize();
    }

    private void Normalize()
    {
    }
}

[CustomPropertyDrawer(typeof(Vector3RangeAttribute))]
public class Vector3RangeEditor : PropertyDrawer
{
    protected float height = 0f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        height = 0f;

        Vector3RangeAttribute attribute = this.attribute as Vector3RangeAttribute;
        if (attribute == null)
        {
            // if custom attribute not found we don't do custom logic
            base.OnGUI(position, property, label);
            height = base.GetPropertyHeight(property, label);

            return;
        }

        if (property.propertyType != SerializedPropertyType.Vector3)
        {
            // if field type doesn't match the desired type,
            // show the error msg

            Rect r_msg = new Rect(position.x, position.y + height, position.width, EditorGUIUtility.singleLineHeight);
            height += EditorGUIUtility.singleLineHeight;

            GUI.enabled = false;
            EditorGUI.TextArea(r_msg, "Vector3Range can only be used on Vector3 fields");
            GUI.enabled = true;

            return;
        }

        height = 0f;
        Rect r_x = new Rect(position.x, position.y + height, position.width, EditorGUIUtility.singleLineHeight);
        height += EditorGUIUtility.singleLineHeight;

        height += 2f;
        Rect r_y = new Rect(position.x, position.y + height, position.width, EditorGUIUtility.singleLineHeight);
        height += EditorGUIUtility.singleLineHeight;

        height += 2f;
        Rect r_z = new Rect(position.x, position.y + height, position.width, EditorGUIUtility.singleLineHeight);
        height += EditorGUIUtility.singleLineHeight;

        SerializedProperty sp_x = property.FindPropertyRelative("x");
        SerializedProperty sp_y = property.FindPropertyRelative("y");
        SerializedProperty sp_z = property.FindPropertyRelative("z");

        Vector3 min = attribute.min;
        Vector3 max = attribute.max;
        string name = label.text;

        EditorGUI.Slider(r_x, sp_x, min.x, max.x, name + " X");
        EditorGUI.Slider(r_y, sp_y, min.y, max.y, name + " Y");
        EditorGUI.Slider(r_z, sp_z, min.z, max.z, name + " Z");

        property.serializedObject.ApplyModifiedProperties();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return height;
    }
}