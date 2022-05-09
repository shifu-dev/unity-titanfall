using UnityEngine;

[DisallowMultipleComponent]
public class InteractionRay : MonoBehaviour
{
    [SerializeField, Min(0)] protected float m_RaycastLength;

    public Interactable FindInteractable()
    {
        if (m_RaycastLength < 0.0000001f)
        {
            return null;
        }

        bool hit = Physics.Raycast(transform.position, transform.forward, 
            out RaycastHit hitInfo, m_RaycastLength, 0, QueryTriggerInteraction.Ignore);

        if (hit == false)
        {
            return null;
        }

        Interactable interactable = hitInfo.collider.GetComponent<Interactable>();
        return interactable;
    }
}