using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class AnimationQueue : PlayableBehaviour
{
    private Playable _owner;
    private Playable _mixer;
    private PlayableGraph _graph;
    private int _currentClipIndex = -1;
    private float _timeToNextClip;

    public override void OnPlayableCreate(Playable playable)
    {
        base.OnPlayableCreate(playable);

        _owner = playable;
        _graph = _owner.GetGraph();
        _mixer = AnimationMixerPlayable.Create(_graph, 4);

        _owner.SetInputCount(1);
        _graph.Connect(_mixer, 0, _owner, 0);
        _owner.SetInputWeight(0, 1);
    }

    public void AddAnim(AnimationClip clip)
    {
        var clipPlayable = AnimationClipPlayable.Create(_graph, clip);
        _mixer.AddInput(clipPlayable, 0, 1f);
    }

    public override void PrepareFrame(Playable owner, FrameData info)
    {
        if (_mixer.GetInputCount() == 0)
            return;

        // advance to next clip if necessary
        _timeToNextClip -= (float)info.deltaTime;

        if (_timeToNextClip <= 0.0f)
        {
            _currentClipIndex++;

            if (_currentClipIndex >= _mixer.GetInputCount())
                _currentClipIndex = 0;

            var currentClip = (AnimationClipPlayable)_mixer.GetInput(_currentClipIndex);

            // reset the time so that the next clip starts at the correct position
            currentClip.SetTime(0);

            _timeToNextClip = currentClip.GetAnimationClip().length;
        }

        // adjust the weight of the inputs
        for (int clipIndex = 0; clipIndex < _mixer.GetInputCount(); ++clipIndex)
        {
            if (clipIndex == _currentClipIndex)
                _mixer.SetInputWeight(clipIndex, 1.0f);
            else
                _mixer.SetInputWeight(clipIndex, 0.0f);
        }
    }
}