using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class InteractionBound : MonoBehaviour
{
    public Action<Interactable> OnInteractableFound;
    public Action<Interactable> OnInteractableLost;

    protected void OnTriggerEnter(Collider collider)
    {
        if (OnInteractableFound == null)
            return;

        var interactable = collider.GetComponent<Interactable>();
        if (interactable == null)
            return;

        OnInteractableFound(interactable);
    }

    protected void OnTriggerExit(Collider collider)
    {
        if (OnInteractableLost == null)
            return;

        var interactable = collider.GetComponent<Interactable>();
        if (interactable == null)
            return;

        OnInteractableLost(interactable);
    }
}