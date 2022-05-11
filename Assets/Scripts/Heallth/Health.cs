using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField, Min(0)] protected float m_Health;
    public float health
    {
        get => m_Health;
        set
        {
            m_Health = value;
        }
    }
}