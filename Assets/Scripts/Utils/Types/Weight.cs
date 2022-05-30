using System;
using UnityEngine;

[Serializable]
public struct Weight
{
    public const float min = 0;
    public const float max = 1;

    [SerializeField, Range(min, max)]
    private float m_value;

    public float value
    {
        get => m_value;
        set
        {
            m_value = Math.Clamp(value, min, max);
        }
    }

    public bool isMin => value == min;
    public bool isMax => value == max;

    public Weight(float value)
    {
        this.m_value = Math.Clamp(value, min, max);
    }

    public Weight(int value)
    {
        this.m_value = Math.Clamp(value, min, max);
    }

    public Weight(long value)
    {
        this.m_value = Math.Clamp(value, min, max);
    }

    public Weight(uint value)
    {
        this.m_value = Math.Clamp(value, min, max);
    }

    public Weight(ulong value)
    {
        this.m_value = Math.Clamp(value, min, max);
    }
}