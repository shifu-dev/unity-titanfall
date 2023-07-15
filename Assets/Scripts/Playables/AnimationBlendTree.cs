using System;
using UnityEngine.Animations;

namespace UnityEngine.Playables
{
    public interface IBlendTreeElement
    {
        public float GetLength();
        public Playable GetPlayable();
        public Playable CreatePlayable();
    }

    public abstract class AnimationBlendTree<TPosition> : IBlendTreeElement
        where TPosition : struct, IEquatable<TPosition>
    {
        //// ---------------------------------------------------------------------------------------
        //// IBlendTreeElement : BEGIN
        //// ---------------------------------------------------------------------------------------

        public float GetLength()
        {
            return _blendedClipLength;
        }

        public Playable GetPlayable()
        {
            return _mixer;
        }

        public Playable CreatePlayable()
        {
            return _mixer;
        }

        //// ---------------------------------------------------------------------------------------
        //// IBlendTreeElement : END
        //// ---------------------------------------------------------------------------------------

        protected struct NullElement : IBlendTreeElement
        {
            public float GetLength() { return 0f; }

            public Playable GetPlayable() { return new Playable(); }

            public Playable CreatePlayable() { return new Playable(); }
        }

        protected struct AnimationClipElement : IBlendTreeElement
        {
            public float GetLength()
            {
                if (clip is null)
                {
                    return 0f;
                }

                return clip.length;
            }

            public Playable GetPlayable()
            {
                return playable;
            }

            public Playable CreatePlayable()
            {
                if (playable.IsNull() == false)
                {
                    playable.Destroy();
                }

                playable = AnimationClipPlayable.Create(graph, clip);
                return playable;
            }

            public AnimationClipPlayable playable;
            public PlayableGraph graph;
            public AnimationClip clip;
        }

        protected struct Node
        {
            public IBlendTreeElement element;
            public TPosition position;
            public float speed;
            public float weight;
        }

        public AnimationBlendTree(PlayableGraph graph, int capacity = 0)
        {
            _graph = graph;
            _mixer = AnimationMixerPlayable.Create(_graph, capacity);
            _blendPosition = default;
            _nodes = new Node[capacity];
            _blendedClipLength = 0f;
            _masterSpeed = 1f;
            _footIk = false;
            _count = 0;
        }

        ~AnimationBlendTree()
        {
            if (_mixer.IsValid())
            {
                _mixer.Destroy();
            }
        }

        public bool AddElement(IBlendTreeElement element, TPosition position, float speed = 1f)
        {
            if (element is null)
            {
                return false;
            }

            Node node = new Node();
            node.element = element;
            node.position = position;
            node.speed = speed;

            _AddNode(node);
            return true;
        }

        public bool AddElement(AnimationClip clip, TPosition position, float speed = 1f)
        {
            AnimationClipElement element = new AnimationClipElement();
            element.clip = clip;
            element.graph = _graph;

            return AddElement(element, position, speed);
        }

        public bool SetElement(int index, IBlendTreeElement element)
        {
            if (index < 0 || index > _count - 1)
            {
                return false;
            }

            if (element is null)
            {
                return false;
            }

            _nodes[index].element = element;

            return true;
        }

        public bool SetElement(int index, AnimationClip clip)
        {
            AnimationClipElement element = new AnimationClipElement();
            element.clip = clip;
            element.graph = _graph;

            return SetElement(index, element);
        }

        public bool SetPosition(int index, TPosition position)
        {
            if (index < 0 || index > _count - 1)
            {
                return false;
            }

            _nodes[index].position = position;
            return true;
        }

        public bool SetSpeed(int index, float speed)
        {
            if (index < 0 || index > _count - 1)
            {
                return false;
            }

            _nodes[index].speed = speed;
            return true;
        }

        public void SetBlendPosition(TPosition position, bool? updateGraph = null)
        {
            if (updateGraph.HasValue == false)
            {
                if (_blendPosition.Equals(position))
                {
                    updateGraph = false;
                }
                else
                {
                    updateGraph = true;
                }
            }

            _blendPosition = position;

            if (updateGraph.Value)
            {
                UpdateGraph(true);
            }
        }

        public void Reserve(int capacity)
        {
            capacity = Mathf.Max(capacity, _count);
            Array.Resize(ref _nodes, capacity);
        }

        public bool IsFootIkEnabled()
        {
            return _footIk;
        }

        public void EnableFootIk()
        {
            _footIk = true;
        }

        public void DisableFootIk()
        {
            _footIk = false;
        }

        public abstract void UpdateWeights();

        public void UpdateTree(bool updateWeights = false)
        {
            if (updateWeights)
            {
                UpdateWeights();
            }

            // update _blendedClipLength
            _blendedClipLength = 0f;
            for (var i = 0; i < _count; i++)
            {
                var node = _nodes[i];

                _blendedClipLength += (node.element.GetLength() / node.speed) * node.weight;
            }

            _blendedClipLength /= _masterSpeed;
        }

        public void UpdateGraph(bool updateTree = false)
        {
            if (updateTree)
            {
                UpdateTree(updateTree);
            }

            int count = Mathf.Min(_count, _mixer.GetInputCount());
            for (int i = 0; i < count; i++)
            {
                _mixer.SetInputWeight(i, _nodes[i].weight);

                try
                {
                    AnimationClipPlayable clipPlayable = (AnimationClipPlayable)_mixer.GetInput(i);
                    float speed = 0f;

                    if (_blendedClipLength != 0f)
                    {
                        speed = _nodes[i].element.GetLength() / _blendedClipLength;
                    }

                    clipPlayable.SetSpeed(speed);
                    clipPlayable.SetApplyFootIK(_footIk);
                }
                catch (Exception) { }
            }
        }

        public void BuildGraph(bool rebuildAll = false)
        {
            int inputCount = 0;
            if (rebuildAll == false)
            {
                inputCount = _mixer.GetInputCount();
            }

            _mixer.SetInputCount(_count);
            for (int i = inputCount; i < _count; i++)
            {
                Playable playable = _nodes[i].element.GetPlayable();
                if (playable.IsNull())
                {
                    playable = _nodes[i].element.CreatePlayable();
                }

                _mixer.ConnectInput(i, playable, 0, _nodes[i].weight);
            }
        }

        protected virtual void _AddNode(Node node)
        {
            // prepare the node
            node.weight = 0f;

            // increase capacity if not enough
            if (_count == _nodes.Length)
            {
                Reserve(_nodes.Length + 1);
            }

            _nodes[_count] = node;
            _count++;
        }

        public PlayableGraph graph => _graph;
        public AnimationMixerPlayable playable => _mixer;
        public TPosition blendPosition => _blendPosition;
        public float blendedClipLength => _blendedClipLength;
        public float masterSpeed
        {
            get => _masterSpeed;
            set => _masterSpeed = value;
        }
        public bool footIk
        {
            get => _footIk;
            set => _footIk = value;
        }
        public int count => _count;
        public int capacity => _nodes.Length;

        protected PlayableGraph _graph;
        protected AnimationMixerPlayable _mixer;
        protected TPosition _blendPosition;
        protected Node[] _nodes;
        protected float _blendedClipLength;
        protected float _masterSpeed;
        protected bool _footIk;
        protected int _count;
    }
}