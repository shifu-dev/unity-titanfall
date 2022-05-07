using UnityEngine;

public static class PlayerManager
{
    private static GameObject playerPrefab;
    public static Player localPlayer { get; set; }
    public static ILogger logger { get; private set; }

    public static void Init()
    {
        logger = GameDebug.CreateLogger("PLAYERMANAGER");

        logger.Info("Initializing");
        playerPrefab = Resources.Load<GameObject>("PlayerPrefab");
    }

    public static Player CreateLocalPlayer()
    {
        logger.Info("Creating LocalPlayer");

        if (playerPrefab == null)
        {
            return null;
        }

        GameObject PlayerGameObject = GameObject.Instantiate(playerPrefab);
        PlayerGameObject.name = "LocalPlayer";
        Player player = PlayerGameObject.GetComponent<Player>();
        GameObject.DontDestroyOnLoad(PlayerGameObject);

        localPlayer = player;
        logger.Info("Created LocalPlayer");

        return localPlayer;
    }
}