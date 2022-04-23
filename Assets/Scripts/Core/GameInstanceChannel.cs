using System;
using UnityEngine;

public interface IGameInstanceChannelPipeline
{
    void AwakeByChannel(GameInstanceChannel Channel);
    void UpdateByChannel(GameInstanceChannel Channel);
}

public class GameInstanceChannel : MonoBehaviour
{
    public IGameInstanceChannelPipeline Pipeline;

    void Awake()
    {
        if (Pipeline != default)
        {
            Pipeline.AwakeByChannel(this);
        }
    }

    void Update()
    {
        if (Pipeline != default)
        {
            Pipeline.UpdateByChannel(this);
        }
    }
}