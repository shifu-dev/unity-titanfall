using System;
using UnityEngine;

[DisallowMultipleComponent]
public class CharacterView : CharacterBehaviour
{
    //////////////////////////////////////////////////////////////////
    /// Types
    //////////////////////////////////////////////////////////////////

    public enum Mode
    {
        None,
        Eyes,
        FirstPerson,
        ThirdPerson
    }

    [Serializable]
    protected struct EyesData
    {
        public Transform eyes;
    }

    [Serializable]
    protected struct FirstPersonData
    {
        public Vector3 standPos;
        [Label("Stand-Crouch Transition")] public AnimationClip standCrouchTransition;
        public Vector3 crouchPos;
        public Vector3 minRotation;
        public Vector3 maxRotation;
    }

    [Serializable]
    protected struct ThirdPersonData
    {
        public bool alwaysUpdate;
        public GameObject lookAtSource;
        public Vector3 lookAtSourceOffset;
        public Vector3 acceleration;
        public Vector3 deceleration;
        public Vector3 minRotation;
        public Vector3 maxRotation;
        public float minDistanceFromLookSource;
        public float maxDistanceFromLookSource;
    }

    //////////////////////////////////////////////////////////////////
    /// Variables
    //////////////////////////////////////////////////////////////////

    protected CharacterInputs _charInputs;
    protected CharacterCapsule _charCapsule;
    protected CharacterMovement _charMovement;

    [SerializeField] protected Camera _camera;
    public new Camera camera
    {
        get => _camera;
        set => _camera = value;
    }

    [SerializeField, ReadOnly] protected Vector3 _lookVector;
    public Vector2 lookVector => _lookVector;

    public Mode mode
    {
        get => _stateMachine.currentState;
        set => _stateMachine.Switch(value);
    }

    [Header("View Modes Data"), Space]
    [SerializeField] protected EyesData _eyesData;
    [SerializeField] protected FirstPersonData _firstPersonData;
    [SerializeField] protected ThirdPersonData _thirdPersonData;

    protected StateMachine<Mode> _stateMachine;

    public float turnAngle => _lookVector.x;

    public CharacterView()
    {
        _stateMachine = new StateMachine<Mode>();
        _stateMachine.Add(Mode.None, null, null, null);
        _stateMachine.Add(Mode.Eyes, EnterViewMode_Eyes, UpdateViewMode_Eyes, ExitViewMode_Eyes);
        _stateMachine.Add(Mode.FirstPerson, EnterViewMode_FirstPerson, UpdateViewMode_FirstPerson, ExitViewMode_FirstPerson);
        _stateMachine.Add(Mode.ThirdPerson, EnterViewMode_ThirdPerson, UpdateViewMode_ThirdPerson, ExitViewMode_ThirdPerson);
        _stateMachine.Switch(Mode.None);
    }

    //////////////////////////////////////////////////////////////////
    /// Updates
    //////////////////////////////////////////////////////////////////

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        base.OnInitCharacter(character, initializer);

        _charInputs = character.charInputs;
        _charCapsule = character.charCapsule;
        _charMovement = character.charMovement;
    }

    public override void OnUpdateCharacter()
    {
        base.OnUpdateCharacter();

        _stateMachine.Update();
    }

    public virtual void SwitchView(Mode mode)
    {
        _stateMachine.Switch(mode);
    }

    //////////////////////////////////////////////////////////////////
    /// ViewMode States
    //////////////////////////////////////////////////////////////////

    protected virtual void EnterViewMode_Eyes()
    {
    }
    protected virtual void UpdateViewMode_Eyes()
    {
        if (_camera == null || _eyesData.eyes == null)
            return;

        _camera.transform.position = _eyesData.eyes.transform.position;
        _camera.transform.rotation = _eyesData.eyes.transform.rotation;
    }
    protected virtual void ExitViewMode_Eyes()
    {
    }

    protected virtual void EnterViewMode_FirstPerson()
    {
    }
    protected virtual void UpdateViewMode_FirstPerson()
    {
        if (_camera == null)
        {
            return;
        }

        _lookVector += _charInputs.look;
        _lookVector.x = _lookVector.x < -180 || _lookVector.x > 180 ? -_lookVector.x : _lookVector.x;
        _lookVector.y = _lookVector.y < -180 || _lookVector.y > 180 ? -_lookVector.y : _lookVector.y;
        _lookVector.z = _lookVector.z < -180 || _lookVector.z > 180 ? -_lookVector.z : _lookVector.z;

        _lookVector.x = Math.Clamp(_lookVector.x, _firstPersonData.minRotation.x, _firstPersonData.maxRotation.x);
        _lookVector.y = Math.Clamp(_lookVector.y, _firstPersonData.minRotation.y, _firstPersonData.maxRotation.y);
        _lookVector.z = Math.Clamp(_lookVector.z, _firstPersonData.minRotation.z, _firstPersonData.maxRotation.z);

        Vector3 cam_pos = Vector3.zero;
        Quaternion cam_rot = Quaternion.identity;

        // calculate position
        cam_pos = _firstPersonData.standPos;
        cam_pos = _charCapsule.GetPositionInVolume(cam_pos);

        // calculate rotation
        cam_rot = Quaternion.Euler(new Vector3(_lookVector.y, _lookVector.x, _lookVector.z));

        // apply position and rotation
        _camera.transform.position = cam_pos;
        _camera.transform.rotation = cam_rot;
    }
    protected virtual void ExitViewMode_FirstPerson()
    {
    }

    protected virtual void EnterViewMode_ThirdPerson()
    {
    }
    protected virtual void UpdateViewMode_ThirdPerson()
    {
        var data = _thirdPersonData;
        if (_camera == null || data.lookAtSource == null)
        {
            SwitchView(Mode.None);
            return;
        }

        Vector3 lookAt = data.lookAtSource.transform.position + data.lookAtSourceOffset;
        Vector3 lookDir = _charCapsule.forward;
        Vector3 camPos = _camera.transform.position;
        Quaternion camRot = Quaternion.LookRotation(lookDir, _charCapsule.up);

        bool hit = Physics.Raycast(lookAt, lookDir * -1, out RaycastHit hitInfo, data.maxDistanceFromLookSource);
        if (hit)
        {
            Vector3 targetCamPos = Math.Max(hitInfo.distance, data.minDistanceFromLookSource) * lookDir * -1;

            Vector3 lerpSpeed = Vector3.Distance(lookAt, camPos) > Vector3.Distance(lookAt, targetCamPos) ? data.deceleration : data.acceleration;
            if (lerpSpeed == Vector3.zero)
            {
                lerpSpeed = Vector3.one;
            }
            else
            {
                lerpSpeed = lerpSpeed * delta_time;
            }

            camPos.x = Mathf.Lerp(camPos.x, targetCamPos.x, lerpSpeed.x);
            camPos.y = Mathf.Lerp(camPos.y, targetCamPos.y, lerpSpeed.y);
            camPos.z = Mathf.Lerp(camPos.z, targetCamPos.z, lerpSpeed.z);

            _camera.transform.position = camPos;
            _camera.transform.rotation = camRot;
        }
    }
    protected virtual void ExitViewMode_ThirdPerson()
    {
    }
}