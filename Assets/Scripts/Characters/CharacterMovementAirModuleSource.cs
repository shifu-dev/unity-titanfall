using UnityEngine;

[CreateAssetMenu(menuName = MENU_PATH + "Air Module")]
public class CharacterMovementAirModuleSource : CharacterMovementModuleSource
{
    [SerializeField] public float gravityAcceleration;
    [SerializeField] public float gravityMaxSpeed;
    [SerializeField] public float minMoveDistance;
    [SerializeField] public float moveSpeed;
    [SerializeField] public float moveAcceleration;
    [SerializeField] public float jumpPower;
    [SerializeField] public uint maxJumpCount;

    public override CharacterMovementModule GetModule()
    {
        return new CharacterMovementAirModule(this);
    }
}