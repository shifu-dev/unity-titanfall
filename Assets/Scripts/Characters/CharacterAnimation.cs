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

        _charMovement = _character.GetBehaviour<CharacterMovement>();
        _animator = GetComponent<Animator>();

        if (initializer != null)
        {
            _source = initializer.source;
        }

        if (_source != null)
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

        CreateLocomotionGraph();

        _graph.Play();
    }

    protected virtual void UpdateGraph()
    {
        UpdateLocomotionGraph();
    }

    protected virtual void DestroyGraph()
    {
        if (_graph.IsValid())
        {
            _graph.Destroy();
        }
    }

    protected virtual void CreateLocomotionGraph()
    {

#pragma warning disable format

        _groundTree     = new AnimationBlendTree1D(_graph);
        _standTree      = new AnimationBlendTree1D(_graph);
        _standWalkTree  = new AnimationBlendTree2DSimpleDirectional(_graph);
        _standRunTree   = new AnimationBlendTree2DSimpleDirectional(_graph);
        _crouchTree     = new AnimationBlendTree1D(_graph);
        _crouchWalkTree = new AnimationBlendTree2DSimpleDirectional(_graph);
        _crouchRunTree  = new AnimationBlendTree2DSimpleDirectional(_graph);

        _standWalkTree.Reserve(9);
        _standWalkTree.AddElement(_source.animGroundStandIdle,                 0.00f ,  0.00f );
        _standWalkTree.AddElement(_source.animGroundStandWalkForward,          0.00f ,  1.00f );
        _standWalkTree.AddElement(_source.animGroundStandWalkForwardLeft,     -0.70f ,  0.70f );
        _standWalkTree.AddElement(_source.animGroundStandWalkForwardRight,    -0.70f ,  0.70f );
        _standWalkTree.AddElement(_source.animGroundStandWalkLeft,            -0.10f ,  0.00f );
        _standWalkTree.AddElement(_source.animGroundStandWalkRight,            0.10f ,  0.00f );
        _standWalkTree.AddElement(_source.animGroundStandWalkBackward,         0.00f , -1.00f );
        _standWalkTree.AddElement(_source.animGroundStandWalkBackwardLeft,    -0.70f , -0.70f );
        _standWalkTree.AddElement(_source.animGroundStandWalkBackwardRight,    0.70f , -0.70f );
        _standWalkTree.FootIk = true;
        _standWalkTree.BuildGraph(true);
        _standWalkTree.UpdateGraph(true);

        _standRunTree.Reserve(9);
        _standRunTree.AddElement(_source.animGroundStandIdle,                  0.00f ,  0.00f);
        _standRunTree.AddElement(_source.animGroundStandRunForward,            0.00f ,  2.00f);
        _standRunTree.AddElement(_source.animGroundStandRunForwardLeft,       -1.41f ,  1.41f);
        _standRunTree.AddElement(_source.animGroundStandRunForwardRight,       1.41f ,  1.41f);
        _standRunTree.AddElement(_source.animGroundStandRunLeft,              -2.00f ,  0.00f);
        _standRunTree.AddElement(_source.animGroundStandRunRight,              2.00f ,  0.00f);
        _standRunTree.AddElement(_source.animGroundStandRunBackward,          -0.00f , -2.00f);
        _standRunTree.AddElement(_source.animGroundStandRunBackwardLeft,      -1.41f , -1.41f);
        _standRunTree.AddElement(_source.animGroundStandRunBackwardRight,      1.41f , -1.41f);
        _standRunTree.FootIk = true;
        _standRunTree.BuildGraph(true);
        _standRunTree.UpdateGraph(true);

        _crouchWalkTree.Reserve(9);
        _crouchWalkTree.AddElement(_source.animGroundCrouchIdle,               0.00f ,  0.00f);
        _crouchWalkTree.AddElement(_source.animGroundCrouchWalkForward,        0.00f ,  1.00f);
        _crouchWalkTree.AddElement(_source.animGroundCrouchWalkForwardLeft,   -0.70f ,  0.70f);
        _crouchWalkTree.AddElement(_source.animGroundCrouchWalkForwardRight,  -0.70f ,  0.70f);
        _crouchWalkTree.AddElement(_source.animGroundCrouchWalkLeft,          -1.00f ,  0.00f);
        _crouchWalkTree.AddElement(_source.animGroundCrouchWalkRight,          1.00f ,  0.00f);
        _crouchWalkTree.AddElement(_source.animGroundCrouchWalkBackward,       0.00f , -1.00f);
        _crouchWalkTree.AddElement(_source.animGroundCrouchWalkBackwardLeft,  -0.70f , -0.70f);
        _crouchWalkTree.AddElement(_source.animGroundCrouchWalkBackwardRight,  0.70f , -0.70f);
        _crouchWalkTree.FootIk = true;
        _crouchWalkTree.BuildGraph(true);
        _crouchWalkTree.UpdateGraph(true);

        _crouchRunTree.Reserve(9);
        _crouchRunTree.AddElement(_source.animGroundCrouchIdle,                0.00f ,  0.00f);
        _crouchRunTree.AddElement(_source.animGroundCrouchRunForward,          0.00f ,  2.00f);
        _crouchRunTree.AddElement(_source.animGroundCrouchRunForwardLeft,     -1.41f ,  1.41f);
        _crouchRunTree.AddElement(_source.animGroundCrouchRunForwardRight,     1.41f ,  1.41f);
        _crouchRunTree.AddElement(_source.animGroundCrouchRunLeft,            -2.00f ,  0.00f);
        _crouchRunTree.AddElement(_source.animGroundCrouchRunRight,            2.00f ,  0.00f);
        _crouchRunTree.AddElement(_source.animGroundCrouchRunBackward,        -0.00f , -2.00f);
        _crouchRunTree.AddElement(_source.animGroundCrouchRunBackwardLeft,    -1.41f , -1.41f);
        _crouchRunTree.AddElement(_source.animGroundCrouchRunBackwardRight,    1.41f , -1.41f);
        _crouchRunTree.FootIk = true;
        _crouchRunTree.BuildGraph(true);
        _crouchRunTree.UpdateGraph(true);

        _standTree.Reserve(4);
        _standTree.AddElement(_source.animGroundStandIdle,           0f);
        _standTree.AddElement(_standWalkTree,                        1f);
        _standTree.AddElement(_standRunTree,                         2f);
        _standTree.AddElement(_source.animGroundStandSprintForward,  3f);
        _standTree.FootIk = true;
        _standTree.BuildGraph(true);
        _standTree.UpdateGraph(true);

        _crouchTree.Reserve(3);
        _crouchTree.AddElement(_source.animGroundCrouchIdle, 0f);
        _crouchTree.AddElement(_crouchWalkTree,              1f);
        _crouchTree.AddElement(_crouchRunTree,               2f);
        _crouchTree.FootIk = true;
        _crouchTree.BuildGraph(true);
        _crouchTree.UpdateGraph(true);

        _groundTree.Reserve(2);
        _groundTree.AddElement(_standTree,  0f);
        _groundTree.AddElement(_crouchTree, 1f);
        _groundTree.FootIk = true;
        _groundTree.BuildGraph(true);
        _groundTree.UpdateGraph(true);

        _animOutput.SetSourcePlayable(_groundTree.Playable);

#pragma warning restore format
    }

    protected virtual void UpdateLocomotionGraph()
    {
        float walkSpeed = _source.groundStandWalkSpeed;
        float runSpeed = _source.groundStandRunSpeed;
        float sprintSpeed = _source.groundStandSprintSpeed;

        Vector3 charVelocity = _charMovement.Velocity;
        Vector2 velocity = new Vector2(charVelocity.x, charVelocity.z);
        float speed = velocity.magnitude;
        speed = MathF.Round(speed, 2);

        if (speed <= walkSpeed)
        {
            speed = speed / walkSpeed;
        }
        else if (speed <= runSpeed)
        {
            speed = 1f + (speed - walkSpeed) / (runSpeed - walkSpeed);
        }
        else
        {
            speed = 2f + (speed - runSpeed) / (sprintSpeed - runSpeed);
        }

        velocity = velocity.normalized * Mathf.Clamp(speed, 0f, 2f);

        _standWalkTree.SetBlendPosition(velocity);
        _standRunTree.SetBlendPosition(velocity);
        _crouchWalkTree.SetBlendPosition(velocity);
        _crouchRunTree.SetBlendPosition(velocity);

        _standTree.SetBlendPosition(speed, true);
        _crouchTree.SetBlendPosition(speed, true);
        _groundTree.SetBlendPosition(0f, true);
    }

    private CharacterAsset _source;
    protected CharacterMovement _charMovement;

    protected PlayableGraph _graph;
    protected Animator _animator;
    protected AnimationPlayableOutput _animOutput;

    protected AnimationBlendTree1D _groundTree;
    protected AnimationBlendTree1D _standTree;
    protected AnimationBlendTree2D _standWalkTree;
    protected AnimationBlendTree2D _standRunTree;
    protected AnimationBlendTree1D _crouchTree;
    protected AnimationBlendTree2D _crouchWalkTree;
    protected AnimationBlendTree2D _crouchRunTree;
}