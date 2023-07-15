using UnityEngine;

public class CharacterMovementAirModule : CharacterMovementModule
{
    protected const uint AIR_MAX_MOVE_ITERATIONS = 10;
    protected const float GRAVITY_MULTIPLIER = .05f;

    public CharacterMovementAirModule(CharacterMovementAirModuleSource source)
    {
        // cache Data from CharacterDataSource
        if (source is not null)
        {
            _gravityAcceleration = source.gravityAcceleration;
            _gravityMaxSpeed = source.gravityMaxSpeed;
            _minMoveDistance = source.minMoveDistance;
            _moveSpeed = source.moveSpeed;
            _moveAcceleration = source.moveAcceleration;
            _jumpPower = source.jumpPower;
            _maxJumpCount = source.maxJumpCount;
        }
    }

    public override void OnLoaded(CharacterMovement charMovement)
    {
        base.OnLoaded(charMovement);

        UpdateGroundModule();
    }

    public override bool ShouldRun()
    {
        return true;
    }

    public override void Update()
    {
        UpdateValues();

        Vector3 charUp = _charUp;
        Vector3 charForward = _character.forward;
        Vector3 charRight = _character.right;
        float mass = _character.mass;
        float gravitySpeed = _gravityAcceleration * mass * _deltaTime * _deltaTime * GRAVITY_MULTIPLIER;

        Vector3 vel = _velocity * _deltaTime;
        Vector3 velH = Vector3.ProjectOnPlane(vel, charUp);
        Vector3 velV = vel - velH;

        Vector3 moveV = velV + (charUp * gravitySpeed);
        Vector3 moveH = velH;

        // Vector3 moveHX = Vector3.ProjectOnPlane(moveH, charForward);
        // Vector3 moveHZ = moveH - moveHX;
        // // processed move input
        // Vector3 moveInputRaw = _move;
        // Vector3 moveInput = new Vector3(moveInputRaw.x, 0f, moveInputRaw.y);
        // moveInput = Quaternion.Euler(0f, _charView.turnAngle, 0f) * moveInput;
        // moveInput = character.rotation * moveInput;

        // // helping movement in mAir
        // Vector3 move_help_h = _curMoveSpeed * moveInput * mDeltaTime;
        // Vector3 move_help_h_x = Vector3.ProjectOnPlane(move_help_h, charForward);
        // Vector3 move_help_h_z = move_help_h - move_help_h_x;

        // if (move_help_h_x.magnitude > 0f)
        // {
        //     if (moveHX.normalized == move_help_h_x.normalized)
        //     {
        //         if (move_help_h_x.magnitude > moveHX.magnitude)
        //         {
        //             moveHX = move_help_h_x;
        //         }
        //     }
        //     else
        //     {
        //         moveHX = move_help_h_x;
        //     }
        // }

        // if (move_help_h_z.magnitude > 0f)
        // {
        //     if (moveHZ.normalized == move_help_h_z.normalized)
        //     {
        //         if (move_help_h_z.magnitude > moveHZ.magnitude)
        //         {
        //             moveHZ = move_help_h_z;
        //         }
        //     }
        //     else
        //     {
        //         moveHZ = move_help_h_z;
        //     }
        // }

        // moveH = moveHX + moveHZ;

        // process character jump
        if (_inputJump && _curJumpCount < _maxJumpCount)
        {
            _curJumpCount++;

            if (_curMaintainVelocityOnJump == false)
            {
                moveV = Vector3.zero;
            }

            moveV = charUp * _curJumpPower;
        }

        Vector3 move = moveH + moveV;

        PerformMove(move);
    }

    protected virtual void UpdateGroundModule()
    {
        _groundModule = null;
        if (_charMovement is not null)
        {
            foreach (var module in _charMovement.modules)
            {
                if (module is CharacterMovementGroundModule)
                {
                    _groundModule = module as CharacterMovementGroundModule;
                }
            }
        }
    }

    protected virtual void UpdateValues()
    {
        _curMoveAccel = _moveAcceleration;
        _curMoveSpeed = _moveSpeed;
        _curJumpPower = _jumpPower;
        _curMaxJumpCount = _maxJumpCount;
        _curMinMoveDist = _minMoveDistance;

        // TODO: add this field in data asset
        _curMaintainVelocityOnJump = false;
    }

    protected virtual void PerformMove(Vector3 move)
    {
        Vector3 remainingMove = move;

        for (int i = 0; i < AIR_MAX_MOVE_ITERATIONS; i++)
        {
            remainingMove -= CapsuleMove(remainingMove, out RaycastHit hit, out Vector3 hitNormal);

            if (hit.collider is null)
            {
                // no collision, so end the move
                remainingMove = Vector3.zero;
                break;
            }

            MoveAlongSurface(move, ref remainingMove, hit, hitNormal);
        }
    }

    protected virtual void MoveAlongSurface(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit, Vector3 hitNormal)
    {
        if (hit.collider is null || remainingMove == Vector3.zero)
            return;

        RecalculateNormalIfZero(hit, ref hitNormal);

        bool canStandOnGround = _groundModule is not null &&
         _groundModule.CanStandOnGround(hit, hitNormal, out float slopeAngle);

        if (canStandOnGround)
        {
            remainingMove = Vector3.zero;
            return;
        }

        // hit.normal gives normal respective to capsule's body,
        // useful for sliding off on corners
        Vector3 slideMove = Vector3.ProjectOnPlane(remainingMove, hit.normal);
        if (_curMaintainVelocityAlongSurface)
        {
            slideMove = slideMove.normalized * remainingMove.magnitude;
        }

        remainingMove = slideMove;
    }

    protected CharacterMovementGroundModule _groundModule;

    protected float _gravityAcceleration;
    protected float _gravityMaxSpeed;
    protected float _minMoveDistance;
    protected float _moveSpeed;
    protected float _moveAcceleration;
    protected float _jumpPower;

    protected uint _maxJumpCount;

    protected bool _inputJump;

    protected float _curMinMoveDist = 0;
    protected float _curMoveSpeed = 0;
    protected float _curMoveAccel = 0;
    protected float _curJumpPower = 0;
    protected float _curStepUpHeight = 0;
    protected float _curStepDownDepth = 0;
    protected float _curSlopeUpAngle = 0;
    protected float _curSlopeDownAngle = 0;
    protected uint _curJumpCount = 0;
    protected uint _curMaxJumpCount = 0;
    protected bool _curMaintainVelocityOnJump = false;
    protected bool _curMaintainVelocityOnSurface = true;
    protected bool _curMaintainVelocityAlongSurface = true;
}