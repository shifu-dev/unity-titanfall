using GameLog;
using UnityEngine;

public class GameInstance : IGameInstanceChannelPipeline
{
    //////////////////////////////////////////////////////////////////
    /// Singleton Interface | BEGIN

    protected static GameInstance instance = new GameInstance();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void StaticSubsystemRegistrationRuntimeMethod() { }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void StaticAfterAssembliesLoadedRuntimeMethod() { }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void StaticBeforeSplashScreenRuntimeMethod() { }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void StaticBeforeSceneLoadRuntimeMethod()
    {
        instance.Init();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void StaticAfterSceneLoadRuntimeMethod() { }

    /// Singleton Interface | END
    //////////////////////////////////////////////////////////////////

    protected GameLog.ILogger _logger;
    protected GameInstanceChannel _updateChannel;

    protected virtual void Init()
    {
        GameDebug.Init();
        _logger = GameDebug.GetOrCreateLogger("GAMEINSTANCE");

        _logger.Info("Initializing");

        // Setup GameInstanceUpdateChannel
        GameObject UpdateChannelPrefab = Resources.Load<GameObject>("GameInstanceUpdateChannelPrefab");
        GameObject UpdateChannelGameObject = GameObject.Instantiate(UpdateChannelPrefab);
        UpdateChannelGameObject.name = "GameInstanceUpdateChannel";
        _updateChannel = UpdateChannelGameObject.GetComponent<GameInstanceChannel>();
        _updateChannel.Pipeline = this;
        GameObject.DontDestroyOnLoad(_updateChannel);
        _logger.Info("Created GameInstanceUpdateChannel");

        _logger.Info("Initializing CharacterRegistry");
        CharacterRegistry.Init();

        _logger.Info("Initializing PlayerManager");
        PlayerManager.Init();
        _logger.Info("PlayerManager: CreatingLocalPlayer");
        PlayerManager.CreateLocalPlayer();

        _logger.Info("Initializing LevelManager");
        LevelManager.Init();
        _logger.Info("LevelManager: LoadBootstrapLevel");
        LevelManager.LoadTrainingLevel();
    }

    protected virtual void Shutdown()
    {
        _logger.Info("Shutting down CharacterRegistry");
        CharacterRegistry.Shutdown();

        _logger.Info("Shutting down LevelManager");
        LevelManager.Shutdown();

        _logger.Info("Shutting down PlayerManager");
        PlayerManager.Shutdown();

        _logger.Info("Shutting down GameDebug");
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