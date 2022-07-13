using System;
using UnityEngine;

public class CharacterInputs : CharacterBehaviour
{
    public PlayerInputs playerInputs { get; protected set; }

    public bool isValid => true;
    public bool isGettingInputs => playerInputs;

    // Movement Inputs
    public Vector3 move => playerInputs ? playerInputs.move : default;
    public float moveAngle => playerInputs ? playerInputs.moveAngle : default;
    public bool walk => playerInputs ? playerInputs.walk : default;
    public bool sprint => playerInputs ? playerInputs.sprint : default;
    public bool jump => playerInputs ? playerInputs.jump : default;
    public bool crouch => playerInputs ? playerInputs.crouch : default;
    public bool prone => playerInputs ? playerInputs.prone : default;

    // Look Inputs
    public Vector3 look => playerInputs ? playerInputs.look : default;
    public Vector3 lookProcessed { get; protected set; }

    // Action Inputs
    public bool action => playerInputs ? playerInputs.action : default;

    // Weapon Inputs
    public bool weapon1 => playerInputs ? playerInputs.weapon1 : false;
    public bool weapon2 => playerInputs ? playerInputs.weapon2 : false;
    public bool weapon3 => playerInputs ? playerInputs.weapon3 : false;
    public bool grenade1 => playerInputs ? playerInputs.grenade1 : false;
    public bool grenade2 => playerInputs ? playerInputs.grenade2 : false;
    public bool reload => playerInputs ? playerInputs.reload : false;
    public bool use1 => playerInputs ? playerInputs.use1 : false;
    public bool use2 => playerInputs ? playerInputs.use2 : false;
    public bool use3 => playerInputs ? playerInputs.use3 : false;

    public override void OnUpdateCharacter()
    {
        base.OnUpdateCharacter();

        lookProcessed += look;
    }

    public override void OnPossessed(Player player)
    {
        this.playerInputs = player.playerInputs;
    }
}