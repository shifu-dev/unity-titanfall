using System;
using UnityEngine;

public interface CharacterMovementState
{
    //////////////////////////////////////////////////////////////////
    /// Enum Fields
    //////////////////////////////////////////////////////////////////

    public const uint NONE = 0;
    public const uint DEAD = 1;
    public const uint NAVIGATION = 2;
    public const uint GROUND_STAND_JUMP = 3;
    public const uint GROUND_STAND_IDLE = 4;
    public const uint GROUND_STAND_WALK = 5;
    public const uint GROUND_STAND_RUN = 6;
    public const uint GROUND_STAND_SPRINT = 7;
    public const uint GROUND_CROUCH_JUMP = 8;
    public const uint GROUND_CROUCH_IDLE = 9;
    public const uint GROUND_CROUCH_WALK = 10;
    public const uint GROUND_CROUCH_RUN = 11;
    public const uint GROUND_PRONE_IDLE = 12;
    public const uint GROUND_PRONE_MOVE = 13;
    public const uint GROUND_PRONE_ROLL = 14;
    public const uint GROUND_SLIDE = 11;
    public const uint GROUND_ROLL = 11;
    public const uint AIR_JUMP = 15;
    public const uint AIR_IDLE = 16;
    public const uint CUSTOM = 17;

    public static readonly string[] strings = new string[]
    {
        "NONE",
        "DEAD",
        "NAVIGATION",
        "GROUND STAND JUMP",
        "GROUND STAND IDLE",
        "GROUND STAND WALK",
        "GROUND STAND RUN",
        "GROUND STAND SPRINT",
        "GROUND CROUCH JUMP",
        "GROUND CROUCH IDLE",
        "GROUND CROUCH WALK",
        "GROUND CROUCH RUN",
        "GROUND PRONE IDLE",
        "GROUND PRONE MOVE",
        "GROUND PRONE ROLL",
        "GROUND SLIDE",
        "GROUND ROLL",
        "AIR JUMP",
        "AIR IDLE",
        "CUSTOM"
    };

    //////////////////////////////////////////////////////////////////
    /// Member Fields
    //////////////////////////////////////////////////////////////////

    uint previous { get; set; }
    uint current { get; set; }
    // uint current
    // {
    //     get;
    //     set
    //     {
    //         if (value != current)
    //         {
    //             previous = current;
    //             current = value;
    //             weight = 1;
    //         }
    //     }
    // }
    float weight { get; set; }

    bool isCustom
    {
        get => current == CUSTOM;
    }
    bool isGrounded
    {
        get => current == GROUND_STAND_JUMP ||
               current == GROUND_STAND_IDLE ||
               current == GROUND_STAND_WALK ||
               current == GROUND_STAND_RUN ||
               current == GROUND_STAND_SPRINT ||
               current == GROUND_CROUCH_JUMP ||
               current == GROUND_CROUCH_IDLE ||
               current == GROUND_CROUCH_WALK ||
               current == GROUND_CROUCH_RUN ||
               current == GROUND_PRONE_IDLE ||
               current == GROUND_PRONE_MOVE ||
               current == GROUND_PRONE_ROLL;

    }
    bool isGroundStanding
    {
        get => current == GROUND_STAND_JUMP ||
               current == GROUND_STAND_IDLE ||
               current == GROUND_STAND_WALK ||
               current == GROUND_STAND_RUN ||
               current == GROUND_STAND_SPRINT;
    }
    bool isGroundCrouching
    {
        get => current == GROUND_CROUCH_JUMP ||
               current == GROUND_CROUCH_IDLE ||
               current == GROUND_CROUCH_WALK ||
               current == GROUND_CROUCH_RUN;
    }
    bool isGroundProne
    {
        get => current == GROUND_PRONE_IDLE ||
               current == GROUND_PRONE_MOVE ||
               current == GROUND_PRONE_ROLL;
    }
    bool isAir
    {
        get => false;
    }
    bool isAirRising
    {
        get => false;
    }
    bool isAirIdle
    {
        get => false;
    }
    bool isAirFalling
    {
        get => false;
    }

    string getString
    {
        get => strings[Math.Clamp(current, NONE, CUSTOM)];
    }
}

[Serializable]
public struct CharacterMovementStateImpl : CharacterMovementState
{
    private CharacterMovementState pimpl => this as CharacterMovementState;

    [SerializeField] private uint m_previous;
    [SerializeField] private uint m_current;
    [SerializeField, Range(0, 1)] private float m_weight;

    public uint previous
    {
        get => m_previous;
        set => m_previous = value;
    }
    public uint current
    {
        get => m_current;
        set => m_current = value;
    }
    public float weight
    {
        get => m_weight;
        set => m_weight = value;
    }

    public CharacterMovementStateImpl(uint state, float weight = 1)
    {
        this.m_previous = CharacterMovementState.NONE;
        this.m_current = state;
        this.m_weight = weight;
    }

    public override string ToString()
    {
        return pimpl.getString;
    }
}
