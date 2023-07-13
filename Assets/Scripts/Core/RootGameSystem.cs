using UnityEngine;
using GameFramework;
using GameFramework.LevelManagement;

[RegisterGameSystem(typeof(TitanfallRootGameSystem))]
class TitanfallRootGameSystem : GameSystem
{
    protected override void OnRegistered(GameSystem system)
    {
        base.OnRegistered(system);

        Debug.Log("TitanfallRootGameSystem registered");
        LevelManager.Registry.GetLevel("BootstrapLevel", out LevelAsset bootstrapLevel);
        LevelManager.LoadLevelAsync(bootstrapLevel);
    }
}