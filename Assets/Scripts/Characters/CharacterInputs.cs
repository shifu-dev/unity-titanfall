using System;
using UnityEngine;

public class CharacterInputs : CharacterBehaviour
{
    public PlayerInputs playerInputs { get; set; }

    public bool isValid { get => true; }
    public bool isGettingInputs { get => playerInputs; }

    // Movement Inputs
    public Vector3 move { get => playerInputs ? playerInputs.move : default; }
    public float moveAngle { get => playerInputs ? playerInputs.moveAngle : default; }
    public bool walk { get => playerInputs ? playerInputs.walk : default; }
    public bool sprint { get => playerInputs ? playerInputs.sprint : default; }
    public bool jump { get => playerInputs ? playerInputs.jump : default; }
    public bool crouch { get => playerInputs ? playerInputs.crouch : default; }
    public bool prone { get => playerInputs ? playerInputs.prone : default; }

    // Look Inputs
    public Vector3 look { get => playerInputs ? playerInputs.look : default; }
    public Vector3 lookProcessed { get; protected set; }

    // Weapon Inputs
    public bool weapon1 { get => playerInputs ? playerInputs.weapon1 : false; }
    public bool weapon2 { get => playerInputs ? playerInputs.weapon2 : false; }
    public bool weapon3 { get => playerInputs ? playerInputs.weapon3 : false; }
    public bool grenade1 { get => playerInputs ? playerInputs.grenade1 : false; }
    public bool grenade2 { get => playerInputs ? playerInputs.grenade2 : false; }
    public bool fire1 { get => playerInputs ? playerInputs.fire1 : false; }
    public bool fire2 { get => playerInputs ? playerInputs.fire2 : false; }
    public bool fire3 { get => playerInputs ? playerInputs.fire3 : false; }

    public override void OnUpdateCharacter()
    {
        lookProcessed += look;
    }
}