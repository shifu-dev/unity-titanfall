namespace UnityEngine.Playables
{
    public class AnimationBlendTree1D : AnimationBlendTree<float>
    {
        public AnimationBlendTree1D(PlayableGraph graph)
            : base(graph) { }

        public override void UpdateWeights()
        {
            if (_count == 0) return;

            // find the elements affected by current weight
            int l = -1;
            int r = -1;
            for (int i = 0; i < _count; i++)
            {   
                float posI = _nodes[i].position;

                if (posI == _blendPosition)
                {
                    l = i;
                    r = i;

                    // found the closest one, 
                    // no need to search more
                    break;
                }

                if (posI < _blendPosition && (l < 0 || posI > _nodes[l].position))
                {
                    l = i;
                }

                if (posI > _blendPosition && (r < 0 || posI < _nodes[r].position))
                {
                    r = i;
                }
            }

            l = l < 0 ? r : l;
            r = r < 0 ? l : r;

            // set weights for all elements
            for (int i = 0; i < _count; i++)
            {
                float weight = 0f;

                if (i >= l && i <= r)
                {
                    float diff = _nodes[r].position - _nodes[l].position;
                    float pos = _nodes[i].position;

                    // diff == 0 indicates elements are at same place
                    if (diff != 0)
                    {
                        weight = diff - Mathf.Abs(_blendPosition - pos);
                        weight = weight / diff;
                    }
                    else
                    {
                        weight = 1f;
                    }
                }

                _nodes[i].weight = weight;
            }
        }
    }
}