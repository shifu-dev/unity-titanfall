using System;
using UnityEngine;

public interface IGameInstanceChannelPipeline
{
    void AwakeByChannel(GameInstanceChannel Channel);
    void UpdateByChannel(GameInstanceChannel Channel);
    void ApplicationQuitByChannel(GameInstanceChannel Channel);
    void ApplicationFocusByChannel(GameInstanceChannel channel, bool isFocused);
    void ApplicationPauseByChannel(GameInstanceChannel channel, bool isPaused);
}

public class GameInstanceChannel : MonoBehaviour
{
    public IGameInstanceChannelPipeline Pipeline;

    void Awake()
    {
        if (Pipeline != null)
        {
            Pipeline.AwakeByChannel(this);
        }
    }

    void Update()
    {
        if (Pipeline != null)
        {
            Pipeline.UpdateByChannel(this);
        }
    }

    void OnApplicationFocus(bool isFocused)
    {
        if (Pipeline != null)
        {
            Pipeline.ApplicationFocusByChannel(this, isFocused);
        }
    }

    void OnApplicationPause(bool isPaused)
    {
        if (Pipeline != null)
        {
            Pipeline.ApplicationPauseByChannel(this, isPaused);
        }
    }

    void OnApplicationQuit()
    {
        if (Pipeline != null)
        {
            Pipeline.ApplicationQuitByChannel(this);
        }
    }
}