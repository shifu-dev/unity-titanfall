using UnityEngine;

public class PlayerInteraction : PlayerBehaviour
{
    [SerializeField] protected LayerMask _layerMask;
    [SerializeField] protected QueryTriggerInteraction _triggerQuery;

    protected Transform _raycastSource;
    protected float _raycastLength;
    protected Interactable _interactable;

    public PlayerInteraction()
    {
        _layerMask = default;
        _triggerQuery = QueryTriggerInteraction.Collide;
        _raycastSource = null;
        _raycastLength = 3;
    }

    public override void OnPlayerUpdate()
    {
        _interactable = null;

        if (_raycastSource is not null)
        {
            bool hit = Physics.Raycast(_raycastSource.position, _raycastSource.forward, out RaycastHit hitInfo,
                _raycastLength, _layerMask, _triggerQuery);

            if (hit)
            {
                _interactable = Interactable.GetInteractable(hitInfo.collider);
            }
        }
    }

    public override void OnPlayerPossess(Character character)
    {
        if (character is not null)
        {
            _raycastSource = _player.playerCamera.tppCamera.transform;
        }
        else
        {
            _raycastSource = null;
        }
    }

    public Interactable GetInteractable()
    {
        return _interactable;
    }
}