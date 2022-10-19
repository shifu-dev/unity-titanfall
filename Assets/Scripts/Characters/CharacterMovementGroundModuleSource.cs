using UnityEngine;

[CreateAssetMenu(menuName = MENU_PATH + "Ground Module")]
public class CharacterMovementGroundModuleSource : CharacterMovementModuleSource
{
    [SerializeField] public float checkDepth;
    [SerializeField] public LayerMask groundLayer;
    [SerializeField] public float minMoveDistance;

    [SerializeField] public float standIdleSpeed;
    [SerializeField] public float standIdleAcceleration;
    [SerializeField] public float standWalkSpeed;
    [SerializeField] public float standWalkAcceleration;
    [SerializeField] public float standRunSpeed;
    [SerializeField] public float standRunAcceleration;
    [SerializeField] public float standSprintSpeed;
    [SerializeField] public float standSprintAcceleration;
    [SerializeField] public float standSprintLeftAngleMax;
    [SerializeField] public float standSprintRightAngleMax;
    [SerializeField] public float standJumpForce;
    [SerializeField] public float standStepUpPercent;
    [SerializeField] public float standStepDownPercent;
    [SerializeField] public float standSlopeUpAngle;
    [SerializeField] public float standSlopeDownAngle;
    [SerializeField] public Vector3 standCapsuleCenter;
    [SerializeField] public float standCapsuleHeight;
    [SerializeField] public float standCapsuleRadius;
    [SerializeField] public float standToCrouchTransitionSpeed;
    [SerializeField] public bool standMaintainVelocityOnSurface;
    [SerializeField] public bool standMaintainVelocityAlongSurface;

    [SerializeField] public float crouchIdleSpeed;
    [SerializeField] public float crouchIdleAcceleration;
    [SerializeField] public float crouchWalkSpeed;
    [SerializeField] public float crouchWalkAcceleration;
    [SerializeField] public float crouchRunSpeed;
    [SerializeField] public float crouchRunAcceleration;
    [SerializeField] public bool crouchAutoRiseToStandSprint;
    [SerializeField] public float crouchJumpForce;
    [SerializeField] public float crouchStepUpPercent;
    [SerializeField] public float crouchStepDownPercent;
    [SerializeField] public float crouchSlopeUpAngle;
    [SerializeField] public float crouchSlopeDownAngle;
    [SerializeField] public Vector3 crouchCapsuleCenter;
    [SerializeField] public float crouchCapsuleHeight;
    [SerializeField] public float crouchCapsuleRadius;
    [SerializeField] public float crouchToStandTransitionSpeed;
    [SerializeField] public bool crouchMaintainVelocityOnSurface;
    [SerializeField] public bool crouchMaintainVelocityAlongSurface;

    public override CharacterMovementModule GetModule()
    {
        return new CharacterMovementGroundModule(this);
    }
}