using ILogger = Serilog.ILogger;

public static class LevelManager
{
    public const string LOG_CATEGORY = "LevelManager";

    private static Level _activeLevel;
    public static Level activeLevel
    {
        get => _activeLevel;
    }

    private static ILogger _logger;
    public static ILogger logger
    {
        get => _logger;
    }

    public static void Init()
    {
        _logger = GameLog.CreateLogger(LOG_CATEGORY);
        _logger.Information("Initializing");
    }

    public static void Shutdown()
    {
        _logger.Information("Shutting down");
    }

    public static async void LoadLevel(Level level)
    {
        _logger.Information("Loading " + level.levelName + " Level");
        if (level == _activeLevel)
        {
            _logger.Information(level.levelName + " Level is already loaded");
            return;
        }

        await level.LoadLevelAsync();
        _logger.Information("Loaded " + level.levelName + " Level successfully");

        _activeLevel = level;
    }

    public static void LoadBootstrapLevel()
    {
        Level level = LevelRegistry.Instance.GetAsset("Bootstrap Level");
        if (level == null)
        {
            _logger.Information("Bootstrap level could not be found");
            return;
        }

        LoadLevel(level);
    }
}