using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterMovement))]
public class CharacterMovementEditor : Editor
{
    private SerializedProperty IsOnGround;
    private SerializedProperty CurrentMoveSpeed;
    private SerializedProperty GroundCheckDepth;
    private SerializedProperty GroundStandWalkSpeed;
    private SerializedProperty GroundStandRunSpeed;
    private SerializedProperty GroundStandSprintSpeed;
    private SerializedProperty GroundStandSprintLeftAngleMax;
    private SerializedProperty GroundStandSprintRightAngleMax;
    private SerializedProperty GroundStandJumpSpeed;
    private SerializedProperty GroundStandStepUpHeight;
    private SerializedProperty GroundStandStepDownHeight;
    private SerializedProperty GroundStandWalkableAngleZ;
    private SerializedProperty GroundCrouchWalkSpeed;
    private SerializedProperty GroundCrouchRunSpeed;
    private SerializedProperty GroundCrouchJumpSpeed;
    private SerializedProperty GroundCrouchStepUpHeight;
    private SerializedProperty GroundCrouchStepDownHeight;
    private SerializedProperty GroundCrouchWalkableAngleZ;
    private SerializedProperty GroundCrouchAutoRiseToStandSprint;
    private SerializedProperty GroundProneMoveSpeed;
    private SerializedProperty GroundProneAutoRiseToStandSprint;

    protected virtual void OnEnable()
    {
        CurrentMoveSpeed = serializedObject.FindProperty("CurrentMoveSpeed");
        GroundCheckDepth = serializedObject.FindProperty("GroundCheckDepth");
        GroundStandWalkSpeed = serializedObject.FindProperty("GroundStandWalkSpeed");
        GroundStandRunSpeed = serializedObject.FindProperty("GroundStandRunSpeed");
        GroundStandSprintSpeed = serializedObject.FindProperty("GroundStandSprintSpeed");
        GroundStandSprintLeftAngleMax = serializedObject.FindProperty("GroundStandSprintLeftAngleMax");
        GroundStandSprintRightAngleMax = serializedObject.FindProperty("GroundStandSprintRightAngleMax");
        GroundStandJumpSpeed = serializedObject.FindProperty("GroundStandJumpSpeed");
        GroundStandStepUpHeight = serializedObject.FindProperty("GroundStandStepUpHeight");
        GroundStandStepDownHeight = serializedObject.FindProperty("GroundStandStepDownHeight");
        GroundStandWalkableAngleZ = serializedObject.FindProperty("GroundStandWalkableAngleZ");
        GroundCrouchWalkSpeed = serializedObject.FindProperty("GroundCrouchWalkSpeed");
        GroundCrouchRunSpeed = serializedObject.FindProperty("GroundCrouchRunSpeed");
        GroundCrouchJumpSpeed = serializedObject.FindProperty("GroundCrouchJumpSpeed");
        GroundCrouchStepUpHeight = serializedObject.FindProperty("GroundCrouchStepUpHeight");
        GroundCrouchStepDownHeight = serializedObject.FindProperty("GroundCrouchStepDownHeight");
        GroundCrouchWalkableAngleZ = serializedObject.FindProperty("GroundCrouchWalkableAngleZ");
        GroundCrouchAutoRiseToStandSprint = serializedObject.FindProperty("GroundCrouchAutoRiseToStandSprint");
        GroundProneMoveSpeed = serializedObject.FindProperty("GroundProneMoveSpeed");
        GroundProneAutoRiseToStandSprint = serializedObject.FindProperty("GroundProneAutoRiseToStandSprint");
    }

    public override void OnInspectorGUI()
    {
        CharacterMovement target = this.target as CharacterMovement;

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Ground Movement Stand", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(GroundStandWalkSpeed, new GUIContent("Walk Speed"));
        EditorGUILayout.PropertyField(GroundStandRunSpeed, new GUIContent("Run Speed"));
        EditorGUILayout.PropertyField(GroundStandSprintSpeed, new GUIContent("Sprint Speed"));
        EditorGUILayout.PropertyField(GroundStandSprintLeftAngleMax, new GUIContent("Sprint LeftAngle"));
        EditorGUILayout.PropertyField(GroundStandSprintRightAngleMax, new GUIContent("Sprint RightAngle"));
        EditorGUILayout.PropertyField(GroundStandJumpSpeed, new GUIContent("Jump Power"));
        EditorGUILayout.PropertyField(GroundStandStepUpHeight, new GUIContent("Step Up Height"));
        EditorGUILayout.PropertyField(GroundStandStepDownHeight, new GUIContent("Step Down Height"));
        EditorGUILayout.PropertyField(GroundStandWalkableAngleZ, new GUIContent("Move Angle Z"));

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Ground Movement Crouch", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(GroundCrouchWalkSpeed, new GUIContent("Walk Speed"));
        EditorGUILayout.PropertyField(GroundCrouchRunSpeed, new GUIContent("Run Speed"));
        EditorGUILayout.PropertyField(GroundCrouchJumpSpeed, new GUIContent("Jump Speed"));
        EditorGUILayout.PropertyField(GroundCrouchStepUpHeight, new GUIContent("Step Up Height"));
        EditorGUILayout.PropertyField(GroundCrouchStepDownHeight, new GUIContent("Step Down Height"));
        EditorGUILayout.PropertyField(GroundCrouchWalkableAngleZ, new GUIContent("Move Angle Z"));
        EditorGUILayout.PropertyField(GroundCrouchAutoRiseToStandSprint, new GUIContent("AutoRise To StandSprint"));

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Ground Movement Prone", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(GroundProneMoveSpeed, new GUIContent("Move Speed"));
        EditorGUILayout.PropertyField(GroundProneAutoRiseToStandSprint, new GUIContent("AutoRise To StandSprint"));

        serializedObject.ApplyModifiedProperties();
    }
}