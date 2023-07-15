using System;
using UnityEngine;

public abstract class AssetRegistry<T> : ObjectRegistry<T>
    where T : DataAsset
{
    protected override void _OnAssetLoad(T asset)
    {
        base._OnAssetLoad(asset);

        asset.OnLoad();
    }

    protected override void _OnAssetUnload(T asset)
    {
        base._OnAssetUnload(asset);

        asset.OnUnload();
    }
}