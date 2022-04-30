using System;
using UnityEngine;

#if UNITY_EDITOR

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

#endif

[DisallowMultipleComponent]
public class CharacterInputs : MonoBehaviour, IMovementInputs
{
    public Character Character { get => _Character; }
    public PlayerInputs PlayerInputs { get => _PlayerInputs; set => _PlayerInputs = value; }

    [NonSerialized] protected Character _Character;
    [NonSerialized] protected PlayerInputs _PlayerInputs;

    public bool IsValid { get => true; }
    public bool IsGettingInputs { get => _PlayerInputs; }
    public float MoveInputAngle { get => _PlayerInputs ? _PlayerInputs.MoveInputAngle : default; }
    public Vector3 MoveInputVector { get => _PlayerInputs ? _PlayerInputs.MoveInputVector : default; }
    public Vector3 LookInputVector { get => _PlayerInputs ? _PlayerInputs.LookInputVector : default; }
    public bool WantsToWalk { get => _PlayerInputs ? _PlayerInputs.WantsToWalk : default; }
    public bool WantsToSprint { get => _PlayerInputs ? _PlayerInputs.WantsToSprint : default; }
    public bool WantsToJump { get => _PlayerInputs ? _PlayerInputs.WantsToJump : default; }
    public bool WantsToCrouch { get => _PlayerInputs ? _PlayerInputs.WantsToCrouch : default; }
    public bool WantsToProne { get => _PlayerInputs ? _PlayerInputs.WantsToProne : default; }

    public Vector3 TotalLookInputVector { get; protected set; }

    public void Init(Character character)
    {
    }

    public void UpdateImpl()
    {
        TotalLookInputVector += LookInputVector;
    }
}