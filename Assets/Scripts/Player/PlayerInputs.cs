using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    public bool IsValid { get => true; }
    public Vector3 MoveInputVector { get => _MoveInputVector; }
    public Vector3 LookInputVector { get => _LookInputVector; }
    public float MoveInputAngle { get => _MoveInputAngle; }
    public bool WantsToWalk { get => _WantsToWalk; }
    public bool WantsToSprint { get => _WantsToSprint; }
    public bool WantsToJump { get => _WantsToJump; }
    public bool WantsToCrouch { get => _WantsToCrouch; }
    public bool WantsToProne { get => _WantsToProne; }

    [SerializeField] protected Vector3 _MoveInputVector;
    [SerializeField] protected Vector3 _LookInputVector;
    [SerializeField] protected float _MoveInputAngle;
    [SerializeField] protected bool _WantsToWalk;
    [SerializeField] protected bool _WantsToSprint;
    [SerializeField] protected bool _WantsToJump;
    [SerializeField] protected bool _WantsToCrouch;
    [SerializeField] protected bool _WantsToProne;

    public void Init(Player character)
    {
    }

    public void UpdateImpl()
    {
        _MoveInputVector.x = Input.GetAxis("move x");
        _MoveInputVector.y = Input.GetAxis("move y");
        _MoveInputVector.z = 0;

        _MoveInputAngle = Vector3.SignedAngle(_MoveInputVector, new Vector3(0, 1, 0), new Vector3(0, 0, 1));

        _LookInputVector.x = Input.GetAxis("look x");
        _LookInputVector.y = Input.GetAxis("look y");
        _LookInputVector.z = 0;
    }
}
