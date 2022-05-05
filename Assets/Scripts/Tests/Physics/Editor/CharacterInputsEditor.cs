using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterInputs))]
public class CharacterInputsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CharacterInputs target = this.target as CharacterInputs;

        EditorGUI.BeginDisabledGroup(true);

        EditorGUILayout.Toggle("Is Valid", target.IsValid);
        EditorGUILayout.Toggle("Is Getting Inputs", target.IsGettingInputs);

        EditorGUILayout.Space();
        EditorGUILayout.Vector3Field("Raw Move Input", target.MoveInputVector);
        EditorGUILayout.FloatField("Raw Move Input Angle", target.MoveInputAngle);

        EditorGUILayout.Space();
        EditorGUILayout.Vector3Field("Raw Look Input", target.LookInputVector);
        EditorGUILayout.Vector3Field("Processed Look Input", target.TotalLookInputVector);

        EditorGUILayout.Space();
        EditorGUILayout.Toggle("Wants To Walk", target.WantsToWalk);
        EditorGUILayout.Toggle("Wants To Sprint", target.WantsToSprint);
        EditorGUILayout.Toggle("Wants To Jump", target.WantsToJump);
        EditorGUILayout.Toggle("Wants To Crouch", target.WantsToCrouch);
        EditorGUILayout.Toggle("Wants To Prone", target.WantsToProne);

        EditorGUI.EndDisabledGroup();
    }
}