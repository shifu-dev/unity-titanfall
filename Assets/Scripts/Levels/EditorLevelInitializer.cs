using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using GameFramework.LevelManagement;

[Serializable]
public class AssetReferenceLevel : AssetReferenceT<LevelAsset>
{
    public AssetReferenceLevel(string guid) : base(guid) { }
}

public class EditorLevelInitializer : MonoBehaviour
{
    private async void Awake()
    {
        Debug.Log("Initializing Addressables...");
        await Addressables.InitializeAsync(autoReleaseHandle: true).Task;
        Debug.Log("Initialized Addressables.");

        Debug.Log("Initializing PlayerManagerSystem...");
        PlayerManager.System = new PlayerManagerSystem();
        Debug.Log("Initializing PlayerManagerSystem.");

        Debug.Log($"Loading level asset{_levelAsset.SubObjectName}...");
        LevelAsset level = await _levelAsset.LoadAssetAsync().Task;
        Debug.Log("Loaded level asset.");

        Debug.Log($"Loading level{level.name}...");
        level.PerformLoad();
        Debug.Log($"Loaded level.");
    }

    [SerializeField]
    AssetReferenceLevel _levelAsset;
}