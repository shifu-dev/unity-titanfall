using System;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Logging;

using UnityObject = UnityEngine.Object;

public abstract class ObjectRegistry<T>
    where T : UnityObject
{
    public delegate void OnAssetRegisteredDelegate(T asset);
    public delegate void OnAssetUnregisteredDelegate(T asset);

    public void Init()
    {
        _CreateLogger();
        _logger.Information("Initializing...");

        _Init();

        _logger.Information("Initialized Successfully");
    }

    public void Shutdown()
    {
        _logger.Information("Shutting down...");

        _Shutdown();

        _logger.Information("Shutdown completed");
    }

    protected virtual void _CreateLogger()
    {
        _logger = GameLog.System.CreateLogger($"{typeof(T).Name}Registry");
    }

    protected virtual void _Init()
    {
        LoadAssets();
    }

    protected virtual void _Shutdown()
    {
        UnloadAssets();
    }

    public void LoadAssets()
    {
        _logger.Information("Loading assets...");

        _assets = Resources.LoadAll<T>("");
        foreach (var asset in _assets)
        {
            _OnAssetLoad(asset);
        }

        _logger.Information("Loaded {0} assets", _assets.Length);
    }

    public void UnloadAssets()
    {
        _logger.Information("Unloading {0} assets...", _assets.Length);

        foreach (var asset in _assets)
        {
            _OnAssetUnload(asset);
            Resources.UnloadAsset(asset);
        }

        _logger.Information("Unloaded assets successfully");
    }

    protected virtual void _OnAssetLoad(T asset)
    {
        _logger.Information("Loaded [{0}] asset", asset.name);
    }

    protected virtual void _OnAssetUnload(T asset)
    {
        _logger.Information("Unloading [{0}] asset", asset.name);
    }

    public T GetAsset(string charName)
    {
        if (String.IsNullOrEmpty(charName))
        {
            return null;
        }

        foreach (var asset in _assets)
        {
            if (asset.name == charName)
            {
                return asset;
            }
        }

        return null;
    }

    public T GetAsset(int index)
    {
        if (index < 0 || index > _assets.Length - 1)
        {
            return null;
        }

        return _assets[index];
    }

    public OnAssetRegisteredDelegate AssetRegisteredEvent;
    public OnAssetUnregisteredDelegate AssetUnregisteredEvent;

    public IReadOnlyList<T> Assets => _assets;
    public int Count => _assets.Length;

    protected IGameLogger _logger;
    protected T[] _assets;
}