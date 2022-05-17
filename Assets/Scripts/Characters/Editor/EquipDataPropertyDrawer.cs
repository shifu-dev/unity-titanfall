using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(EquipData))]
public class EquipDataPropertyDrawer : PropertyDrawer
{
    private const float lineHeight = 20f;
    private const float lineGap = 1.5f;
    private float height = 0f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        height = 0;
        EditorGUI.BeginProperty(position, label, property);

        Rect rect_foldout = new Rect(position.x, position.y, position.width, lineHeight);
        height += lineHeight;

        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true, EditorStyles.foldoutHeader);
        if (property.isExpanded)
        {
            //////////////////////////////////////////////////////////////////
            /// collect serialized properties
            SerializedProperty sp_equipableObject = property.FindPropertyRelative("equipableObject");
            SerializedProperty sp_status = property.FindPropertyRelative("status");
            SerializedProperty sp_weight = property.FindPropertyRelative("weight");
            SerializedProperty sp_equipSpeed = property.FindPropertyRelative("equipSpeed");
            SerializedProperty sp_unequipSpeed = property.FindPropertyRelative("unequipSpeed");
            SerializedProperty sp_localPositionOnEquip = property.FindPropertyRelative("localPositionOnEquip");
            SerializedProperty sp_localRotationOnEquip = property.FindPropertyRelative("localRotationOnEquip");
            SerializedProperty sp_localPositionOnUnequip = property.FindPropertyRelative("localPositionOnUnequip");
            SerializedProperty sp_localRotationOnUnequip = property.FindPropertyRelative("localRotationOnUnequip");
            SerializedProperty sp_parentOnUnequip = property.FindPropertyRelative("parentOnUnequip");

            //////////////////////////////////////////////////////////////////
            /// process rect positions
            Rect rect_equipableObject = new Rect(position.x, position.y + height, position.width, lineHeight);
            height += lineHeight + lineGap;
            Rect rect_status = new Rect(position.x, position.y + height, (position.width * .85f), lineHeight);
            Rect rect_weight = new Rect(position.x + rect_status.width - 20, position.y + height, (position.width * .2f), lineHeight);
            height += lineHeight + lineGap;
            Rect rect_equipSpeed = new Rect(position.x, position.y + height, position.width, lineHeight);
            height += lineHeight + lineGap;
            Rect rect_unequipSpeed = new Rect(position.x, position.y + height, position.width, lineHeight);
            height += lineHeight + lineGap;
            Rect rect_localPositionOnEquip = new Rect(position.x, position.y + height, position.width, lineHeight);
            height += lineHeight + lineGap;
            Rect rect_localRotationOnEquip = new Rect(position.x, position.y + height, position.width, lineHeight);
            height += lineHeight + lineGap;
            Rect rect_localPositionOnUnequip = new Rect(position.x, position.y + height, position.width, lineHeight);
            height += lineHeight + lineGap;
            Rect rect_localRotationOnUnequip = new Rect(position.x, position.y + height, position.width, lineHeight);
            height += lineHeight + lineGap;
            Rect rect_parentOnUnequip = new Rect(position.x, position.y + height, position.width, lineHeight);
            height += lineHeight + lineGap;

            //////////////////////////////////////////////////////////////////
            /// show properties

            GUI.enabled = false;
            EditorGUI.PropertyField(rect_equipableObject, sp_equipableObject, new GUIContent("Equipable"));
            EditorGUI.PropertyField(rect_status, sp_status, new GUIContent("Status"));
            EditorGUI.PropertyField(rect_weight, sp_weight, GUIContent.none);
            EditorGUI.PropertyField(rect_equipSpeed, sp_equipSpeed, new GUIContent("Equip Speed"));
            EditorGUI.PropertyField(rect_unequipSpeed, sp_unequipSpeed, new GUIContent("Unequip Speed"));
            EditorGUI.PropertyField(rect_localPositionOnEquip, sp_localPositionOnEquip, new GUIContent("Position On Equip"));
            EditorGUI.PropertyField(rect_localRotationOnEquip, sp_localRotationOnEquip, new GUIContent("Rotation On Equip"));
            EditorGUI.PropertyField(rect_localPositionOnUnequip, sp_localPositionOnUnequip, new GUIContent("Position On Unequip"));
            EditorGUI.PropertyField(rect_localRotationOnUnequip, sp_localRotationOnUnequip, new GUIContent("Rotation On Unequip"));
            EditorGUI.PropertyField(rect_parentOnUnequip, sp_parentOnUnequip, new GUIContent("Parent On Unequip"));
            GUI.enabled = true;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return height;
    }
}