using System;
using UnityEngine.AddressableAssets;
using GameFramework.LevelManagement;

[Serializable]
public class AssetReferenceLevel : AssetReferenceT<LevelAsset>
{
    public AssetReferenceLevel(string guid) : base(guid) { }
}