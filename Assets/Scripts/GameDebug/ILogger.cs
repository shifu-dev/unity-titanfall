using UnityEngine;

public interface ILogger
{
    void Info(params object[] msg);
    void Error(params object[] msg);
}