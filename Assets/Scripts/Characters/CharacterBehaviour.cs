using UnityEngine;

[RequireComponent(typeof(Character))]
public abstract class CharacterBehaviour : MonoBehaviour
{
    public Character character { get; protected set; }

    public virtual void OnInitCharacter(Character character, CharacterInitializer initializer = null)
    {
        this.character = character;
    }

    public virtual void OnUpdateCharacter()
    {
    }

    public virtual void OnFixedUpdateCharacter()
    {
    }

    public virtual void OnDestroyCharacter()
    {
    }
}