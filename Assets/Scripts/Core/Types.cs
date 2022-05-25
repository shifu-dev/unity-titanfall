using System;
using UnityEngine;

public struct slot
{
    private uint value;

    public slot(uint value)
    {
        this.value = value;
    }
}

[Serializable]
public struct weight
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

    public weight(float value)
    {
        this.m_value = Math.Clamp(value, min, max);
    }

    public weight(int value)
    {
        this.m_value = Math.Clamp(value, min, max);
    }

    public weight(long value)
    {
        this.m_value = Math.Clamp(value, min, max);
    }

    public weight(uint value)
    {
        this.m_value = Math.Clamp(value, min, max);
    }

    public weight(ulong value)
    {
        this.m_value = Math.Clamp(value, min, max);
    }
}