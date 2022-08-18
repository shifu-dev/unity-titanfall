using System;
using GameLog;
using UnityEngine;

public static class LevelManager
{
    public static Level ActiveLevel => m_ActiveLevel;
    private static Level m_ActiveLevel;

    public static LevelRegistry registry { get; private set; }
    public static GameLog.ILogger logger { get; private set; }

    public static void Init()
    {
        logger = GameDebug.GetOrCreateLogger("LEVEL MANAGER");
        logger.Info("Initializing");
    }

    public static void Shutdown()
    {
        logger.Info("Shutting down");
    }

    public static async void LoadLevel(Level level)
    {
        logger.Info("Loading " + level.levelName + " Level");
        if (level == m_ActiveLevel)
        {
            logger.Info(level.levelName + " Level is already loaded");
            return;
        }

        await level.LoadLevelAsync();
        logger.Info("Loaded " + level.levelName + " Level successfully");

        m_ActiveLevel = level;
    }

    public static void LoadBootstrapLevel()
    {
        Level level = LevelRegistry.Instance.GetAsset("Bootstrap Level");
        if (level == null)
        {
            logger.Info("Bootstrap level could not be found");
            return;
        }
        
        LoadLevel(level);
    }
}