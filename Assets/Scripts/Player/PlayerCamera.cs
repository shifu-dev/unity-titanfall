using System;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Player Player { get => _Player; }
    [NonSerialized] protected Player _Player;

    Camera ThirdPersonCamera;
    Camera FirstPersonCamera;

    public void Init(Player player)
    {
        _Player = player;

        GameObject ThirdPersonCameraObjectPrefab = Resources.Load<GameObject>("ThirdPersonCameraObject");
        GameObject FirstPersonCameraObjectPrefab = Resources.Load<GameObject>("FirstPersonCameraObject");

        GameObject ThirdPersonCameraObject = GameObject.Instantiate(ThirdPersonCameraObjectPrefab);
        GameObject FirstPersonCameraObject = GameObject.Instantiate(FirstPersonCameraObjectPrefab);

        ThirdPersonCameraObject.name = "ThirdPersonPlayerCamera";
        FirstPersonCameraObject.name = "FirstPersonPlayerCamera";

        FirstPersonCameraObject.SetActive(false);

        ThirdPersonCamera = ThirdPersonCameraObject.GetComponent<Camera>();
        FirstPersonCamera = FirstPersonCameraObject.GetComponent<Camera>();
    }

    public void UpdateImpl()
    {
    }

    public virtual void OnPossessed(Character character)
    {
        if (character)
        {
            character.CharacterCamera.SetCamera(ThirdPersonCamera);
        }
    }

    public virtual void OnUnpossessed(Character character)
    {
        if (character)
        {
            character.CharacterCamera.UnsetCamera();
        }
    }
}