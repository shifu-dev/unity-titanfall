using System;
using UnityEngine;

[Serializable]
public struct Weight
{
    public static readonly Weight Min = new Weight(min);
    public static readonly Weight Max = new Weight(max);

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

public interface IWeightMover
{
    Weight Move(Weight current, Weight target);
}

public struct LerpWeightMover : IWeightMover
{
    public float speed;

    public LerpWeightMover(float speed)
    {
        this.speed = speed;
    }

    public Weight Move(Weight current, Weight target)
    {
        return new Weight(Mathf.Lerp(current.value, target.value, speed * Time.deltaTime));
    }
}

public struct StepWeightMover : IWeightMover
{
    public float step;

    public StepWeightMover(float speed)
    {
        this.step = speed;
    }

    public Weight Move(Weight current, Weight target)
    {
        return new Weight(Mathf.MoveTowards(current.value, target.value, step * Time.deltaTime));
    }
}

public struct SnapWeightMover : IWeightMover
{
    public Weight Move(Weight current, Weight target)
    {
        return target;
    }
}
