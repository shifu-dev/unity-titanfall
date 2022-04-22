using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(CharacterInteraction))]
[RequireComponent(typeof(CharacterWeapon))]
[RequireComponent(typeof(CharacterCamera))]
[RequireComponent(typeof(CharacterCapsule))]
[RequireComponent(typeof(CharacterAnimation))]
public abstract class Character : MonoBehaviour
{
    public CharacterMovement CharacterMovement;
    public CharacterInteraction CharacterInteraction;
    public CharacterWeapon CharacterWeapon;
    public CharacterCamera CharacterCamera;
    public CharacterCapsule CharacterCapsule;
    public CharacterAnimation CharacterAnimation;
}