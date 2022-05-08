using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterMovement))]
public class CharacterMovementEditor : Editor
{
    new CharacterMovement target = null;

    bool showGroundData = false;
    bool showAirData = false;

    // Ground Stand Data
    SerializedProperty m_GroundCheckDepth;
    SerializedProperty m_GroundMinMoveDistance;
    SerializedProperty m_GroundStandWalkSpeed;
    SerializedProperty m_GroundStandRunSpeed;
    SerializedProperty m_GroundStandSprintSpeed;
    SerializedProperty m_GroundStandSprintLeftAngleMax;
    SerializedProperty m_GroundStandSprintRightAngleMax;
    SerializedProperty m_GroundStandJumpSpeed;
    SerializedProperty m_GroundStandStepUpPercent;
    SerializedProperty m_GroundStandStepDownPercent;
    SerializedProperty m_GroundStandSlopeUpAngle;
    SerializedProperty m_GroundStandSlopeDownAngle;
    SerializedProperty m_GroundStandMaintainVelocityOnSlopes;
    SerializedProperty m_GroundStandMaintainVelocityOnWallSlides;

    // Ground Crouch Data
    SerializedProperty m_GroundCrouchWalkSpeed;
    SerializedProperty m_GroundCrouchRunSpeed;
    SerializedProperty m_GroundCrouchJumpSpeed;
    SerializedProperty m_GroundCrouchStepUpPercent;
    SerializedProperty m_GroundCrouchStepDownPercent;
    SerializedProperty m_GroundCrouchSlopeUpAngle;
    SerializedProperty m_GroundCrouchSlopeDownAngle;
    SerializedProperty m_GroundCrouchMaintainVelocityOnSlopes;
    SerializedProperty m_GroundCrouchMaintainVelocityOnWallSlides;
    SerializedProperty m_GroundCrouchAutoRiseToStandSprint;

    // Ground Prone Data
    SerializedProperty m_GroundProneMoveSpeed;
    SerializedProperty m_GroundProneSlopeUpAngle;
    SerializedProperty m_GroundProneSlopeDownAngle;
    SerializedProperty m_GroundProneMaintainVelocityOnSlopes;
    SerializedProperty m_GroundProneMaintainVelocityOnWallSlides;
    SerializedProperty m_GroundProneAutoRiseToStandSprint;

    // Air Data
    SerializedProperty m_AirHelperSpeed;
    SerializedProperty m_AirGravityDirection;
    SerializedProperty m_AirGravityAcceleration;

    protected virtual void OnEnable()
    {
        this.target = base.target as CharacterMovement;

        // Ground Stand Properties
        m_GroundCheckDepth = serializedObject.FindProperty("m_GroundCheckDepth");
        m_GroundMinMoveDistance = serializedObject.FindProperty("m_GroundMinMoveDistance");
        m_GroundStandWalkSpeed = serializedObject.FindProperty("m_GroundStandWalkSpeed");
        m_GroundStandRunSpeed = serializedObject.FindProperty("m_GroundStandRunSpeed");
        m_GroundStandSprintSpeed = serializedObject.FindProperty("m_GroundStandSprintSpeed");
        m_GroundStandSprintLeftAngleMax = serializedObject.FindProperty("m_GroundStandSprintLeftAngleMax");
        m_GroundStandSprintRightAngleMax = serializedObject.FindProperty("m_GroundStandSprintRightAngleMax");
        m_GroundStandJumpSpeed = serializedObject.FindProperty("m_GroundStandJumpSpeed");
        m_GroundStandStepUpPercent = serializedObject.FindProperty("m_GroundStandStepUpPercent");
        m_GroundStandStepDownPercent = serializedObject.FindProperty("m_GroundStandStepDownPercent");
        m_GroundStandSlopeUpAngle = serializedObject.FindProperty("m_GroundStandSlopeUpAngle");
        m_GroundStandSlopeDownAngle = serializedObject.FindProperty("m_GroundStandSlopeDownAngle");
        m_GroundStandMaintainVelocityOnSlopes = serializedObject.FindProperty("m_GroundStandMaintainVelocityOnSlopes");
        m_GroundStandMaintainVelocityOnWallSlides = serializedObject.FindProperty("m_GroundStandMaintainVelocityOnWallSlides");

        // Ground Crouch Properties
        m_GroundCrouchWalkSpeed = serializedObject.FindProperty("m_GroundCrouchWalkSpeed");
        m_GroundCrouchRunSpeed = serializedObject.FindProperty("m_GroundCrouchRunSpeed");
        m_GroundCrouchJumpSpeed = serializedObject.FindProperty("m_GroundCrouchJumpSpeed");
        m_GroundCrouchStepUpPercent = serializedObject.FindProperty("m_GroundCrouchStepUpPercent");
        m_GroundCrouchStepDownPercent = serializedObject.FindProperty("m_GroundCrouchStepDownPercent");
        m_GroundCrouchSlopeUpAngle = serializedObject.FindProperty("m_GroundCrouchSlopeUpAngle");
        m_GroundCrouchSlopeDownAngle = serializedObject.FindProperty("m_GroundCrouchSlopeDownAngle");
        m_GroundCrouchMaintainVelocityOnSlopes = serializedObject.FindProperty("m_GroundCrouchMaintainVelocityOnSlopes");
        m_GroundCrouchMaintainVelocityOnWallSlides = serializedObject.FindProperty("m_GroundCrouchMaintainVelocityOnWallSlides");
        m_GroundCrouchAutoRiseToStandSprint = serializedObject.FindProperty("m_GroundCrouchAutoRiseToStandSprint");

        // Ground Prone Properties
        m_GroundProneMoveSpeed = serializedObject.FindProperty("m_GroundProneMoveSpeed");
        m_GroundProneSlopeUpAngle = serializedObject.FindProperty("m_GroundProneSlopeUpAngle");
        m_GroundProneSlopeDownAngle = serializedObject.FindProperty("m_GroundProneSlopeDownAngle");
        m_GroundProneMaintainVelocityOnSlopes = serializedObject.FindProperty("m_GroundProneMaintainVelocityOnSlopes");
        m_GroundProneMaintainVelocityOnWallSlides = serializedObject.FindProperty("m_GroundProneMaintainVelocityOnWallSlides");
        m_GroundProneAutoRiseToStandSprint = serializedObject.FindProperty("m_GroundProneAutoRiseToStandSprint");

        // Air Data
        m_AirHelperSpeed = serializedObject.FindProperty("m_AirHelperSpeed");
        m_AirGravityDirection = serializedObject.FindProperty("m_AirGravityDirection");
        m_AirGravityAcceleration = serializedObject.FindProperty("m_AirGravityAcceleration");
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

        EditorGUILayout.EnumPopup("Movement State", target.MovementState.State);
        EditorGUILayout.Toggle("Is On Ground", target.PhysIsOnGround);

        GUI.enabled = true;
    }

    private void ShowGroundData()
    {
        EditorGUILayout.Separator();
        showGroundData = EditorGUILayout.BeginFoldoutHeaderGroup(showGroundData, "Ground Data");
        if (showGroundData)
        {
            EditorGUILayout.PropertyField(m_GroundCheckDepth, new GUIContent("Ground Check Depth"));
            EditorGUILayout.PropertyField(m_GroundMinMoveDistance, new GUIContent("Min Move Distance"));

            //////////////////////////////////////////////////////////////////
            /// Ground Stand Data
            //////////////////////////////////////////////////////////////////
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Ground Stand Data", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_GroundStandWalkSpeed, new GUIContent("Walk Speed"));
            EditorGUILayout.PropertyField(m_GroundStandRunSpeed, new GUIContent("Run Speed"));
            EditorGUILayout.PropertyField(m_GroundStandSprintSpeed, new GUIContent("Sprint Speed"));
            EditorGUILayout.PropertyField(m_GroundStandSprintLeftAngleMax, new GUIContent("Sprint Left Angle Max"));
            EditorGUILayout.PropertyField(m_GroundStandSprintRightAngleMax, new GUIContent("Sprint Right Angle Max"));
            EditorGUILayout.PropertyField(m_GroundStandJumpSpeed, new GUIContent("Jump Power"));

            EditorGUILayout.PropertyField(m_GroundStandStepUpPercent, new GUIContent("Step Up Percent"));
            // GUI.enabled = false;
            // EditorGUILayout.FloatField("Stand Up Height", target.GroundStandStepUpHeight);
            // GUI.enabled = true;

            EditorGUILayout.PropertyField(m_GroundStandStepDownPercent, new GUIContent("Step Down Percent"));
            // GUI.enabled = false;
            // EditorGUILayout.FloatField("Step Down Depth", target.GroundStandStepDownDepth);
            // GUI.enabled = true;

            EditorGUILayout.PropertyField(m_GroundStandSlopeUpAngle, new GUIContent("Slope Up Angle"));
            EditorGUILayout.PropertyField(m_GroundStandSlopeDownAngle, new GUIContent("Slope Down Angle"));
            EditorGUILayout.PropertyField(m_GroundStandMaintainVelocityOnSlopes, new GUIContent("Maintain Velocity On Slopes"));
            EditorGUILayout.PropertyField(m_GroundStandMaintainVelocityOnWallSlides, new GUIContent("Maintain Velocity On WallSlides"));

            //////////////////////////////////////////////////////////////////
            /// Ground Crouch Data
            //////////////////////////////////////////////////////////////////
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Ground Crouch Data", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(m_GroundCrouchWalkSpeed, new GUIContent("Walk Speed"));
            EditorGUILayout.PropertyField(m_GroundCrouchRunSpeed, new GUIContent("Run Speed"));
            EditorGUILayout.PropertyField(m_GroundCrouchJumpSpeed, new GUIContent("Jump Power"));

            EditorGUILayout.PropertyField(m_GroundCrouchStepUpPercent, new GUIContent("Step Up Percent"));
            // GUI.enabled = false;
            // EditorGUILayout.FloatField("Crouch Up Height", target.GroundCrouchStepUpHeight);
            // GUI.enabled = true;

            EditorGUILayout.PropertyField(m_GroundCrouchStepDownPercent, new GUIContent("Step Down Percent"));
            // GUI.enabled = false;
            // EditorGUILayout.FloatField("Step Down Depth", target.GroundCrouchStepDownDepth);
            // GUI.enabled = true;

            EditorGUILayout.PropertyField(m_GroundCrouchSlopeUpAngle, new GUIContent("Slope Up Angle"));
            EditorGUILayout.PropertyField(m_GroundCrouchSlopeDownAngle, new GUIContent("Slope Down Angle"));
            EditorGUILayout.PropertyField(m_GroundCrouchMaintainVelocityOnSlopes, new GUIContent("Maintain Velocity On Slopes"));
            EditorGUILayout.PropertyField(m_GroundCrouchMaintainVelocityOnWallSlides, new GUIContent("Maintain Velocity On WallSlides"));
            EditorGUILayout.PropertyField(m_GroundCrouchAutoRiseToStandSprint, new GUIContent("AutoRise To StandSprint"));

            //////////////////////////////////////////////////////////////////
            /// Ground Prone Data
            //////////////////////////////////////////////////////////////////
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Ground Prone Data", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_GroundProneMoveSpeed, new GUIContent("MoveSpeed"));
            EditorGUILayout.PropertyField(m_GroundProneSlopeUpAngle, new GUIContent("Slope Up Angle"));
            EditorGUILayout.PropertyField(m_GroundProneSlopeDownAngle, new GUIContent("Slope Down Angle"));
            EditorGUILayout.PropertyField(m_GroundProneMaintainVelocityOnSlopes, new GUIContent("Maintain Velocity On Slopes"));
            EditorGUILayout.PropertyField(m_GroundProneMaintainVelocityOnWallSlides, new GUIContent("Maintain Velocity On WallSlides"));
            EditorGUILayout.PropertyField(m_GroundProneAutoRiseToStandSprint, new GUIContent("AutoRise To StandSprint"));
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void ShowAirData()
    {
        EditorGUILayout.Separator();
        showAirData = EditorGUILayout.BeginFoldoutHeaderGroup(showAirData, "Air Data");
        if (showAirData)
        {
            EditorGUILayout.PropertyField(m_AirHelperSpeed, new GUIContent("Helper Speed"));
            EditorGUILayout.PropertyField(m_AirGravityDirection, new GUIContent("Gravity Direction"));
            EditorGUILayout.PropertyField(m_AirGravityAcceleration, new GUIContent("Gravity Acceleration"));
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}