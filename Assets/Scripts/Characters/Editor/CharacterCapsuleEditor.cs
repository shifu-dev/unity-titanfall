using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterCapsule))]
public class CharacterCapsuleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CharacterCapsule target = (CharacterCapsule)this.target;
        EditorGUI.BeginDisabledGroup(true);

        EditorGUILayout.Separator();
        if (target.Capsule == null)
        {
            EditorGUILayout.TextField("No CapsuleCollider assigned!", EditorStyles.boldLabel);
            return;
        }

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