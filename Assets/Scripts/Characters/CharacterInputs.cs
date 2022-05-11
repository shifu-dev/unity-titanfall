using System;
using UnityEngine;

public class CharacterInputs : CharacterBehaviour
{
    public PlayerInputs playerInputs { get; set; }

    public bool IsValid { get => true; }
    public bool IsGettingInputs { get => playerInputs; }
    public float MoveInputAngle { get => playerInputs ? playerInputs.MoveInputAngle : default; }
    public Vector3 MoveInputVector { get => playerInputs ? playerInputs.MoveInputVector : default; }
    public Vector3 LookInputVector { get => playerInputs ? playerInputs.LookInputVector : default; }
    public bool WantsToWalk { get => playerInputs ? playerInputs.WantsToWalk : default; }
    public bool WantsToSprint { get => playerInputs ? playerInputs.WantsToSprint : default; }
    public bool WantsToJump { get => playerInputs ? playerInputs.WantsToJump : default; }
    public bool WantsToCrouch { get => playerInputs ? playerInputs.WantsToCrouch : default; }
    public bool WantsToProne { get => playerInputs ? playerInputs.WantsToProne : default; }

    public Vector3 TotalLookInputVector { get; protected set; }

    public override void OnUpdateCharacter()
    {
        TotalLookInputVector += LookInputVector;
    }
}