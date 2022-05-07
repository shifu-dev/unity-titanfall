using UnityEngine;

public static class GameDebug
{
    public static void Init()
    {
    }

    public static void Shutdown()
    {
    }

    public static ILogger CreateLogger(string name)
    {
        return new Logger(name);
    }
}