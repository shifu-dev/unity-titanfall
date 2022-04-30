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
        if (target.CapsuleCollider == null)
        {
            EditorGUILayout.TextField("No CapsuleCollider assigned!", EditorStyles.boldLabel);
            return;
        }

        EditorGUILayout.Separator();
        EditorGUILayout.Toggle("Is Sphere Shaped", target.IsSphereShaped);

        EditorGUILayout.Separator();
        GUILayout.Label("Lengths (WorldSpace)");
        EditorGUILayout.FloatField("Sphere Radius", target.GetWorldRadius);
        EditorGUILayout.FloatField("Total Height", target.GetWorldHeight);
        EditorGUILayout.FloatField("Cylinder Height", target.GetWorldCylinderHeight);

        EditorGUILayout.Separator();
        GUILayout.Label("Positions (WorldSpace)");
        EditorGUILayout.Vector3Field("Top", target.GetWorldTopPosition);
        EditorGUILayout.Vector3Field("Start Sphere", target.GetWorldStartSpherePosition);
        EditorGUILayout.Vector3Field("Center", target.GetWorldCenter);
        EditorGUILayout.Vector3Field("End Sphere", target.GetWorldEndSpherePosition);
        EditorGUILayout.Vector3Field("Base", target.GetWorldBasePosition);

        EditorGUILayout.Separator();
        GUILayout.Label("Directions (WorldSpace)");
        EditorGUILayout.Vector3Field("Forward", target.GetWorldForwardVector);
        EditorGUILayout.Vector3Field("Right", target.GetWorldRightVector);
        EditorGUILayout.Vector3Field("Up", target.GetWorldUpVector);

        EditorGUILayout.Separator();
        GUILayout.Label("Volumes (WorldSpace)");
        EditorGUILayout.FloatField("Total", target.GetWorldVolume);
        EditorGUILayout.FloatField("Sphere", target.GetWorldSphereVolume);
        EditorGUILayout.FloatField("Cylinder", target.GetWorldCylinderVolume);

        EditorGUI.EndDisabledGroup();
    }
}