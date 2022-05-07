using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInstance : IGameInstanceChannelPipeline
{
    #region Singeltion Interface

    protected static GameInstance Instance = new GameInstance();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void StaticSubsystemRegistrationRuntimeMethod() { }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void StaticAfterAssembliesLoadedRuntimeMethod() { }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void StaticBeforeSplashScreenRuntimeMethod() { }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void StaticBeforeSceneLoadRuntimeMethod()
    {
        Instance.Init();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void StaticAfterSceneLoadRuntimeMethod() { }

    #endregion

    protected ILogger logger;
    protected GameInstanceChannel UpdateChannel;

    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    protected virtual void Init()
    {
        GameDebug.Init();
        logger = GameDebug.CreateLogger("GAMEINSTANCE");

        logger.Info("Init");

        // Setup GameInstanceUpdateChannel
        GameObject UpdateChannelPrefab = Resources.Load<GameObject>("GameInstanceUpdateChannelPrefab");
        GameObject UpdateChannelGameObject = GameObject.Instantiate(UpdateChannelPrefab);
        UpdateChannelGameObject.name = "GameInstanceUpdateChannel";
        UpdateChannel = UpdateChannelGameObject.GetComponent<GameInstanceChannel>();
        UpdateChannel.Pipeline = this;
        GameObject.DontDestroyOnLoad(UpdateChannel);
        logger.Info("Created GameInstanceUpdateChannel");

        logger.Info("Initializing PlayerManager");
        PlayerManager.Init();
        logger.Info("PlayerManager: CreatingLocalPlayer");
        PlayerManager.CreateLocalPlayer();

        logger.Info("Initializing LevelManager");
        LevelManager.Init();
        logger.Info("LevelManager: LoadBootstrapLevel");
        LevelManager.LoadBootstrapLevel();
    }

    public void AwakeByChannel(GameInstanceChannel Channel)
    {
        // logger.Info("AwakeByChannel");
    }

    public void StartByChannel(GameInstanceChannel Channel)
    {
        // logger.Info("StartByChannel");
    }

    public void UpdateByChannel(GameInstanceChannel Channel)
    {
        // logger.Info("UpdateByChannel");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            logger.Info("LevelManager: LoadTrainingLevel");
            LevelManager.LoadTrainingLevel();
        }
    }
}