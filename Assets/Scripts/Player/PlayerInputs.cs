using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour, IMovementInputs
{
    public bool IsValid { get => true; }
    public Vector3 MoveInputVector { get => _MoveInputVector; }
    public float MoveInputAngle { get => _MoveInputAngle; }
    public bool WantsToWalk { get => _WantsToWalk; }
    public bool WantsToSprint { get => _WantsToSprint; }
    public bool WantsToJump { get => _WantsToJump; }
    public bool WantsToCrouch { get => _WantsToCrouch; }
    public bool WantsToProne { get => _WantsToProne; }

    [SerializeField] protected Vector3 _MoveInputVector;
    [SerializeField] protected float _MoveInputAngle;
    protected bool _WantsToWalk;
    protected bool _WantsToSprint;
    protected bool _WantsToJump;
    protected bool _WantsToCrouch;
    protected bool _WantsToProne;

    public void Init(Player character)
    {
    }

    public void UpdateImpl()
    {
        _MoveInputVector.x = Input.GetAxis("HorizontalMovement");
        _MoveInputVector.y = Input.GetAxis("VerticalMovement");
        _MoveInputVector.z = 0;

        _MoveInputAngle = Vector3.SignedAngle(_MoveInputVector, new Vector3(0, 1, 0), new Vector3(0, 0, 1));
    }
}
