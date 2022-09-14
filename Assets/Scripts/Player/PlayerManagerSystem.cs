using UnityEngine;
using GameFramework;
using GameFramework.LogManagement;

using ILogger = GameFramework.LogManagement.ILogger;

[GameSystemRegistration(typeof(PlayerManagerSystem))]
public class PlayerManagerSystem : GameSystem
{
    protected override void OnRegistered(GameSystem system)
    {
        PlayerManager.System = this;

        base.OnRegistered(system);

        _logger = LogManager.CreateLogger("PlayerManager");

        _logger.Information("Initializing...");
        _logger.Information("Initialized");
    }

    protected override void OnUnregistered(GameSystem system)
    {
        base.OnUnregistered(system);

        _logger.Information("Shutting down...");
        _logger.Information("Shutdown completed");
    }

    public Player CreateLocalPlayer()
    {
        _logger.Information("Creating LocalPlayer...");

        GameObject playerPrefab = Resources.Load<GameObject>("PlayerPrefab");
        if (playerPrefab is null)
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

    protected ILogger _logger;

    protected Player _localPlayer;
    public Player localPlayer
    {
        get => _localPlayer;
    }
}