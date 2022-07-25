using System;
using UnityEngine;

public class Controller : MonoBehaviour
{
    protected Character _character;
    public Character character => _character;

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
    }

    protected virtual void Update()
    {
    }

    public virtual void Possess(Character character)
    {
        _character = character;
        _character.OnPossess(this);

        OnPossess(_character);
    }

    protected virtual void OnPossess(Character character)
    {
    }
}