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
        logger = GameDebug.GetOrCreateLogger("LEVELMANAGER");
        logger.Info("Initalizing");

        registry = Resources.Load<LevelRegistry>("LevelRegistry");
        if (registry == null)
        {
            logger.Error("LevelRegistry could not be loaded");
        }
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

    public static void LoadBootstrapLevel() => LoadLevel(registry.BootstrapLevel);
    public static void LoadTrainingLevel() => LoadLevel(registry.TrainingLevel);
}