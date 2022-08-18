using System;
using System.Collections.Generic;
using UnityEngine;
using GameLog;

using ILogger = GameLog.ILogger;

public abstract class ObjectRegistry<T>
    where T : UnityEngine.Object
{
    public readonly string LogCategory;

    public delegate void OnAssetRegisteredDelegate(T asset);
    public delegate void OnAssetUnregisteredDelegate(T asset);

    public ObjectRegistry(string logCategory)
    {
        LogCategory = logCategory;
    }

    public void Init()
    {
        _logger = GameDebug.GetOrCreateLogger(LogCategory);
        _logger.Info("Initializing...");

        InternalInit();

        _logger.Info("Initialized Successfully");
    }

    public void Shutdown()
    {
        _logger.Info("Shutting down...");

        InternalShutdown();

        _logger.Info("Shutdown completed");
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
        _logger.Info("Loading assets...");

        _assets = Resources.LoadAll<T>("");
        foreach (var asset in _assets)
        {
            InternalOnAssetLoad(asset);
        }

        _logger.Info("Loaded {0} assets", _assets.Length);
    }

    public void UnloadAssets()
    {
        _logger.Info("Unloading {0} assets...", _assets.Length);

        foreach (var asset in _assets)
        {
            InternalOnAssetUnload(asset);
            Resources.UnloadAsset(asset);
        }

        _logger.Info("Unloaded assets successfully");
    }

    protected virtual void InternalOnAssetLoad(T asset)
    {
        _logger.Info("Loaded [{0}] asset", asset.name);
    }

    protected virtual void InternalOnAssetUnload(T asset)
    {
        _logger.Info("Unloading [{0}] asset", asset.name);
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

    private ILogger _logger;
    private T[] _assets;
}