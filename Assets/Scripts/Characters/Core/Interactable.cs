using UnityEngine;

public class Interactable : MonoBehaviour
{
    //////////////////////////////////////////////////////////////////
    /// Static Members
    //////////////////////////////////////////////////////////////////

    public static Interactable GetInteractable(Collider collider)
    {
        if (collider == null)
        {
            return null;
        }

        Interactable interactable = collider.GetComponent<Interactable>();
        if (interactable == null)
        {
            InteractableReference interactableReference = collider.GetComponent<InteractableReference>();
            if (interactableReference == null)
            {
                return null;
            }

            interactable = interactableReference.getInteractable;
        }

        return interactable;
    }

    //////////////////////////////////////////////////////////////////
    /// Variables
    //////////////////////////////////////////////////////////////////

    [Header("INTERACTABLE"), Space]

    [SerializeField, ReadOnly] protected CharacterInteraction m_Interactor;
    public CharacterInteraction interactor => m_Interactor;

    [SerializeField] public bool canInteract;

    [SerializeField] protected bool m_RequireRaycast;
    public bool requireRaycast => m_RequireRaycast;

    [SerializeField, Vector3Range(0, 180)] protected Vector3 _faceAngle;
    public Vector3 faceAngle => _faceAngle;

    public bool CanInteract(CharacterInteraction interactor)
    {
        if (canInteract == false)
            return false;

        return true;
    }

    public void OnInteract(CharacterInteraction interactor)
    {
        if (canInteract == false)
            return;

        m_Interactor = interactor;
    }

    public void OnUnInteract()
    {
    }
}