using System;
using UnityEngine;

public class PlayerCamera : PlayerBehaviour
{
    protected Character _character;

    protected Camera _tppCamera;
    protected Camera _fppCamera;

    public override void OnPlayerCreate(Player player)
    {
        base.OnPlayerCreate(player);

        GameObject tppCameraObjectPrefab = Resources.Load<GameObject>("ThirdPersonCameraObject");
        GameObject fppCameraObjectPrefab = Resources.Load<GameObject>("FirstPersonCameraObject");

        GameObject tppCameraObject = GameObject.Instantiate(tppCameraObjectPrefab);
        GameObject fppCameraObject = GameObject.Instantiate(fppCameraObjectPrefab);

        tppCameraObject.name = "ThirdPersonPlayerCamera";
        fppCameraObject.name = "FirstPersonPlayerCamera";

        GameObject.DontDestroyOnLoad(tppCameraObject);
        GameObject.DontDestroyOnLoad(fppCameraObject);

        // tppCameraObject.SetActive(false);
        fppCameraObject.SetActive(false);

        _tppCamera = tppCameraObject.GetComponent<Camera>();
        _fppCamera = fppCameraObject.GetComponent<Camera>();
    }

    public override void OnPlayerPossess(Character character)
    {
        // unpossess if value is null
        if (character == null)
        {
            _character.charView.camera = null;

            _character = character;
            return;
        }

        _character = character;
        CharacterView charView = _character.charView;

        charView.camera = _tppCamera;
        charView.SwitchView(CharacterView.Mode.FirstPerson);
    }
}