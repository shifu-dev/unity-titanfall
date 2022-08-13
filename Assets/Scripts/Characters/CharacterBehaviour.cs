using UnityEngine;

[RequireComponent(typeof(Character))]
public abstract class CharacterBehaviour : MonoBehaviour
{
    public Character character => _character;
    protected Character _character;

    public virtual void OnCharacterCreate(Character character, CharacterInitializer initializer)
    {
        _character = character;
    }

    public virtual void OnCharacterSpawn() { }

    public virtual void OnCharacterPreUpdate() { }

    public virtual void OnCharacterUpdate() { }

    public virtual void OnCharacterPostUpdate() { }

    public virtual void OnCharacterFixedUpdate() { }

    public virtual void OnCharacterDead() { }

    public virtual void OnCharacterDespawn() { }

    public virtual void OnCharacterDestroy() { }

    public virtual void OnCharacterPossess(Controller controller) { }

    public virtual void OnCharacterReset() { }
}