using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField, ReadOnly] protected CharacterInteraction m_Interactor;
    public CharacterInteraction interactor => m_Interactor;

    [SerializeField] public bool canInteract;

    [SerializeField] protected bool m_RequireRaycast;
    public bool requireRaycast => m_RequireRaycast;

    [SerializeField, Range(0, 180)] protected float m_FaceAngleX;
    [SerializeField, Range(0, 180)] protected float m_FaceAngleY;
    [SerializeField, Range(0, 180)] protected float m_FaceAngleZ;
    public Vector3 requireFaceAngle => new Vector3(m_FaceAngleX, m_FaceAngleY, m_FaceAngleZ);

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