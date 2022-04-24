using System;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class CharacterCollision : MonoBehaviour
{
    public Rigidbody Rigidbody { get => _Rigidbody; }

    [NonSerialized] protected Rigidbody _Rigidbody;

    public void Init(Character character)
    {
        _Rigidbody = GetComponent<Rigidbody>();
    }

    public void UpdateImpl()
    {
    }
}