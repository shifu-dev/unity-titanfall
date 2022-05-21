using UnityEngine;

public class CharacterInteraction : CharacterBehaviour
{
    //////////////////////////////////////////////////////////////////
    /// Variables
    //////////////////////////////////////////////////////////////////

    public CharacterInputs charInputs { get; protected set; }
    public CharacterEquip charEquip { get; protected set; }

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

    public CharacterInteraction()
    {
    }

    //////////////////////////////////////////////////////////////////
    /// UpdateLoop
    //////////////////////////////////////////////////////////////////

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        base.OnInitCharacter(character, initializer);

        charInputs = character.characterInputs;
        charEquip = character.characterEquip;

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
            charEquip.OnGrenadeFound(grenade);
            return;
        }

        Weapon weapon = interactable.GetComponent<Weapon>();
        if (weapon != null)
        {
            charEquip.OnWeaponFound(weapon);
            return;
        }
    }

    protected void OnInteractableLost(Interactable interactable)
    {
    }
}