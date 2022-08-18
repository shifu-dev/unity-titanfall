using UnityEngine;

public abstract class DataAsset : ScriptableObject
{
    public virtual void OnLoad()
    {
    }

    public virtual void OnUnload()
    {
    }
}