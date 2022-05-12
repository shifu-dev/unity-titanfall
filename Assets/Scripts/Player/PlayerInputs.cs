using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    [field: SerializeField, ReadOnly] public Vector3 moveInputVector { get; protected set; }
    [field: SerializeField, ReadOnly] public Vector3 lookInputVector { get; protected set; }
    [field: SerializeField, ReadOnly] public float moveInputAngle { get; protected set; }
    [field: SerializeField, ReadOnly] public bool wantsToWalk { get; protected set; }
    [field: SerializeField, ReadOnly] public bool wantsToSprint { get; protected set; }
    [field: SerializeField, ReadOnly] public bool wantsToJump { get; protected set; }
    [field: SerializeField, ReadOnly] public bool wantsToCrouch { get; protected set; }
    [field: SerializeField, ReadOnly] public bool wantsToProne { get; protected set; }
    [field: SerializeField, ReadOnly] public bool grenadeSlot1 { get; protected set; }
    [field: SerializeField, ReadOnly] public bool grenadeSlot2 { get; protected set; }

    public void Init(Player character)
    {
    }

    public void UpdateImpl()
    {
        float moveInput_x = Input.GetAxis("move x");
        float moveInput_y = Input.GetAxis("move y");
        float moveInput_z = 0;
        moveInputVector = new Vector3(moveInput_x, moveInput_y, moveInput_z);

        moveInputAngle = Vector3.SignedAngle(moveInputVector, new Vector3(0, 1, 0), new Vector3(0, 0, 1));

        float lookInput_x = Input.GetAxis("look x");
        float lookInput_y = -Input.GetAxis("look y");
        float lookInput_z = 0;
        lookInputVector = new Vector3(lookInput_x, lookInput_y, lookInput_z);

        grenadeSlot1 = Input.GetKey("GrenadeSlot1");
        grenadeSlot2 = Input.GetKey("GrenadeSlot2");
    }
}