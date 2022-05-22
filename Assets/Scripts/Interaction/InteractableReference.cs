using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class InteractableReference : MonoBehaviour
{
    [SerializeField] private Interactable m_interactable;
    public Interactable getInteractable => m_interactable;
}