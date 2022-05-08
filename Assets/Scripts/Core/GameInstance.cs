using GameLog;
using UnityEngine;

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

    protected GameLog.ILogger logger;
    protected GameInstanceChannel UpdateChannel;

    protected virtual void Init()
    {
        GameDebug.Init();
        logger = GameDebug.GetOrCreateLogger("GAMEINSTANCE");

        logger.Info("Initializing");

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
        LevelManager.LoadTrainingLevel();
    }

    protected virtual void Shutdown()
    {
        logger.Info("Shutting down LevelManager");
        LevelManager.Shutdown();

        logger.Info("Shutting down PlayerManager");
        PlayerManager.Shutdown();

        logger.Info("Shutting down GameDebug");
        GameDebug.Shutdown();
    }

    public void AwakeByChannel(GameInstanceChannel Channel)
    {
    }

    public void StartByChannel(GameInstanceChannel Channel)
    {
    }

    public void UpdateByChannel(GameInstanceChannel Channel)
    {
    }

    public void ApplicationFocusByChannel(GameInstanceChannel channel, bool isFocused)
    {
    }

    public void ApplicationPauseByChannel(GameInstanceChannel channel, bool isPaused)
    {
    }

    public void ApplicationQuitByChannel(GameInstanceChannel Channel)
    {
        Shutdown();
    }
}