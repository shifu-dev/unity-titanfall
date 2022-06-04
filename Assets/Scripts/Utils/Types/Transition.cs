using System;
using UnityEngine;

[Serializable]
public struct Transition
{
    [SerializeField] public AnimationCurve curve;

    public Transition(AnimationCurve curve)
    {
        this.curve = curve;
    }

    public float this[float weight]
    {
        get
        {
            if (curve == null)
                return 0;

            weight = Weight.Clamp(weight);
            return curve.Evaluate(weight * curve.length);
        }
    }
}