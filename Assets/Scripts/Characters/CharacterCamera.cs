using System;
using UnityEngine;

public enum CharacterCameraModes
{
    None,
    Eyes,
    FirstPerson,
    ThirdPerson,
    ThirdPersonLeftShoulder,
    ThirdPersonRightShoulder,
    Cinematic
}

public class CharacterCamera : MonoBehaviour
{
    public Character Character { get => _Character; }
    protected Character _Character;

    public CharacterInputs CharacterInputs { get => _CharacterInputs; }
    protected CharacterInputs _CharacterInputs;

    public Camera Camera;

    public CharacterCameraModes CameraMode { get => _CameraMode;  }
    protected CharacterCameraModes _CameraMode;

    public GameObject FirstPersonCameraSource;
    public float FirstPersonMaxRotationY = 89.9f;
    public float FirstPersonMinRotationY = -89.9f;

    public void Init(Character character)
    {
        _Character = character;
        _CharacterInputs = _Character.CharacterInputs;

        _CameraMode = CharacterCameraModes.None;
    }

    public void UpdateImpl()
    {
        if (Camera && FirstPersonCameraSource)
        {
            var lookInput = _CharacterInputs.LookInputVector;
            var camPos = FirstPersonCameraSource.transform.position;
            var currentCamRot = Camera.transform.rotation.eulerAngles;
            var camRotX = Math.Clamp(lookInput.x + currentCamRot.x, FirstPersonMinRotationY, FirstPersonMaxRotationY);
            var camRotY = currentCamRot.y + lookInput.y;
            var camRotZ = 0;
            var camRot = Quaternion.Euler(camRotX, camRotY, camRotZ);

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

    public virtual void SetCameraMode(CharacterCameraModes NewMode)
    {
    }
}