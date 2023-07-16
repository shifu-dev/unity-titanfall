using System.Diagnostics.Contracts;
using UnityEngine;
using GameFramework;
using GameFramework.Logging;

// [RegisterGameSystem(typeof(PlayerManagerSystem))]
public class PlayerManagerSystem : GameSystem
{
    protected override void OnRegistered(GameSystem system)
    {
        PlayerManager.System = this;

        base.OnRegistered(system);

        _logger = GameLog.System.CreateLogger("PlayerManager");

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

        GameObject playerGo = GameObject.Instantiate(playerPrefab);
        GameObject.DontDestroyOnLoad(playerGo);
        playerGo.name = "LocalPlayer";

        Player player = playerGo.GetComponent<Player>();
        Contract.Assert(player is not null);

        _logger.Information("Created LocalPlayer.");

        return player;
    }

    public Player localPlayer
    {
        get => _localPlayer;
    }

    protected Player _localPlayer;
    protected IGameLogger _logger = new SilentLogger();
}