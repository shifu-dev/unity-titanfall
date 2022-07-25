using UnityEngine;

public class PlayerInputs : PlayerBehaviour
{
    protected Character _character;

    protected Vector2 _move = Vector2.zero;
    protected Vector2 _look = Vector2.zero;
    protected bool _walk = false;
    protected bool _sprint = false;
    protected bool _crouch = false;
    protected bool _prone = false;
    protected bool _jump = false;
    protected bool _weapon1;
    protected bool _weapon2;
    protected bool _weapon3;
    protected bool _grenade1;
    protected bool _grenade2;
    protected bool _reload;
    protected bool _leftFire;
    protected bool _rightFire;
    protected bool _sight;
    protected bool _interact;

    public override void OnPlayerPossess(Character character)
    {
        _character = character;
    }

    public override void OnPlayerUpdate()
    {
        if (_character != null)
        {
            ProcessInputs();

            CharacterMovement charMovement = _character.charMovement;
            if (charMovement != null)
            {
                charMovement.SetMoveVector(_move);

                if (_sprint) charMovement.StartSprint();
                if (_walk) charMovement.StartWalk();
                if (_jump) charMovement.StartJump();
                if (_crouch) charMovement.StartCrouch();
                if (_prone) charMovement.StartProne();
            }

            CharacterView charView = _character.charView;
            if (charView != null)
            {
                charView.SetLookVector(_look);
            }

            CharacterEquip charEquip = _character.charEquip;
            if (charEquip != null)
            {
                if (_weapon1) charEquip.SwitchToWeapon1();
                else if (_weapon2) charEquip.SwitchToWeapon2();
                else if (_weapon3) charEquip.SwitchToWeapon3();

                if (_grenade1) charEquip.SwitchToGrenade1();
                else if (_grenade2) charEquip.SwitchToGrenade2();

                if (_leftFire) charEquip.FireLeftWeapon();
                if (_rightFire) charEquip.FireRightWeapon();

                if (_reload) charEquip.ReloadRightWeapon();

                if (_interact) charEquip.EquipCommand();
            }
        }
    }

    protected virtual void ProcessInputs()
    {
        _move = GetMoveVector();
        _look = GetLookVector();
        _walk = Input.GetKey(KeyCode.LeftControl);
        _sprint = Input.GetKey(KeyCode.LeftShift);
        _crouch = Input.GetKey(KeyCode.C);
        _prone = Input.GetKey(KeyCode.V);
        _jump = Input.GetKeyDown(KeyCode.Space);

        _weapon1 = Input.GetKeyDown(KeyCode.Alpha1);
        _weapon2 = Input.GetKeyDown(KeyCode.Alpha2);
        _weapon3 = Input.GetKeyDown(KeyCode.Alpha3);
        _grenade1 = Input.GetKeyDown(KeyCode.Alpha4);
        _grenade2 = Input.GetKeyDown(KeyCode.Alpha5);
        _reload = Input.GetKeyDown(KeyCode.R);
        _leftFire = Input.GetMouseButton(2);
        _rightFire = Input.GetMouseButton(0);
        _sight = Input.GetMouseButton(1);
        _interact = Input.GetKeyDown(KeyCode.E);
    }

    protected Vector2 GetMoveVector()
    {
        Vector2 move = Vector2.zero;
        move.y += Input.GetKey(KeyCode.W) ? +1 : 0;
        move.x += Input.GetKey(KeyCode.A) ? -1 : 0;
        move.y += Input.GetKey(KeyCode.S) ? -1 : 0;
        move.x += Input.GetKey(KeyCode.D) ? +1 : 0;
        return move;
    }

    protected Vector2 GetLookVector()
    {
        Vector2 look = Vector2.zero;
        look.x = Input.GetAxis("look x");
        look.y = Input.GetAxis("look y");
        return look;
    }
}