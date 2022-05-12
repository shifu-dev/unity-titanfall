using System;
using UnityEngine;

public class CharacterInputs : CharacterBehaviour
{
    public PlayerInputs playerInputs { get; set; }

    public bool isValid { get => true; }
    public bool isGettingInputs { get => playerInputs; }

    // Movement Inputs
    public Vector3 moveInputVector { get => playerInputs ? playerInputs.moveInputVector : default; }
    public float moveInputAngle { get => playerInputs ? playerInputs.moveInputAngle : default; }
    public bool wantsToWalk { get => playerInputs ? playerInputs.wantsToWalk : default; }
    public bool wantsToSprint { get => playerInputs ? playerInputs.wantsToSprint : default; }
    public bool wantsToJump { get => playerInputs ? playerInputs.wantsToJump : default; }
    public bool wantsToCrouch { get => playerInputs ? playerInputs.wantsToCrouch : default; }
    public bool wantsToProne { get => playerInputs ? playerInputs.wantsToProne : default; }

    // Look Inputs
    public Vector3 lookInputVector { get => playerInputs ? playerInputs.lookInputVector : default; }
    public Vector3 totalLookInputVector { get; protected set; }

    // Weapon Inputs
    public bool grenadeSlot1 { get => playerInputs ? playerInputs.grenadeSlot1 : false; }
    public bool grenadeSlot2 { get => playerInputs ? playerInputs.grenadeSlot1 : false; }

    public override void OnUpdateCharacter()
    {
        totalLookInputVector += lookInputVector;
    }
}