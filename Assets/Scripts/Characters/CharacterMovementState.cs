using System;

[Serializable]
public struct CharacterMovementState
{
    public enum Enum : uint
    {
        UNKNOWN = 0,
        CUSTOM,
        DEAD,

        GROUND_STAND_JUMP,
        GROUND_STAND_IDLE,
        GROUND_STAND_WALK,
        GROUND_STAND_RUN,
        GROUND_STAND_SPRINT,
        GROUND_CROUCH_JUMP,
        GROUND_CROUCH_IDLE,
        GROUND_CROUCH_WALK,
        GROUND_CROUCH_RUN,
        GROUND_PRONE_IDLE,
        GROUND_PRONE_MOVE,
        GROUND_PRONE_ROLL,

        AIR_JUMP,
        AIR_IDLE,
    }

    public Enum State;

    public CharacterMovementState(Enum State_In)
    {
        State = State_In;
    }

    public uint ToIndex()
    {
        return ((uint)State);
    }

    public override String ToString()
    {
        return State.ToString();
    }

    public bool IsCustom() => false;
    public bool IsGrounded()
    {
        return State == Enum.GROUND_STAND_JUMP ||
               State == Enum.GROUND_STAND_IDLE ||
               State == Enum.GROUND_STAND_WALK ||
               State == Enum.GROUND_STAND_RUN ||
               State == Enum.GROUND_STAND_SPRINT ||
               State == Enum.GROUND_CROUCH_JUMP ||
               State == Enum.GROUND_CROUCH_IDLE ||
               State == Enum.GROUND_CROUCH_WALK ||
               State == Enum.GROUND_CROUCH_RUN ||
               State == Enum.GROUND_PRONE_IDLE ||
               State == Enum.GROUND_PRONE_MOVE ||
               State == Enum.GROUND_PRONE_ROLL;
    }
    public bool IsGroundStanding() => false;
    public bool IsGroundCrouching() => false;
    public bool IsGroundProne() => false;
    public bool IsAir() => false;
    public bool IsAirRising() => false;
    public bool IsAirIdle() => false;
    public bool IsAirFalling() => false;
}