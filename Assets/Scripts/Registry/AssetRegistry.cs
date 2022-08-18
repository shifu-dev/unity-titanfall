using System;
using UnityEngine;

public abstract class AssetRegistry<T> : ObjectRegistry<T>
    where T : DataSource
{
    public AssetRegistry(string regName)
        : base(regName) { }

    protected override void InternalOnAssetLoad(T asset)
    {
        base.InternalOnAssetLoad(asset);

        asset.OnLoad();
    }

    protected override void InternalOnAssetUnload(T asset)
    {
        base.InternalOnAssetUnload(asset);

        asset.OnUnload();
    }
}