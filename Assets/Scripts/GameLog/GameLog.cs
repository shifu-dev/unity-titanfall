using System;
using System.IO;
using UnityEngine;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.File.Header;

using ILogger = Serilog.ILogger;

public static class GameLog
{
    public const string HEADER = "------------------------ FILE OPENED --------------------------";
    public const string OUTPUT_TEMPLATE = "[{Frame} {Timestamp:HH:mm:ss} {Level:u3}] {Logger}: {Message:lj}{NewLine}{Exception}";

    private static string _logPath;
    public static string LogPath => _logPath;

    public static void Init()
    {
#if UNITY_EDITOR
        _logPath = Application.dataPath.Replace("Assets", "Logs/");
#else
        _logPath = Application.temporaryCachePath;
#endif

        Debug.Log($"<b>GameLog LogPath:</b> {_logPath}");

        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(
                path: GetAddressFor("GameLog"),
                restrictedToMinimumLevel: LogEventLevel.Information,
                buffered: false,
                shared: false,
                outputTemplate: OUTPUT_TEMPLATE,
                hooks: new HeaderWriter(HEADER))

#if UNITY_EDITOR
            .WriteTo.UnityConsole(outputTemplate: "<b>[{Frame} {Timestamp:HH:mm:ss} {Level:u3}] {Logger}:</b> {Message:lj}{NewLine}{Exception}")
#endif

            .MinimumLevel.Information()
            .CreateAsyncLogger();
    }

    public static void Shutdown()
    {
    }

    public static ILogger CreateLogger(string name, Action<LoggerConfiguration> config = null)
    {
        var builder = new LoggerConfiguration()
            .Enrich.WithLoggerName(name)
            .Enrich.WithFrameCount()
            .WriteTo.Logger(Log.Logger);

        if (config is not null)
        {
            config(builder);
        }

        return builder.CreateLogger();
    }

    public static string GetAddressFor(string logFile)
    {
        string name = Path.ChangeExtension(logFile, ".log");
        return Path.Combine(_logPath, name);
    }
}