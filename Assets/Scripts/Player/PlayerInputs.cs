using UnityEngine;

public class PlayerInputs : MonoBehaviour
{
    // Movement Inputs
    [field: SerializeField, ReadOnly] public Vector3 move { get; protected set; }
    [field: SerializeField, ReadOnly] public float moveAngle { get; protected set; }
    [field: SerializeField, ReadOnly] public bool walk { get; protected set; }
    [field: SerializeField, ReadOnly] public bool sprint { get; protected set; }
    [field: SerializeField, ReadOnly] public bool jump { get; protected set; }
    [field: SerializeField, ReadOnly] public bool crouch { get; protected set; }
    [field: SerializeField, ReadOnly] public bool prone { get; protected set; }
    [field: SerializeField, ReadOnly] public bool peekLeft { get; protected set; }
    [field: SerializeField, ReadOnly] public bool peekRight { get; protected set; }

    // Look Inputs
    [field: SerializeField, ReadOnly] public Vector3 look { get; protected set; }

    // Action Inputs
    [field: SerializeField, ReadOnly] public bool action { get; protected set; }

    // Weapon Inputs
    [field: SerializeField, ReadOnly] public bool grenade1 { get; protected set; }
    [field: SerializeField, ReadOnly] public bool grenade2 { get; protected set; }
    [field: SerializeField, ReadOnly] public bool weapon1 { get; protected set; }
    [field: SerializeField, ReadOnly] public bool weapon2 { get; protected set; }
    [field: SerializeField, ReadOnly] public bool weapon3 { get; protected set; }
    [field: SerializeField, ReadOnly] public bool fire1 { get; protected set; }
    [field: SerializeField, ReadOnly] public bool fire2 { get; protected set; }
    [field: SerializeField, ReadOnly] public bool fire3 { get; protected set; }
    [field: SerializeField, ReadOnly] public bool reload { get; protected set; }

    public void Init(Player character)
    {
    }

    public void UpdateImpl()
    {
        float moveInput_x = Input.GetAxis("move x");
        float moveInput_y = Input.GetAxis("move y");
        float moveInput_z = 0;
        move = new Vector3(moveInput_x, moveInput_y, moveInput_z);

        moveAngle = Vector3.SignedAngle(move, new Vector3(0, 1, 0), new Vector3(0, 0, 1));

        walk = Input.GetKey(KeyCode.LeftControl);
        sprint = Input.GetKey(KeyCode.LeftShift);
        jump = Input.GetKey(KeyCode.Space);
        crouch = Input.GetKey(KeyCode.C);
        prone = Input.GetKey(KeyCode.V);

        float lookInput_x = Input.GetAxis("look x");
        float lookInput_y = -Input.GetAxis("look y");
        float lookInput_z = 0;
        look = new Vector3(lookInput_x, lookInput_y, lookInput_z);

        peekLeft = Input.GetKey(KeyCode.Q);
        peekRight = Input.GetKey(KeyCode.E);
        action = Input.GetKey(KeyCode.Tab);

        weapon1 = Input.GetKeyDown(KeyCode.Alpha1);
        weapon2 = Input.GetKeyDown(KeyCode.Alpha2);
        weapon3 = Input.GetKeyDown(KeyCode.Alpha3);

        grenade1 = Input.GetKey(KeyCode.Alpha4);
        grenade2 = Input.GetKey(KeyCode.Alpha5);

        fire1 = Input.GetMouseButton(0);
        fire2 = Input.GetMouseButton(1);
        fire3 = Input.GetMouseButton(2);
        reload = Input.GetKey(KeyCode.R);
    }
}