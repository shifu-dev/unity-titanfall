using System;
using UnityEngine;

public class CharacterInputs : CharacterBehaviour
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

    public override void OnUpdateCharacter()
    {
        TotalLookInputVector += LookInputVector;
    }
}