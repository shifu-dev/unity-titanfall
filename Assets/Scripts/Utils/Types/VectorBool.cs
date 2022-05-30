using System;

[Serializable]
public struct VectorBool
{
    public const bool defaultBoolValue = false;

    public bool x;
    public bool y;
    public bool z;

    public VectorBool(bool x, bool y, bool z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public VectorBool(bool x, bool y)
    {
        this.x = x;
        this.y = y;
        this.z = defaultBoolValue;
    }

    public VectorBool(bool value)
    {
        this.x = value;
        this.y = value;
        this.z = value;
    }
}