using UnityEngine;
using UnityEngine.AddressableAssets;
using GameFramework;
using GameFramework.LevelManagement;

[RegisterGameSystem(typeof(TitanfallRootGameSystem))]
class TitanfallRootGameSystem : GameSystem
{
    protected override void OnRegistered(GameSystem system)
    {
        Debug.Log("Initializing Addressables...");
        Addressables.InitializeAsync();
        Debug.Log("Initialized Addressables.");

        LevelManager.Registry.GetLevel("BootstrapLevel", out LevelAsset bootstrapLevel);
        LevelManager.LoadLevelAsync(bootstrapLevel);
    }
}