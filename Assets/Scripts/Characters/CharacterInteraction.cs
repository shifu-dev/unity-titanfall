using UnityEngine;

public class CharacterInteraction : CharacterBehaviour
{
    //////////////////////////////////////////////////////////////////
    /// Variables
    //////////////////////////////////////////////////////////////////

    public CharacterWeapon CharacterWeapon => character.characterWeapon;
    public CharacterInputs CharacterInputs => character.characterInputs;

    [SerializeField, Space] protected InteractionBound m_InteractionBound;
    public InteractionBound InteractionBound
    {
        get => m_InteractionBound;
        set
        {
            if (value != null)
            {
                value.OnInteractableFound = this.OnInteractableFound;
                value.OnInteractableLost = this.OnInteractableLost;
            }

            m_InteractionBound = value;
        }
    }

    [SerializeField] protected InteractionRay m_InteractionRay;
    public InteractionRay InteractionRay
    {
        get => m_InteractionRay;
        set => m_InteractionRay = value;
    }

    //////////////////////////////////////////////////////////////////
    /// Constructors
    //////////////////////////////////////////////////////////////////

    public CharacterInteraction()
    {
    }

    //////////////////////////////////////////////////////////////////
    /// UpdateLoop
    //////////////////////////////////////////////////////////////////

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        base.OnInitCharacter(character, initializer);

        // this sets up the events
        InteractionBound = InteractionBound;
    }

    public override void OnUpdateCharacter()
    {
        if (m_InteractionRay)
        {
            OnInteractableFound(m_InteractionRay.FindInteractable());
        }
    }

    //////////////////////////////////////////////////////////////////
    /// Interactables
    //////////////////////////////////////////////////////////////////

    protected void OnInteractableFound(Interactable interactable)
    {
        if (interactable == null)
            return;

        Grenade grenade = interactable.GetComponent<Grenade>();
        if (grenade != null)
        {
            CharacterWeapon.OnGrenadeFound(grenade);
            return;
        }
    }

    protected void OnInteractableLost(Interactable interactable)
    {
    }
}