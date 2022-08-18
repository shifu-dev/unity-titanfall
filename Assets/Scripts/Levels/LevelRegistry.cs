using UnityEngine;

public class LevelRegistry : AssetRegistry<Level>
{
    public static readonly LevelRegistry Instance = new LevelRegistry();

    public const string LOG_CATEGORY = "LEVEL ASSET REGISTRY";

    public LevelRegistry() : base(LOG_CATEGORY) { }
}