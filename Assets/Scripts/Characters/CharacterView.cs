using System;
using UnityEngine;

public enum CharacterViewModes
{
    None,
    Eyes,
    FirstPerson,
    ThirdPerson,
    ThirdPersonLeftShoulder,
    ThirdPersonRightShoulder,
    Cinematic
}

[DisallowMultipleComponent]
public class CharacterView : CharacterBehaviour
{
    public CharacterInputs charInputs { get; protected set; }
    public CharacterCapsule charCapsule { get; protected set; }
    public CharacterMovement charMovement { get; protected set; }

    [SerializeField] protected Camera m_camera;
    [SerializeField] protected CharacterViewModes m_viewMode;
    [SerializeField] protected CharacterViewModes m_nextViewMode;
    [SerializeField] protected Weight m_weight;

    [SerializeField] protected CharViewModeData_Eyes m_EyesData;
    [SerializeField] protected CharViewModeData_FirstPerson m_FirstPersonData;
    [SerializeField] protected CharViewModeData_ThirdPerson m_ThirdPersonData;
    [SerializeField] protected CharViewModeData_ThirdPersonLeftShoulder m_ThirdPersonLeftShoulderData;
    [SerializeField] protected CharViewModeData_ThirdPersonRightShoulder m_ThirdPersonRightShoulderData;
    [SerializeField] protected CharViewModeData_Cinematic m_CinematicData;

    [SerializeField, ReadOnly] protected Vector3 m_lookVector;
    public Vector2 lookVector => m_lookVector;

    public float turnAngle => m_lookVector.y;

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        base.OnInitCharacter(character, initializer);

        charInputs = character.charInputs;
        charCapsule = character.charCapsule;
        charMovement = character.charMovement;

        if (m_nextViewMode == CharacterViewModes.None)
            m_weight.value = 1;
    }

    public override void OnUpdateCharacter()
    {
        switch (m_viewMode)
        {
            case CharacterViewModes.Eyes: UpdateViewMode_Eyes(m_weight); break;
            case CharacterViewModes.FirstPerson: UpdateViewMode_FirstPerson(m_weight); break;
            case CharacterViewModes.ThirdPerson: UpdateViewMode_ThirdPerson(m_weight); break;
            case CharacterViewModes.ThirdPersonLeftShoulder: UpdateViewMode_ThirdPersonLeftShoulder(m_weight); break;
            case CharacterViewModes.ThirdPersonRightShoulder: UpdateViewMode_ThirdPersonRightShoulder(m_weight); break;
            case CharacterViewModes.Cinematic: UpdateViewMode_Cinematic(m_weight); break;
            default: break;
        }
    }

    public void SetCamera(Camera camera)
    {
        if (camera == null)
            return;

        this.m_camera = camera;
    }

    public void UnsetCamera()
    {
        m_camera = null;
    }

    public virtual void SetViewMode(CharacterViewModes NewMode)
    {
    }

    protected virtual void UpdateViewMode_Eyes(Weight weight)
    {
    }

    protected virtual void UpdateViewMode_FirstPerson(Weight weight)
    {
        ref var data = ref m_FirstPersonData;
        if (m_camera == null || data.pos == null)
        {
            SetViewMode(CharacterViewModes.None);
            return;
        }

        Vector3 input = charInputs.look;
        data.look.x = Math.Clamp(data.look.x + input.x, data.minRotation.x, data.maxRotation.x);
        data.look.y = Math.Clamp(data.look.y + input.y, data.minRotation.y, data.maxRotation.y);
        data.look.z = Math.Clamp(data.look.z + input.z, data.minRotation.z, data.maxRotation.z);
        m_lookVector = data.look;

        m_camera.transform.position = data.pos.position;
        m_camera.transform.rotation = Quaternion.Euler(data.look);
    }

    protected virtual void UpdateViewMode_ThirdPerson(Weight weight)
    {
        var data = m_ThirdPersonData;
        if (m_camera == null || data.lookAtSource == null)
        {
            SetViewMode(CharacterViewModes.None);
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

    protected virtual void UpdateViewMode_ThirdPersonLeftShoulder(Weight weight)
    {
    }

    protected virtual void UpdateViewMode_ThirdPersonRightShoulder(Weight weight)
    {
    }

    protected virtual void UpdateViewMode_Cinematic(Weight weight)
    {
    }
}

[Serializable]
public struct CharViewModeData_Eyes
{
}

[Serializable]
public struct CharViewModeData_FirstPerson
{
    public Transform pos;
    public Vector3 look;
    public Vector3 minRotation;
    public Vector3 maxRotation;
}

[Serializable]
public struct CharViewModeData_ThirdPerson
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

[Serializable]
public struct CharViewModeData_ThirdPersonLeftShoulder
{
}

[Serializable]
public struct CharViewModeData_ThirdPersonRightShoulder
{
}

[Serializable]
public struct CharViewModeData_Cinematic
{
}
