using UnityEngine;

public class LevelRegistry : AssetRegistry<Level>
{
    public static readonly LevelRegistry Instance = new LevelRegistry();
}