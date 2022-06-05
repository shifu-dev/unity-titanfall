using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterMovement))]
public class CharacterMovementEditor : Editor
{
    private new CharacterMovement target;
    private SerializedProperty sp_movementState;

    protected virtual void OnEnable()
    {
        this.target = base.target as CharacterMovement;
        CollectProperties();
    }

    // public override void OnInspectorGUI()
    // {
    //     ShowScript();
    //     ShowMovementState();
    // }

    protected virtual void CollectProperties()
    {
        sp_movementState = serializedObject.FindProperty("m_movementState");
    }

    protected virtual void ShowScript()
    {
        GUI.enabled = false;

        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((CharacterMovement)target),
            typeof(CharacterMovement), false);

        GUI.enabled = true;

        EditorGUILayout.Separator();
    }

    protected virtual void ShowMovementState()
    {
        GUI.enabled = false;
        EditorGUILayout.PropertyField(sp_movementState, new GUIContent("Movement State"));
        EditorGUILayout.Toggle("Is On Ground", target.physIsOnGround);
        GUI.enabled = true;
    }
}