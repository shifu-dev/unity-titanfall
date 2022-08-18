using UnityEngine;

public abstract class DataSource : ScriptableObject
{
    public virtual void OnLoad()
    {
    }

    public virtual void OnUnload()
    {
    }
}