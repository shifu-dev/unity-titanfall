using UnityEngine;

public class PlayerInputs : MonoBehaviour
{
    [field: SerializeField] public bool useDebugInputs { get; protected set; }
    [field: SerializeField] public Vector3 debugMove { get; protected set; }

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
    [field: SerializeField, ReadOnly] public bool use1 { get; protected set; }
    [field: SerializeField, ReadOnly] public bool use2 { get; protected set; }
    [field: SerializeField, ReadOnly] public bool use3 { get; protected set; }
    [field: SerializeField, ReadOnly] public bool reload { get; protected set; }

    [field: SerializeField] public VectorBool invertLook { get; protected set; }
    [field: SerializeField] public Vector3 lookSensitivity { get; protected set; }

    public void Init(Player character)
    {
    }

    public void UpdateImpl()
    {
        //////////////////////////////////////////////////////////////////
        /// move
        Vector3 tmpMove = Vector3.zero;
        if (useDebugInputs)
        {
            tmpMove = debugMove;
        }
        else
        {
            tmpMove.y += Input.GetKey(KeyCode.W) ? 1 : 0;
            tmpMove.x += Input.GetKey(KeyCode.A) ? -1 : 0;
            tmpMove.y += Input.GetKey(KeyCode.S) ? -1 : 0;
            tmpMove.x += Input.GetKey(KeyCode.D) ? 1 : 0;
        }
        move = tmpMove;

        moveAngle = Vector3.SignedAngle(move, new Vector3(0, 1, 0), new Vector3(0, 0, 1));

        walk = Input.GetKey(KeyCode.LeftControl);
        sprint = Input.GetKey(KeyCode.LeftShift);
        jump = Input.GetKey(KeyCode.Space);
        crouch = Input.GetKey(KeyCode.C);
        prone = Input.GetKey(KeyCode.V);

        //////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////
        /// look

        float look_x = Input.GetAxis("look x");
        float look_y = -Input.GetAxis("look y");
        float look_z = 0;

        look_x = 0;
        look_y = 0;

        look_x = invertLook.x ? -look_x : look_x;
        look_y = invertLook.y ? -look_y : look_y;
        look_z = invertLook.z ? -look_z : look_z;

        look_x *= lookSensitivity.x;
        look_y *= lookSensitivity.y;
        look_z *= lookSensitivity.z;

        look = new Vector3(look_x, look_y, look_z);

        //////////////////////////////////////////////////////////////////

        peekLeft = Input.GetKey(KeyCode.Q);
        peekRight = Input.GetKey(KeyCode.E);
        action = Input.GetKey(KeyCode.Tab);

        //////////////////////////////////////////////////////////////////
        /// weapons

        weapon1 = Input.GetKeyDown(KeyCode.Alpha1);
        weapon2 = Input.GetKeyDown(KeyCode.Alpha2);
        weapon3 = Input.GetKeyDown(KeyCode.Alpha3);

        grenade1 = Input.GetKey(KeyCode.Alpha4);
        grenade2 = Input.GetKey(KeyCode.Alpha5);

        //////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////
        /// actions

        use1 = Input.GetMouseButton(0);
        use2 = Input.GetMouseButton(1);
        use3 = Input.GetMouseButton(2);
        reload = Input.GetKey(KeyCode.R);
    }
}