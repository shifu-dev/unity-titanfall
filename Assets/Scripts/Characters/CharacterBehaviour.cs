using UnityEngine;

[RequireComponent(typeof(Character))]
public abstract class CharacterBehaviour : MonoBehaviour
{
    public Character character { get; protected set; }
    protected float delta_time { get; private set; }
    protected int frame_count { get; private set; }

    public virtual void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        this.character = character;
        delta_time = .1f;
        frame_count = 0;
    }

    public virtual void OnUpdateCharacter()
    {
        delta_time = Time.deltaTime;
        frame_count = Time.frameCount;
    }

    public virtual void OnFixedUpdateCharacter()
    {
        delta_time = Time.deltaTime;
    }

    public virtual void OnDestroyCharacter() { }

    public virtual void OnPossessed(Player player) { }

    public virtual void OnUnPossessed() { }

}