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

    public CharacterInputs charInputs { get; protected set; }
    public CharacterCapsule charCapsule { get; protected set; }
    public CharacterMovement charMovement { get; protected set; }

    [SerializeField] protected Camera m_camera;
    public new Camera camera
    {
        get => m_camera;
        set => m_camera = value;
    }

    [SerializeField, ReadOnly] protected Vector3 m_lookVector;
    public Vector2 lookVector => m_lookVector;

    public Mode mode
    {
        get => m_stateMachine.currentState;
        set => m_stateMachine.Switch(value);
    }

    [Header("View Modes Data"), Space]
    [SerializeField] protected EyesData m_eyesData;
    [SerializeField] protected FirstPersonData m_firstPersonData;
    [SerializeField] protected ThirdPersonData m_thirdPersonData;

    protected StateMachine<Mode> m_stateMachine;

    public float turnAngle => m_lookVector.x;

    public CharacterView()
    {
        m_stateMachine = new StateMachine<Mode>();
        m_stateMachine.Add(Mode.None, null, null, null);
        m_stateMachine.Add(Mode.Eyes, EnterViewMode_Eyes, UpdateViewMode_Eyes, ExitViewMode_Eyes);
        m_stateMachine.Add(Mode.FirstPerson, EnterViewMode_FirstPerson, UpdateViewMode_FirstPerson, ExitViewMode_FirstPerson);
        m_stateMachine.Add(Mode.ThirdPerson, EnterViewMode_ThirdPerson, UpdateViewMode_ThirdPerson, ExitViewMode_ThirdPerson);
        m_stateMachine.Switch(Mode.None);
    }

    //////////////////////////////////////////////////////////////////
    /// Updates
    //////////////////////////////////////////////////////////////////

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        base.OnInitCharacter(character, initializer);

        charInputs = character.charInputs;
        charCapsule = character.charCapsule;
        charMovement = character.charMovement;
    }

    public override void OnUpdateCharacter()
    {
        m_stateMachine.Update();
    }

    public virtual void SwitchView(Mode mode)
    {
        m_stateMachine.Switch(mode);
    }

    //////////////////////////////////////////////////////////////////
    /// ViewMode States
    //////////////////////////////////////////////////////////////////

    protected virtual void EnterViewMode_Eyes()
    {
    }
    protected virtual void UpdateViewMode_Eyes()
    {
        if (m_camera == null || m_eyesData.eyes == null)
            return;

        m_camera.transform.position = m_eyesData.eyes.transform.position;
        m_camera.transform.rotation = m_eyesData.eyes.transform.rotation;
    }
    protected virtual void ExitViewMode_Eyes()
    {
    }

    protected virtual void EnterViewMode_FirstPerson()
    {
    }
    protected virtual void UpdateViewMode_FirstPerson()
    {
        if (m_camera == null)
        {
            return;
        }

        m_lookVector += charInputs.look;
        m_lookVector.x = m_lookVector.x < -180 || m_lookVector.x > 180 ? -m_lookVector.x : m_lookVector.x;
        m_lookVector.y = m_lookVector.y < -180 || m_lookVector.y > 180 ? -m_lookVector.y : m_lookVector.y;
        m_lookVector.z = m_lookVector.z < -180 || m_lookVector.z > 180 ? -m_lookVector.z : m_lookVector.z;

        m_lookVector.x = Math.Clamp(m_lookVector.x, m_firstPersonData.minRotation.x, m_firstPersonData.maxRotation.x);
        m_lookVector.y = Math.Clamp(m_lookVector.y, m_firstPersonData.minRotation.y, m_firstPersonData.maxRotation.y);
        m_lookVector.z = Math.Clamp(m_lookVector.z, m_firstPersonData.minRotation.z, m_firstPersonData.maxRotation.z);

        Vector3 camPos = Vector3.zero;
        Quaternion camRot = Quaternion.identity;

        // calculate position
        camPos = m_firstPersonData.standPos;
        camPos = charCapsule.GetPositionInVolume(camPos);

        // calculate rotation
        camRot = Quaternion.Euler(new Vector3(m_lookVector.y, m_lookVector.x, m_lookVector.z));

        // apply position and rotation
        m_camera.transform.position = camPos;
        m_camera.transform.rotation = camRot;
    }
    protected virtual void ExitViewMode_FirstPerson()
    {
    }

    protected virtual void EnterViewMode_ThirdPerson()
    {
    }
    protected virtual void UpdateViewMode_ThirdPerson()
    {
        var data = m_thirdPersonData;
        if (m_camera == null || data.lookAtSource == null)
        {
            SwitchView(Mode.None);
            return;
        }

        Vector3 lookAt = data.lookAtSource.transform.position + data.lookAtSourceOffset;
        Vector3 lookDir = charCapsule.forward;
        Vector3 camPos = m_camera.transform.position;
        Quaternion camRot = Quaternion.LookRotation(lookDir, charCapsule.up);

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
                lerpSpeed = lerpSpeed * Time.deltaTime;
            }

            camPos.x = Mathf.Lerp(camPos.x, targetCamPos.x, lerpSpeed.x);
            camPos.y = Mathf.Lerp(camPos.y, targetCamPos.y, lerpSpeed.y);
            camPos.z = Mathf.Lerp(camPos.z, targetCamPos.z, lerpSpeed.z);

            m_camera.transform.position = camPos;
            m_camera.transform.rotation = camRot;
        }
    }
    protected virtual void ExitViewMode_ThirdPerson()
    {
    }
}