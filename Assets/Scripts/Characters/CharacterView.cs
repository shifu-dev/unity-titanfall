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
    public CharacterInputs CharacterInputs { get; protected set; }
    public CharacterCapsule CharacterCapsule { get; protected set; }

    public bool AlwaysUpdate = false;
    public CharacterViewModes CameraMode;
    public Camera Camera;
    public GameObject LookAtSource;
    public Vector3 LookAtSourceOffset;
    public Vector3 Acceleration;
    public Vector3 Deceleration;
    public Vector3 MinRotation = new Vector3(-89.9f, 0, 0);
    public Vector3 MaxRotation = new Vector3(89.9f, 0, 0);
    public float MaxDistanceFromLookSource = 10;
    public float MinDistanceFromLookSource = 2;

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        CharacterInputs = character.charInputs;
        CharacterCapsule = character.charCapsule;

        CameraMode = CharacterViewModes.None;
    }

    public override void OnUpdateCharacter()
    {
        Vector3 totalRawInput = CharacterInputs.lookProcessed;
        Vector3 lookVector = new Vector3(totalRawInput.y, totalRawInput.x, totalRawInput.z);
        lookVector.x = Math.Clamp(lookVector.x, MinRotation.x, MaxRotation.x);
        lookVector.y = Math.Clamp(lookVector.y, MinRotation.y, MaxRotation.y);
        lookVector.z = Math.Clamp(lookVector.z, MinRotation.z, MaxRotation.z);

        if (Camera && LookAtSource)
        {
            Vector3 lookAt = LookAtSource.transform.position + LookAtSourceOffset;
            Vector3 lookDir = CharacterCapsule.forward;
            Vector3 camPos = Camera.transform.position;
            Quaternion camRot = Quaternion.LookRotation(lookDir, CharacterCapsule.up);

            bool hit = Physics.Raycast(lookAt, lookDir * -1, out RaycastHit hitInfo, MaxDistanceFromLookSource);
            if (hit)
            {
                Vector3 targetCamPos = Math.Max(hitInfo.distance, MinDistanceFromLookSource) * lookDir * -1;

                Vector3 lerpSpeed = Vector3.Distance(lookAt, camPos) > Vector3.Distance(lookAt, targetCamPos) ? Deceleration : Acceleration;
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
            }

            camPos = transform.position + new Vector3(0, 1, -2);

            Camera.transform.position = camPos;
            Camera.transform.rotation = camRot;
        }
    }

    public void SetCamera(Camera camera)
    {
        if (camera == null)
            return;

        Camera = camera;
    }

    public void UnsetCamera()
    {
        Camera = null;
    }

    public virtual void SetCameraMode(CharacterViewModes NewMode)
    {
    }
}