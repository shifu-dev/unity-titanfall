using UnityEngine;
using ILogger = Serilog.ILogger;

public static class PlayerManager
{
    private static Player _localPlayer;
    public static Player localPlayer
    {
        get => _localPlayer;
    }

    private static ILogger _logger;

    public static void Init()
    {
        _logger = GameLog.CreateLogger("PlayerManager");

        _logger.Information("Initializing...");
        _logger.Information("Initialized");
    }

    public static void Shutdown()
    {
        _logger.Information("Shutting down...");
        _logger.Information("Shutdown completed");
    }

    public static Player CreateLocalPlayer()
    {
        _logger.Information("Creating LocalPlayer...");

        GameObject playerPrefab = Resources.Load<GameObject>("PlayerPrefab");
        if (playerPrefab == null)
        {
            return null;
        }

        GameObject PlayerGameObject = GameObject.Instantiate(playerPrefab);
        PlayerGameObject.name = "LocalPlayer";
        Player player = PlayerGameObject.GetComponent<Player>();
        GameObject.DontDestroyOnLoad(PlayerGameObject);

        _logger.Information("Created LocalPlayer");

        return player;
    }
}