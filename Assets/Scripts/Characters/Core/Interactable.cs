using UnityEngine;

public class Interactable : MonoBehaviour
{
    public static Interactable GetInteractable(Collider collider)
    {
        if (collider is null)
        {
            return null;
        }

        Interactable interactable = collider.GetComponent<Interactable>();
        if (interactable is null)
        {
            InteractableReference interactableReference = collider.GetComponent<InteractableReference>();
            if (interactableReference is null)
            {
                return null;
            }

            interactable = interactableReference.getInteractable;
        }

        return interactable;
    }

    [Header("INTERACTABLE"), Space]

    [SerializeField, ReadOnly] protected Character _Interactor;
    public Character interactor => _Interactor;

    [SerializeField] public bool canInteract;

    [SerializeField] protected bool _RequireRaycast;
    public bool requireRaycast => _RequireRaycast;

    [SerializeField, Vector3Range(0, 180)] protected Vector3 _faceAngle;
    public Vector3 faceAngle => _faceAngle;

    public bool CanInteract(Character interactor)
    {
        if (canInteract == false)
            return false;

        return true;
    }

    public void OnInteract(Character interactor)
    {
        if (canInteract == false)
            return;

        _Interactor = interactor;
    }

    public void OnUnInteract()
    {
    }
}