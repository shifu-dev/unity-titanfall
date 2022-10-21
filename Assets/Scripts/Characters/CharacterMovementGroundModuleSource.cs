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

    [SerializeField] public AnimationClip animStandIdle;
    [SerializeField] public AnimationClip animStandWalkForward;
    [SerializeField] public AnimationClip animStandWalkForwardLeft;
    [SerializeField] public AnimationClip animStandWalkForwardRight;
    [SerializeField] public AnimationClip animStandWalkLeft;
    [SerializeField] public AnimationClip animStandWalkRight;
    [SerializeField] public AnimationClip animStandWalkBackward;
    [SerializeField] public AnimationClip animStandWalkBackwardLeft;
    [SerializeField] public AnimationClip animStandWalkBackwardRight;
    [SerializeField] public AnimationClip animStandRunForward;
    [SerializeField] public AnimationClip animStandRunForwardLeft;
    [SerializeField] public AnimationClip animStandRunForwardRight;
    [SerializeField] public AnimationClip animStandRunLeft;
    [SerializeField] public AnimationClip animStandRunRight;
    [SerializeField] public AnimationClip animStandRunBackward;
    [SerializeField] public AnimationClip animStandRunBackwardLeft;
    [SerializeField] public AnimationClip animStandRunBackwardRight;
    [SerializeField] public AnimationClip animStandSprintForward;

    [SerializeField] public AnimationClip animCrouchIdle;
    [SerializeField] public AnimationClip animCrouchWalkForward;
    [SerializeField] public AnimationClip animCrouchWalkForwardLeft;
    [SerializeField] public AnimationClip animCrouchWalkForwardRight;
    [SerializeField] public AnimationClip animCrouchWalkLeft;
    [SerializeField] public AnimationClip animCrouchWalkRight;
    [SerializeField] public AnimationClip animCrouchWalkBackward;
    [SerializeField] public AnimationClip animCrouchWalkBackwardLeft;
    [SerializeField] public AnimationClip animCrouchWalkBackwardRight;
    [SerializeField] public AnimationClip animCrouchRunForward;
    [SerializeField] public AnimationClip animCrouchRunForwardLeft;
    [SerializeField] public AnimationClip animCrouchRunForwardRight;
    [SerializeField] public AnimationClip animCrouchRunLeft;
    [SerializeField] public AnimationClip animCrouchRunRight;
    [SerializeField] public AnimationClip animCrouchRunBackward;
    [SerializeField] public AnimationClip animCrouchRunBackwardLeft;
    [SerializeField] public AnimationClip animCrouchRunBackwardRight;

    public override CharacterMovementModule GetModule()
    {
        return new CharacterMovementGroundModule(this);
    }
}