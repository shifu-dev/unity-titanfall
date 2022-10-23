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
        //////////////////////////////////////////////////////////////////
        /// IBlendTreeElement : BEGIN

        public float GetLength()
        {
            return mBlendedClipLength;
        }

        public Playable GetPlayable()
        {
            return mMixer;
        }

        public Playable CreatePlayable()
        {
            return mMixer;
        }

        /// IBlendTreeElement : END
        //////////////////////////////////////////////////////////////////

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

        ~AnimationBlendTree()
        {
            if (mMixer.IsValid())
            {
                mMixer.Destroy();
            }
        }

        public AnimationBlendTree(PlayableGraph graph, int capacity = 0)
        {
            mGraph = graph;
            mMixer = AnimationMixerPlayable.Create(mGraph, capacity);
            mBlendPosition = default;
            mNodes = new Node[capacity];
            mBlendedClipLength = 0f;
            mMasterSpeed = 1f;
            mFootIk = false;
            mCount = 0;
        }

        protected virtual void AddNode(Node node)
        {
            // prepare the node
            node.weight = 0f;

            // increase capacity if not enough
            if (mCount == mNodes.Length)
            {
                Reserve(mNodes.Length + 1);
            }

            mNodes[mCount] = node;
            mCount++;
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

            AddNode(node);
            return true;
        }

        public bool AddElement(AnimationClip clip, TPosition position, float speed = 1f)
        {
            AnimationClipElement element = new AnimationClipElement();
            element.clip = clip;
            element.graph = mGraph;

            return AddElement(element, position, speed);
        }

        public bool SetElement(int index, IBlendTreeElement element)
        {
            if (index < 0 || index > mCount - 1)
            {
                return false;
            }

            if (element is null)
            {
                return false;
            }

            mNodes[index].element = element;

            return true;
        }

        public bool SetElement(int index, AnimationClip clip)
        {
            AnimationClipElement element = new AnimationClipElement();
            element.clip = clip;
            element.graph = mGraph;

            return SetElement(index, element);
        }

        public bool SetPosition(int index, TPosition position)
        {
            if (index < 0 || index > mCount - 1)
            {
                return false;
            }

            mNodes[index].position = position;
            return true;
        }

        public bool SetSpeed(int index, float speed)
        {
            if (index < 0 || index > mCount - 1)
            {
                return false;
            }

            mNodes[index].speed = speed;
            return true;
        }

        public void SetBlendPosition(TPosition position, bool? updateGraph = null)
        {
            if (updateGraph.HasValue == false)
            {
                if (mBlendPosition.Equals(position))
                {
                    updateGraph = false;
                }
                else
                {
                    updateGraph = true;
                }
            }

            mBlendPosition = position;

            if (updateGraph.Value)
            {
                UpdateGraph(true);
            }
        }

        public void Reserve(int capacity)
        {
            capacity = Mathf.Max(capacity, mCount);
            Array.Resize(ref mNodes, capacity);
        }

        public abstract void UpdateWeights();

        public void UpdateTree(bool updateWeights = false)
        {
            if (updateWeights)
            {
                UpdateWeights();
            }

            // update _blendedClipLength
            mBlendedClipLength = 0f;
            for (var i = 0; i < mCount; i++)
            {
                var node = mNodes[i];

                mBlendedClipLength += (node.element.GetLength() / node.speed) * node.weight;
            }

            mBlendedClipLength /= mMasterSpeed;
        }

        public void UpdateGraph(bool updateTree = false)
        {
            if (updateTree)
            {
                UpdateTree(updateTree);
            }

            int count = Mathf.Min(mCount, mMixer.GetInputCount());
            for (int i = 0; i < count; i++)
            {
                mMixer.SetInputWeight(i, mNodes[i].weight);

                try
                {
                    AnimationClipPlayable clipPlayable = (AnimationClipPlayable)mMixer.GetInput(i);
                    float speed = 0f;

                    if (mBlendedClipLength != 0f)
                    {
                        speed = mNodes[i].element.GetLength() / mBlendedClipLength;
                    }

                    clipPlayable.SetSpeed(speed);
                    clipPlayable.SetApplyFootIK(mFootIk);
                }
                catch (Exception) { }
            }
        }

        public void BuildGraph(bool rebuildAll = false)
        {
            int inputCount = 0;
            if (rebuildAll == false)
            {
                inputCount = mMixer.GetInputCount();
            }

            mMixer.SetInputCount(mCount);
            for (int i = inputCount; i < mCount; i++)
            {
                Playable playable = mNodes[i].element.GetPlayable();
                if (playable.IsNull())
                {
                    playable = mNodes[i].element.CreatePlayable();
                }

                mMixer.ConnectInput(i, playable, 0, mNodes[i].weight);
            }
        }

        public PlayableGraph Graph => mGraph;
        public AnimationMixerPlayable Playable => mMixer;
        public TPosition BlendPosition => mBlendPosition;
        public float BlendedClipLength => mBlendedClipLength;
        public float MasterSpeed
        {
            get => mMasterSpeed;
            set => mMasterSpeed = value;
        }
        public bool FootIk
        {
            get => mFootIk;
            set => mFootIk = value;
        }
        public int Count => mCount;
        public int Capacity => mNodes.Length;

        protected PlayableGraph mGraph;
        protected AnimationMixerPlayable mMixer;
        protected TPosition mBlendPosition;
        protected Node[] mNodes;
        protected float mBlendedClipLength;
        protected float mMasterSpeed;
        protected bool mFootIk;
        protected int mCount;
    }
}