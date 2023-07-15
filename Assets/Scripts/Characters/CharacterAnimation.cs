using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
public class CharacterAnimation : CharacterBehaviour
{
    public override void OnCharacterCreate(Character character, CharacterInitializer initializer)
    {
        base.OnCharacterCreate(character, initializer);

        _charMovement = _character.charMovement;
        _animator = GetComponent<Animator>();

        if (initializer is not null)
        {
            _source = initializer.source;
        }

        if (_source is not null)
        {
            CreateGraph();
        }
    }

    public override void OnCharacterUpdate()
    {
        base.OnCharacterUpdate();

        UpdateGraph();
    }

    public override void OnCharacterDestroy()
    {
        base.OnCharacterDestroy();

        DestroyGraph();
    }

    protected virtual void CreateGraph()
    {
        _graph = PlayableGraph.Create();
        _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        _animOutput = AnimationPlayableOutput.Create(_graph, "Animation", _animator);

        // CreateLocomotionGraph();

        _graph.Play();
    }

    protected virtual void UpdateGraph()
    {
        // UpdateLocomotionGraph();
    }

    protected virtual void DestroyGraph()
    {
        if (_graph.IsValid())
        {
            _graph.Destroy();
        }
    }

    private CharacterAsset _source;
    protected CharacterMovement _charMovement;

    protected PlayableGraph _graph;
    protected Animator _animator;
    protected AnimationPlayableOutput _animOutput;
}