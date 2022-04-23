using System;
using UnityEngine;

public struct CharacterInputs : IMovementInputs
{
    public IMovementInputs InputChannel;

	public bool IsValid { get => true; }
    public Vector3 MoveInputVector { get => InputChannel == default ? default : InputChannel.MoveInputVector; }
    public float MoveInputAngle { get => InputChannel == default ? default : InputChannel.MoveInputAngle; }
    public bool WantsToWalk { get => InputChannel == default ? default : InputChannel.WantsToWalk; }
    public bool WantsToSprint { get => InputChannel == default ? default : InputChannel.WantsToSprint; }
    public bool WantsToJump { get => InputChannel == default ? default : InputChannel.WantsToJump; }
    public bool WantsToCrouch { get => InputChannel == default ? default : InputChannel.WantsToCrouch; }
    public bool WantsToProne { get => InputChannel == default ? default : InputChannel.WantsToProne; }
}