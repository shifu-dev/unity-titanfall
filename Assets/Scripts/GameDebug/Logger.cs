using UnityEngine;

public class Logger : ILogger
{
    public string loggerName = "UnknownLogger";

    public Logger(string name)
    {
        loggerName = name;
    }

    public void Info(params object[] msg)
    {
        Debug.Log(loggerName + " [INFO]: " + string.Format("{0}", msg));
    }

    public void Error(params object[] msg)
    {
        Debug.Log(loggerName + " [ERR]: " + string.Format("{0}", msg));
    }
}