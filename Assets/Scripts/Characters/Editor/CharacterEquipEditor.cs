using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterEquip))]
public class CharacterEquipEditor : Editor
{
    new CharacterEquip target = null;

    private SerializedProperty sp_leftHand = null;
    private SerializedProperty sp_rightHand = null;

    void OnEnable()
    {
        this.target = base.target as CharacterEquip;

        sp_leftHand = serializedObject.FindProperty("m_leftHand");
        sp_rightHand = serializedObject.FindProperty("m_rightHand");
    }

    public override void OnInspectorGUI()
    {
        ShowScript();
        ShowHand(sp_leftHand, "Left Hand");
        ShowHand(sp_rightHand, "Right Hand");

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowScript()
    {
        GUI.enabled = false;

        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((CharacterEquip)target),
            typeof(CharacterEquip), false);

        GUI.enabled = true;

        EditorGUILayout.Separator();
    }

    private void ShowHand(SerializedProperty property, string label)
    {
        //////////////////////////////////////////////////////////////////
        /// collect serialized properties
        SerializedProperty sp_id = property.FindPropertyRelative("m_id");
        SerializedProperty sp_locked = property.FindPropertyRelative("locked");

        label = String.Format($"{label} ({sp_id.intValue}){(sp_locked.boolValue ? " (Locked)" : "")}");
        property.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(property.isExpanded, label);
        if (property.isExpanded)
        {
            SerializedProperty sp_current = property.FindPropertyRelative("m_current");
            SerializedProperty sp_next = property.FindPropertyRelative("m_next");

            EditorGUILayout.PropertyField(sp_current, new GUIContent("Current"));
            EditorGUILayout.PropertyField(sp_next, new GUIContent("Next"));
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}