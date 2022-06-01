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

    public Enum state;

    public CharacterMovementState(Enum state)
    {
        this.state = state;
    }

    public uint index
    {
        get
        {
            return ((uint)state);
        }
    }

    public bool isCustom
    {
        get
        {
            return false;
        }
    }
    public bool isGrounded
    {
        get
        {
            return state == Enum.GROUND_STAND_JUMP ||
                   state == Enum.GROUND_STAND_IDLE ||
                   state == Enum.GROUND_STAND_WALK ||
                   state == Enum.GROUND_STAND_RUN ||
                   state == Enum.GROUND_STAND_SPRINT ||
                   state == Enum.GROUND_CROUCH_JUMP ||
                   state == Enum.GROUND_CROUCH_IDLE ||
                   state == Enum.GROUND_CROUCH_WALK ||
                   state == Enum.GROUND_CROUCH_RUN ||
                   state == Enum.GROUND_PRONE_IDLE ||
                   state == Enum.GROUND_PRONE_MOVE ||
                   state == Enum.GROUND_PRONE_ROLL;
        }
    }
    public bool isGroundStanding
    {
        get
        {
            return false;
        }
    }
    public bool isGroundCrouching
    {
        get
        {
            return false;
        }
    }
    public bool isGroundProne
    {
        get
        {
            return false;
        }
    }
    public bool isAir
    {
        get
        {
            return false;
        }
    }
    public bool isAirRising
    {
        get
        {
            return false;
        }
    }
    public bool isAirIdle
    {
        get
        {
            return false;
        }
    }
    public bool isAirFalling
    {
        get
        {
            return false;
        }
    }

    public override string ToString()
    {
        return state.ToString();
    }
}