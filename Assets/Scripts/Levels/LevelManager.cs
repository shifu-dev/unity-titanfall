using System;
using UnityEngine;

public static class LevelManager
{
    public static Level ActiveLevel => m_ActiveLevel;
    private static Level m_ActiveLevel;

    public static void Init()
    {
    }

    public static void Shutdown()
    {
    }

    public static void LoadLevel(Level level)
    {
        if (level == m_ActiveLevel)
        {
            return;
        }
    }
}