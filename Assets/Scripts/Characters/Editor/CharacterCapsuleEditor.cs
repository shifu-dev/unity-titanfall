using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterCapsule))]
public class CharacterCapsuleEditor : Editor
{
    new CharacterCapsule target = null;
    private bool showInfo = false;

    void OnEnable()
    {
        this.target = base.target as CharacterCapsule;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.ApplyModifiedProperties();
        target.PerformMove();

        ShowWorldInfo();
    }

    private void ShowWorldInfo()
    {
        EditorGUILayout.Separator();

        showInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showInfo, "Info");
        if (showInfo)
        {
            EditorGUI.BeginDisabledGroup(true);

            if (target.collider == null)
            {
                EditorGUILayout.TextField("No CapsuleCollider assigned!", EditorStyles.boldLabel);
            }
            else
            {
                EditorGUILayout.Separator();
                EditorGUILayout.Vector3Field("Position", target.position);
                EditorGUILayout.Vector3Field("Rotation", target.position);
                EditorGUILayout.Vector3Field("Scale", target.position);

                EditorGUILayout.Separator();
                EditorGUILayout.Toggle("Is Sphere Shaped", target.isSphereShaped);

                EditorGUILayout.Separator();
                GUILayout.Label("Lengths (WorldSpace)");
                EditorGUILayout.FloatField("Sphere Radius", target.radius);
                EditorGUILayout.FloatField("Total Height", target.height);
                EditorGUILayout.FloatField("Cylinder Height", target.cylinderHeight);

                EditorGUILayout.Separator();
                GUILayout.Label("Positions (WorldSpace)");
                EditorGUILayout.Vector3Field("Top", target.topPosition);
                EditorGUILayout.Vector3Field("Start Sphere", target.topSpherePosition);
                EditorGUILayout.Vector3Field("Center", target.center);
                EditorGUILayout.Vector3Field("End Sphere", target.baseSpherePosition);
                EditorGUILayout.Vector3Field("Base", target.basePosition);

                EditorGUILayout.Separator();
                GUILayout.Label("Directions (WorldSpace)");
                EditorGUILayout.Vector3Field("Forward", target.forward);
                EditorGUILayout.Vector3Field("Right", target.right);
                EditorGUILayout.Vector3Field("Up", target.up);

                EditorGUILayout.Separator();
                GUILayout.Label("Volumes (WorldSpace)");
                EditorGUILayout.FloatField("Total", target.volume);
                EditorGUILayout.FloatField("Sphere", target.sphereVolume);
                EditorGUILayout.FloatField("Cylinder", target.cylinderVolume);
            }

            EditorGUI.EndDisabledGroup();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}