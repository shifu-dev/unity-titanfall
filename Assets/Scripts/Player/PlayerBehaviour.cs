using UnityEngine;

[RequireComponent(typeof(Player))]
public abstract class PlayerBehaviour : MonoBehaviour
{
    protected Player _player;
    public Player player => _player;

    public virtual void OnPlayerCreate(Player player)
    {
        _player = player;
    }

    public virtual void OnPlayerPreUpdate() { }

    public virtual void OnPlayerUpdate() { }

    public virtual void OnPlayerPostUpdate() { }

    public virtual void OnPlayerFixedUpdate() { }

    public virtual void OnPlayerDestroy() { }

    public virtual void OnPlayerPossess(Character character) { }

    public virtual void OnPlayerReset() { }
}