using UnityEngine;

public class CharacterInteraction : CharacterBehaviour
{
    //////////////////////////////////////////////////////////////////
    /// Variables
    //////////////////////////////////////////////////////////////////

    public CharacterInputs characterInputs => character.characterInputs;
    public CharacterWeapon characterWeapon => character.characterWeapon;
    public CharacterGrenade characterGrenade => character.characterGrenade;

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
            Interactable interactable = m_InteractionRay.FindInteractable();
            if (interactable != null)
            {
                OnInteractableFound(interactable);
            }
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
            Debug.Log($"OnGrenadeFound : {grenade}");
            characterGrenade.OnGrenadeFound(grenade);
            return;
        }
    }

    protected void OnInteractableLost(Interactable interactable)
    {
    }
}