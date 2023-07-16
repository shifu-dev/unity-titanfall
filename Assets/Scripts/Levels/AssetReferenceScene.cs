using System;
using UnityEditor;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR

[Serializable]
public class AssetReferenceScene : AssetReferenceT<SceneAsset>
{
    public AssetReferenceScene(string guid) : base(guid) { }
}

#else

[Serializable]
public class AssetReferenceScene : AssetReference { }

#endif