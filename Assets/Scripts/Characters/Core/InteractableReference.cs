using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class InteractableReference : MonoBehaviour
{
    [SerializeField] private Interactable _interactable;
    public Interactable getInteractable => _interactable;
}