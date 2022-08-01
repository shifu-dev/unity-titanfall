namespace UnityEngine.Playables
{
    public abstract class AnimationBlendTree2D : AnimationBlendTree<Vector2>
    {
        public AnimationBlendTree2D(PlayableGraph graph)
            : base(graph) { }

        public void AddElement(AnimationClip clip, float x, float y, float speed = 1f)
        {
            AddElement(clip, new Vector2(x, y), speed);
        }

        public bool SetPosition(int index, float x, float y)
        {
            return SetPosition(index, new Vector2(x, y));
        }

        public void SetBlendPosition(float x, float y, bool? updateGraph = null)
        {
            SetBlendPosition(new Vector2(x, y), updateGraph);
        }
    }
}