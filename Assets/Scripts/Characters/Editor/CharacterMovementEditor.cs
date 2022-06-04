using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterMovement))]
public class CharacterMovementEditor : Editor
{
    new CharacterMovement target = null;

    bool showGroundData = false;
    bool showAirData = false;

    SerializedProperty sp_movementState;

    // Ground Stand Data
    SerializedProperty sp_groundCheckDepth;
    SerializedProperty sp_groundLayer;
    SerializedProperty sp_groundMinMoveDistance;
    SerializedProperty sp_groundStandWalkSpeed;
    SerializedProperty sp_groundStandRunSpeed;
    SerializedProperty sp_groundStandSprintSpeed;
    SerializedProperty sp_groundStandSprintLeftAngleMax;
    SerializedProperty sp_groundStandSprintRightAngleMax;
    SerializedProperty sp_groundStandJumpSpeed;
    SerializedProperty sp_groundStandStepUpPercent;
    SerializedProperty sp_groundStandStepDownPercent;
    SerializedProperty sp_groundStandSlopeUpAngle;
    SerializedProperty sp_groundStandSlopeDownAngle;
    SerializedProperty sp_groundStandMaintainVelocityOnSurface;
    SerializedProperty sp_groundStandMaintainVelocityAlongSurface;
    SerializedProperty sp_groundStandToCrouchTransition;

    // Ground Crouch Data
    SerializedProperty sp_groundCrouchWalkSpeed;
    SerializedProperty sp_groundCrouchRunSpeed;
    SerializedProperty sp_groundCrouchJumpSpeed;
    SerializedProperty sp_groundCrouchStepUpPercent;
    SerializedProperty sp_groundCrouchStepDownPercent;
    SerializedProperty sp_groundCrouchSlopeUpAngle;
    SerializedProperty sp_groundCrouchSlopeDownAngle;
    SerializedProperty sp_groundCrouchMaintainVelocityOnSurface;
    SerializedProperty sp_groundCrouchMaintainVelocityAlongSurface;
    SerializedProperty sp_groundCrouchAutoRiseToStandSprint;
    SerializedProperty sp_groundCrouchToStandTransition;

    // Ground Prone Data
    SerializedProperty sp_groundProneMoveSpeed;
    SerializedProperty sp_groundProneSlopeUpAngle;
    SerializedProperty sp_groundProneSlopeDownAngle;
    SerializedProperty sp_groundProneMaintainVelocityOnSurface;
    SerializedProperty sp_groundProneMaintainVelocityAlongSurface;
    SerializedProperty sp_groundProneAutoRiseToStandSprint;

    // Air Data
    SerializedProperty sp_airHelperSpeed;
    SerializedProperty sp_airGravityDirection;
    SerializedProperty sp_airGravityAcceleration;

    protected virtual void OnEnable()
    {
        this.target = base.target as CharacterMovement;
        this.target.OnEditorEnable();

        // Movement State
        sp_movementState = serializedObject.FindProperty("m_movementState");

        // Ground Stand Properties
        sp_groundCheckDepth = serializedObject.FindProperty("m_groundCheckDepth");
        sp_groundLayer = serializedObject.FindProperty("m_groundLayer");
        sp_groundMinMoveDistance = serializedObject.FindProperty("m_groundMinMoveDistance");
        sp_groundStandWalkSpeed = serializedObject.FindProperty("m_groundStandWalkSpeed");
        sp_groundStandRunSpeed = serializedObject.FindProperty("m_groundStandRunSpeed");
        sp_groundStandSprintSpeed = serializedObject.FindProperty("m_groundStandSprintSpeed");
        sp_groundStandSprintLeftAngleMax = serializedObject.FindProperty("m_groundStandSprintLeftAngleMax");
        sp_groundStandSprintRightAngleMax = serializedObject.FindProperty("m_groundStandSprintRightAngleMax");
        sp_groundStandJumpSpeed = serializedObject.FindProperty("m_groundStandJumpSpeed");
        sp_groundStandStepUpPercent = serializedObject.FindProperty("m_groundStandStepUpPercent");
        sp_groundStandStepDownPercent = serializedObject.FindProperty("m_groundStandStepDownPercent");
        sp_groundStandSlopeUpAngle = serializedObject.FindProperty("m_groundStandSlopeUpAngle");
        sp_groundStandSlopeDownAngle = serializedObject.FindProperty("m_groundStandSlopeDownAngle");
        sp_groundStandMaintainVelocityOnSurface = serializedObject.FindProperty("m_groundStandMaintainVelocityOnSurface");
        sp_groundStandMaintainVelocityAlongSurface = serializedObject.FindProperty("m_groundStandMaintainVelocityAlongSurface");
        sp_groundStandToCrouchTransition = serializedObject.FindProperty("m_groundStandToCrouchTransition");
        sp_groundCrouchToStandTransition = serializedObject.FindProperty("m_groundCrouchToStandTransition");

        // Ground Crouch Properties
        sp_groundCrouchWalkSpeed = serializedObject.FindProperty("m_groundCrouchWalkSpeed");
        sp_groundCrouchRunSpeed = serializedObject.FindProperty("m_groundCrouchRunSpeed");
        sp_groundCrouchJumpSpeed = serializedObject.FindProperty("m_groundCrouchJumpSpeed");
        sp_groundCrouchStepUpPercent = serializedObject.FindProperty("m_groundCrouchStepUpPercent");
        sp_groundCrouchStepDownPercent = serializedObject.FindProperty("m_groundCrouchStepDownPercent");
        sp_groundCrouchSlopeUpAngle = serializedObject.FindProperty("m_groundCrouchSlopeUpAngle");
        sp_groundCrouchSlopeDownAngle = serializedObject.FindProperty("m_groundCrouchSlopeDownAngle");
        sp_groundCrouchMaintainVelocityOnSurface = serializedObject.FindProperty("m_groundCrouchMaintainVelocityOnSurface");
        sp_groundCrouchMaintainVelocityAlongSurface = serializedObject.FindProperty("m_groundCrouchMaintainVelocityAlongSurface");
        sp_groundCrouchAutoRiseToStandSprint = serializedObject.FindProperty("m_groundCrouchAutoRiseToStandSprint");

        // Ground Prone Properties
        sp_groundProneMoveSpeed = serializedObject.FindProperty("m_groundProneMoveSpeed");
        sp_groundProneSlopeUpAngle = serializedObject.FindProperty("m_groundProneSlopeUpAngle");
        sp_groundProneSlopeDownAngle = serializedObject.FindProperty("m_groundProneSlopeDownAngle");
        sp_groundProneMaintainVelocityOnSurface = serializedObject.FindProperty("m_groundProneMaintainVelocityOnSurface");
        sp_groundProneMaintainVelocityAlongSurface = serializedObject.FindProperty("m_groundProneMaintainVelocityAlongSurface");
        sp_groundProneAutoRiseToStandSprint = serializedObject.FindProperty("m_groundProneAutoRiseToStandSprint");

        // Air Data
        sp_airHelperSpeed = serializedObject.FindProperty("m_airHelperSpeed");
        sp_airGravityDirection = serializedObject.FindProperty("m_airGravityDirection");
        sp_airGravityAcceleration = serializedObject.FindProperty("m_airGravityAcceleration");
    }

    public override void OnInspectorGUI()
    {
        ShowScript();
        ShowMovementState();
        ShowGroundData();
        ShowAirData();

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowScript()
    {
        GUI.enabled = false;

        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((CharacterMovement)target),
            typeof(CharacterMovement), false);

        GUI.enabled = true;

        EditorGUILayout.Separator();
    }

    private void ShowMovementState()
    {
        GUI.enabled = false;
        EditorGUILayout.PropertyField(sp_movementState, new GUIContent("Movement State"));
        EditorGUILayout.Toggle("Is On Ground", target.physIsOnGround);
        GUI.enabled = true;
    }

    private void ShowGroundData()
    {
        EditorGUILayout.Separator();
        showGroundData = EditorGUILayout.BeginFoldoutHeaderGroup(showGroundData, "Ground Data");
        if (showGroundData)
        {
            EditorGUILayout.PropertyField(sp_groundCheckDepth, new GUIContent("Ground Check Depth"));
            EditorGUILayout.PropertyField(sp_groundLayer, new GUIContent("Ground Layer"));
            EditorGUILayout.PropertyField(sp_groundMinMoveDistance, new GUIContent("Min Move Distance"));

            //////////////////////////////////////////////////////////////////
            /// Ground Stand Data
            //////////////////////////////////////////////////////////////////
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Ground Stand Data", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sp_groundStandWalkSpeed, new GUIContent("Walk Speed"));
            EditorGUILayout.PropertyField(sp_groundStandRunSpeed, new GUIContent("Run Speed"));
            EditorGUILayout.PropertyField(sp_groundStandSprintSpeed, new GUIContent("Sprint Speed"));
            EditorGUILayout.PropertyField(sp_groundStandSprintLeftAngleMax, new GUIContent("Sprint Left Angle Max"));
            EditorGUILayout.PropertyField(sp_groundStandSprintRightAngleMax, new GUIContent("Sprint Right Angle Max"));
            EditorGUILayout.PropertyField(sp_groundStandJumpSpeed, new GUIContent("Jump Power"));

            EditorGUILayout.PropertyField(sp_groundStandStepUpPercent, new GUIContent("Step Up Percent"));
            GUI.enabled = false;
            EditorGUILayout.FloatField("Stand Up Height", target.groundStandStepUpHeight);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(sp_groundStandStepDownPercent, new GUIContent("Step Down Percent"));
            GUI.enabled = false;
            EditorGUILayout.FloatField("Step Down Depth", target.groundStandStepDownDepth);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(sp_groundStandSlopeUpAngle, new GUIContent("Slope Up Angle"));
            EditorGUILayout.PropertyField(sp_groundStandSlopeDownAngle, new GUIContent("Slope Down Angle"));
            EditorGUILayout.PropertyField(sp_groundStandMaintainVelocityOnSurface, new GUIContent("Maintain Velocity On Surface"));
            EditorGUILayout.PropertyField(sp_groundStandMaintainVelocityAlongSurface, new GUIContent("Maintain Velocity Along Surface"));
            EditorGUILayout.PropertyField(sp_groundStandToCrouchTransition, new GUIContent("Crouch Transition"));

            //////////////////////////////////////////////////////////////////
            /// Ground Crouch Data
            //////////////////////////////////////////////////////////////////
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Ground Crouch Data", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(sp_groundCrouchWalkSpeed, new GUIContent("Walk Speed"));
            EditorGUILayout.PropertyField(sp_groundCrouchRunSpeed, new GUIContent("Run Speed"));
            EditorGUILayout.PropertyField(sp_groundCrouchJumpSpeed, new GUIContent("Jump Power"));

            EditorGUILayout.PropertyField(sp_groundCrouchStepUpPercent, new GUIContent("Step Up Percent"));
            GUI.enabled = false;
            EditorGUILayout.FloatField("Crouch Up Height", target.groundCrouchStepUpHeight);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(sp_groundCrouchStepDownPercent, new GUIContent("Step Down Percent"));
            GUI.enabled = false;
            EditorGUILayout.FloatField("Step Down Depth", target.groundCrouchStepDownDepth);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(sp_groundCrouchSlopeUpAngle, new GUIContent("Slope Up Angle"));
            EditorGUILayout.PropertyField(sp_groundCrouchSlopeDownAngle, new GUIContent("Slope Down Angle"));
            EditorGUILayout.PropertyField(sp_groundCrouchMaintainVelocityOnSurface, new GUIContent("Maintain Velocity On Surface"));
            EditorGUILayout.PropertyField(sp_groundCrouchMaintainVelocityAlongSurface, new GUIContent("Maintain Velocity Along Surface"));
            EditorGUILayout.PropertyField(sp_groundCrouchAutoRiseToStandSprint, new GUIContent("AutoRise To StandSprint"));
            EditorGUILayout.PropertyField(sp_groundCrouchToStandTransition, new GUIContent("Stand Transition"));

            //////////////////////////////////////////////////////////////////
            /// Ground Prone Data
            //////////////////////////////////////////////////////////////////
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Ground Prone Data", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sp_groundProneMoveSpeed, new GUIContent("MoveSpeed"));
            EditorGUILayout.PropertyField(sp_groundProneSlopeUpAngle, new GUIContent("Slope Up Angle"));
            EditorGUILayout.PropertyField(sp_groundProneSlopeDownAngle, new GUIContent("Slope Down Angle"));
            EditorGUILayout.PropertyField(sp_groundProneMaintainVelocityOnSurface, new GUIContent("Maintain Velocity On Surface"));
            EditorGUILayout.PropertyField(sp_groundProneMaintainVelocityAlongSurface, new GUIContent("Maintain Velocity Along Surface"));
            EditorGUILayout.PropertyField(sp_groundProneAutoRiseToStandSprint, new GUIContent("AutoRise To StandSprint"));
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void ShowAirData()
    {
        EditorGUILayout.Separator();
        showAirData = EditorGUILayout.BeginFoldoutHeaderGroup(showAirData, "Air Data");
        if (showAirData)
        {
            EditorGUILayout.PropertyField(sp_airHelperSpeed, new GUIContent("Helper Speed"));
            EditorGUILayout.PropertyField(sp_airGravityDirection, new GUIContent("Gravity Direction"));
            EditorGUILayout.PropertyField(sp_airGravityAcceleration, new GUIContent("Gravity Acceleration"));
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}