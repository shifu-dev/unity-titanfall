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
    public const uint GROUND_STAND_IDLE = 3;
    public const uint GROUND_STAND_IDLE_JUMP = 4;
    public const uint GROUND_STAND_WALK = 5;
    public const uint GROUND_STAND_WALK_JUMP = 6;
    public const uint GROUND_STAND_RUN = 7;
    public const uint GROUND_STAND_RUN_JUMP = 8;
    public const uint GROUND_STAND_SPRINT = 9;
    public const uint GROUND_STAND_SPRINT_JUMP = 10;
    public const uint GROUND_CROUCH_IDLE = 11;
    public const uint GROUND_CROUCH_WALK = 12;
    public const uint GROUND_CROUCH_RUN = 13;
    public const uint GROUND_SLIDE = 14;
    public const uint GROUND_ROLL = 15;
    public const uint AIR_JUMP = 16;
    public const uint AIR_IDLE = 17;
    public const uint CUSTOM = 18;

    private static readonly string[] strings = new string[]
    {
        "NONE",
        "DEAD",
        "NAVIGATION",
        "GROUND STAND IDLE",
        "GROUND STAND IDLE JUMP",
        "GROUND STAND WALK",
        "GROUND STAND WALK JUMP",
        "GROUND STAND RUN",
        "GROUND STAND RUN JUMP",
        "GROUND STAND SPRINT",
        "GROUND STAND SPRINT JUMP",
        "GROUND CROUCH IDLE",
        "GROUND CROUCH WALK",
        "GROUND CROUCH RUN",
        "GROUND SLIDE",
        "GROUND ROLL",
        "AIR JUMP",
        "AIR IDLE",
        "CUSTOM"
    };

    //////////////////////////////////////////////////////////////////
    /// Member Fields
    //////////////////////////////////////////////////////////////////

    public uint current { get; set; }

    public bool isCustom
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => current >= CUSTOM;
    }
    public bool isGrounded
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => current == GROUND_STAND_IDLE ||
               current == GROUND_STAND_IDLE_JUMP ||
               current == GROUND_STAND_WALK ||
               current == GROUND_STAND_WALK_JUMP ||
               current == GROUND_STAND_RUN ||
               current == GROUND_STAND_RUN_JUMP ||
               current == GROUND_STAND_SPRINT ||
               current == GROUND_STAND_SPRINT_JUMP ||
               current == GROUND_CROUCH_IDLE ||
               current == GROUND_CROUCH_WALK ||
               current == GROUND_CROUCH_RUN ||
               current == GROUND_SLIDE ||
               current == GROUND_ROLL;
    }
    public bool isGroundStanding
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => current == GROUND_STAND_IDLE ||
               current == GROUND_STAND_IDLE_JUMP ||
               current == GROUND_STAND_WALK ||
               current == GROUND_STAND_WALK_JUMP ||
               current == GROUND_STAND_RUN ||
               current == GROUND_STAND_RUN_JUMP ||
               current == GROUND_STAND_SPRINT ||
               current == GROUND_STAND_SPRINT_JUMP;
    }
    public bool isGroundStandIdle
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => current == GROUND_STAND_IDLE;
    }
    public bool isGroundStandJump
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => current == GROUND_STAND_IDLE_JUMP ||
               current == GROUND_STAND_WALK_JUMP ||
               current == GROUND_STAND_RUN_JUMP ||
               current == GROUND_STAND_SPRINT_JUMP;
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
        get => current == GROUND_CROUCH_IDLE ||
               current == GROUND_CROUCH_WALK ||
               current == GROUND_CROUCH_RUN;
    }
    public bool isGroundCrouchIdle
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => current == GROUND_CROUCH_IDLE;
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

#if UNITY_EDITOR
    [Label("Current"), SerializeField] private string editor_current;
#endif

    private uint _current;

    public uint current
    {
        get => _current;
        set
        {
            _current = value;

#if UNITY_EDITOR
            editor_current = ToString();
#endif
        }
    }

    public CharacterMovementStateImpl(uint state, float weight = 1)
    {
        _current = state;

#if UNITY_EDITOR
        editor_current = null;
        editor_current = ToString();
#endif
    }

    public override string ToString()
    {
        return pimpl.getString;
    }
}
