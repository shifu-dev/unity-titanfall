using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterCapsule))]
public class CharacterCapsuleEditor : Editor
{
    new CharacterCapsule target = null;

    SerializedProperty m_CapsuleCollider;
    SerializedProperty m_Position;
    SerializedProperty m_LocalRotation;
    SerializedProperty m_LocalScale;
    SerializedProperty m_Center;
    SerializedProperty m_Direction;
    SerializedProperty m_Height;
    SerializedProperty m_Radius;
    SerializedProperty m_SkinWidth;
    SerializedProperty m_LayerMask;
    SerializedProperty m_TriggerQuery;

    void OnEnable()
    {
        this.target = base.target as CharacterCapsule;

        m_CapsuleCollider = serializedObject.FindProperty("m_CapsuleCollider");
        m_Position = serializedObject.FindProperty("m_Position");
        m_LocalRotation = serializedObject.FindProperty("m_LocalRotation");
        m_LocalScale = serializedObject.FindProperty("m_LocalScale");
        m_Center = serializedObject.FindProperty("m_Center");
        m_Direction = serializedObject.FindProperty("m_Direction");
        m_Height = serializedObject.FindProperty("m_Height");
        m_Radius = serializedObject.FindProperty("m_Radius");
        m_SkinWidth = serializedObject.FindProperty("m_SkinWidth");
        m_LayerMask = serializedObject.FindProperty("m_LayerMask");
        m_TriggerQuery = serializedObject.FindProperty("m_TriggerQuery");
    }

    public override void OnInspectorGUI()
    {
        ShowScript();
        ShowOptions();

        serializedObject.ApplyModifiedProperties();
        target.OnUpdateCharacter();

        ShowWorldInfo();
    }

    private void ShowScript()
    {
        GUI.enabled = false;

        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((CharacterCapsule)target),
            typeof(CharacterCapsule), false);

        GUI.enabled = true;

        EditorGUILayout.Separator();
    }

    private void ShowOptions()
    {
        EditorGUILayout.PropertyField(m_CapsuleCollider, new GUIContent("CapsuleCollider"));
        EditorGUILayout.PropertyField(m_Center, new GUIContent("Center"));
        EditorGUILayout.PropertyField(m_Direction, new GUIContent("Direction"));
        EditorGUILayout.PropertyField(m_Height, new GUIContent("Height"));
        EditorGUILayout.PropertyField(m_Radius, new GUIContent("Radius"));
        EditorGUILayout.PropertyField(m_SkinWidth, new GUIContent("Skin Width"));
        EditorGUILayout.PropertyField(m_LayerMask, new GUIContent("Layer Mask"));
        EditorGUILayout.PropertyField(m_TriggerQuery, new GUIContent("Trigger Query"));
    }

    private void ShowWorldInfo()
    {
        EditorGUILayout.Separator();
        EditorGUI.BeginDisabledGroup(true);

        if (target.Capsule == null)
        {
            EditorGUILayout.TextField("No CapsuleCollider assigned!", EditorStyles.boldLabel);
            return;
        }

        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(m_Position, new GUIContent("Position"));
        EditorGUILayout.PropertyField(m_LocalRotation, new GUIContent("Rotation"));
        EditorGUILayout.PropertyField(m_LocalScale, new GUIContent("Scale"));

        EditorGUILayout.Separator();
        EditorGUILayout.Toggle("Is Sphere Shaped", target.IsSphereShaped);

        EditorGUILayout.Separator();
        GUILayout.Label("Lengths (WorldSpace)");
        EditorGUILayout.FloatField("Sphere Radius", target.GetRadius);
        EditorGUILayout.FloatField("Total Height", target.GetHeight);
        EditorGUILayout.FloatField("Cylinder Height", target.GetCylinderHeight);

        EditorGUILayout.Separator();
        GUILayout.Label("Positions (WorldSpace)");
        EditorGUILayout.Vector3Field("Top", target.GetTopPosition);
        EditorGUILayout.Vector3Field("Start Sphere", target.GetTopSpherePosition);
        EditorGUILayout.Vector3Field("Center", target.GetCenter);
        EditorGUILayout.Vector3Field("End Sphere", target.GetBaseSpherePosition);
        EditorGUILayout.Vector3Field("Base", target.GetBasePosition);

        EditorGUILayout.Separator();
        GUILayout.Label("Directions (WorldSpace)");
        EditorGUILayout.Vector3Field("Forward", target.GetForwardVector);
        EditorGUILayout.Vector3Field("Right", target.GetRightVector);
        EditorGUILayout.Vector3Field("Up", target.GetUpVector);

        EditorGUILayout.Separator();
        GUILayout.Label("Volumes (WorldSpace)");
        EditorGUILayout.FloatField("Total", target.GetVolume);
        EditorGUILayout.FloatField("Sphere", target.GetSphereVolume);
        EditorGUILayout.FloatField("Cylinder", target.GetCylinderVolume);

        EditorGUI.EndDisabledGroup();
    }
}