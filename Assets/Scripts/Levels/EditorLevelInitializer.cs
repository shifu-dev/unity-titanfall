using UnityEngine;
using UnityEngine.AddressableAssets;
using GameFramework.LevelManagement;

public class EditorLevelInitializer : MonoBehaviour
{
    static bool isInitDone = false;

    private void Awake()
    {
        if (isInitDone) return;
        isInitDone = true;

        Debug.Log("Initializing Addressables...");
        Addressables.InitializeAsync(autoReleaseHandle: true)
            .WaitForCompletion();

        Debug.Log("Initializing PlayerManagerSystem...");
        PlayerManager.System = new PlayerManagerSystem();

        Debug.Log($"Loading level asset{_level.SubObjectName}...");
        LevelAsset level = _level.LoadAssetAsync()
            .WaitForCompletion();

        Debug.Log($"Loading level{level.name}...");
        level.PerformLoad();
    }

    [SerializeField]
    AssetReferenceLevel _level;
}