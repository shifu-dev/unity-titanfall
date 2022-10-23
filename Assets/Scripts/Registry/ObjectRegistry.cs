using System;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Logging;

public abstract class ObjectRegistry<T>
    where T : UnityEngine.Object
{
    public delegate void OnAssetRegisteredDelegate(T asset);
    public delegate void OnAssetUnregisteredDelegate(T asset);

    public void Init()
    {
        CreateLogger();
        _logger.Information("Initializing...");

        InternalInit();

        _logger.Information("Initialized Successfully");
    }

    public void Shutdown()
    {
        _logger.Information("Shutting down...");

        InternalShutdown();

        _logger.Information("Shutdown completed");
    }

    protected virtual void CreateLogger()
    {
        _logger = GameLog.CreateLogger($"{typeof(T).Name}Registry");
    }

    protected virtual void InternalInit()
    {
        LoadAssets();
    }

    protected virtual void InternalShutdown()
    {
        UnloadAssets();
    }

    public void LoadAssets()
    {
        _logger.Information("Loading assets...");

        _assets = Resources.LoadAll<T>("");
        foreach (var asset in _assets)
        {
            InternalOnAssetLoad(asset);
        }

        _logger.Information("Loaded {0} assets", _assets.Length);
    }

    public void UnloadAssets()
    {
        _logger.Information("Unloading {0} assets...", _assets.Length);

        foreach (var asset in _assets)
        {
            InternalOnAssetUnload(asset);
            Resources.UnloadAsset(asset);
        }

        _logger.Information("Unloaded assets successfully");
    }

    protected virtual void InternalOnAssetLoad(T asset)
    {
        _logger.Information("Loaded [{0}] asset", asset.name);
    }

    protected virtual void InternalOnAssetUnload(T asset)
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

    public OnAssetRegisteredDelegate OnAssetRegistered;
    public OnAssetUnregisteredDelegate OnAssetUnregistered;

    public IReadOnlyList<T> Assets => _assets;
    public int Count => _assets.Length;

    protected IGameLogger _logger;
    protected T[] _assets;
}