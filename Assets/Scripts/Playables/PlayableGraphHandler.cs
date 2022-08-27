using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayableGraphHandler
{
    public void ConnectInput(int index)
    {
    }

    protected PlayableGraph _graph;
    public PlayableGraph graph
    {
        get => _graph;
    }

    protected AnimationMixerPlayable _mixer;
}