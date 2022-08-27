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

    protected Serilog.ILogger _logger;
    protected GameInstanceChannel _updateChannel;

    protected virtual void Init()
    {
        GameLog.Init();
        _logger = GameLog.CreateLogger("GameInstance");

        _logger.Information("Initializing...");

        // Setup GameInstanceUpdateChannel
        GameObject UpdateChannelPrefab = Resources.Load<GameObject>("GameInstanceUpdateChannelPrefab");
        GameObject UpdateChannelGameObject = GameObject.Instantiate(UpdateChannelPrefab);
        UpdateChannelGameObject.name = "GameInstanceUpdateChannel";
        _updateChannel = UpdateChannelGameObject.GetComponent<GameInstanceChannel>();
        _updateChannel.Pipeline = this;
        GameObject.DontDestroyOnLoad(_updateChannel);
        _logger.Information("Created GameInstanceUpdateChannel");

        _logger.Information("Initializing CharacterRegistry");
        CharacterRegistry.Instance.Init();

        _logger.Information("Initializing LevelRegistry");
        LevelRegistry.Instance.Init();

        _logger.Information("Initializing PlayerManager");
        PlayerManager.Init();
        PlayerManager.CreateLocalPlayer();

        _logger.Information("Initializing LevelManager");
        LevelManager.Init();
        LevelManager.LoadBootstrapLevel();

        _logger.Information("Initialization completed");
    }

    protected virtual void Shutdown()
    {
        _logger.Information("Shutting down...");

        _logger.Information("Shutting down LevelManager");
        LevelManager.Shutdown();

        _logger.Information("Shutting down PlayerManager");
        PlayerManager.Shutdown();

        _logger.Information("Shutting down CharacterRegistry");
        CharacterRegistry.Instance.Shutdown();

        _logger.Information("Shutting down LevelRegistry");
        LevelRegistry.Instance.Shutdown();

        _logger.Information("Shutdown completed");
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