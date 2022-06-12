using System;
using UnityEngine;
using System.Runtime.CompilerServices;

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
    public const uint GROUND_SLIDE = 15;
    public const uint GROUND_ROLL = 16;
    public const uint AIR_JUMP = 17;
    public const uint AIR_IDLE = 18;
    public const uint CUSTOM = 19;

    private static readonly string[] strings = new string[]
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

    public uint previous { get; }
    public uint current { get; set; }
    public float weight { get; set; }

    public bool isCustom
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => current >= CUSTOM;
    }
    public bool isGrounded
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    public bool isGroundStanding
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => current == GROUND_STAND_JUMP ||
               current == GROUND_STAND_IDLE ||
               current == GROUND_STAND_WALK ||
               current == GROUND_STAND_RUN ||
               current == GROUND_STAND_SPRINT;
    }
    public bool isGroundStandIdle
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => current == GROUND_STAND_IDLE;
    }
    public bool isGroundStandJump
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => current == GROUND_STAND_JUMP;
    }
    public bool isGroundStandWalking
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => current == GROUND_STAND_WALK;
    }
    public bool isGroundStandRunning
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => current == GROUND_STAND_RUN;
    }
    public bool isGroundStandSprinting
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => current == GROUND_STAND_SPRINT;
    }
    public bool isGroundCrouching
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => current == GROUND_CROUCH_JUMP ||
               current == GROUND_CROUCH_IDLE ||
               current == GROUND_CROUCH_WALK ||
               current == GROUND_CROUCH_RUN;
    }
    public bool isGroundCrouchIdle
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => current == GROUND_CROUCH_IDLE;
    }
    public bool isGroundCrouchJump
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => current == GROUND_CROUCH_JUMP;
    }
    public bool isGroundCrouchWalking
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => current == GROUND_CROUCH_WALK;
    }
    public bool isGroundCrouchRunning
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => current == GROUND_CROUCH_RUN;
    }
    public bool isGroundProne
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => current == GROUND_PRONE_IDLE ||
                   current == GROUND_PRONE_MOVE ||
                   current == GROUND_PRONE_ROLL;
    }
    public bool isAir
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => current == AIR_IDLE || 
               current == AIR_JUMP;
    }
    public bool isAirRising
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => false;
    }
    public bool isAirIdle
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => false;
    }
    public bool isAirFalling
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => false;
    }

    public string getString
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => m_previous;
    }
    public uint current
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => m_current;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (value != m_current)
            {
                m_previous = m_current;
                m_current = value;
                m_weight = 1 - m_weight;
            }
        }
    }
    public float weight
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => m_weight;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => m_weight = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CharacterMovementStateImpl(uint state, float weight = 1)
    {
        this.m_previous = CharacterMovementState.NONE;
        this.m_current = state;
        this.m_weight = weight;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return pimpl.getString;
    }
}
