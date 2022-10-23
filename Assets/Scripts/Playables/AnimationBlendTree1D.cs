namespace UnityEngine.Playables
{
    public class AnimationBlendTree1D : AnimationBlendTree<float>
    {
        public AnimationBlendTree1D(PlayableGraph graph)
            : base(graph) { }

        public override void UpdateWeights()
        {
            if (mCount == 0) return;

            // find the elements affected by current weight
            int l = -1;
            int r = -1;
            for (int i = 0; i < mCount; i++)
            {
                float posI = mNodes[i].position;

                if (posI == mBlendPosition)
                {
                    l = i;
                    r = i;

                    // found the closest one, 
                    // no need to search more
                    break;
                }

                if (posI < mBlendPosition && (l < 0 || posI > mNodes[l].position))
                {
                    l = i;
                }

                if (posI > mBlendPosition && (r < 0 || posI < mNodes[r].position))
                {
                    r = i;
                }
            }

            l = l < 0 ? r : l;
            r = r < 0 ? l : r;

            // set weights for all elements
            for (int i = 0; i < mCount; i++)
            {
                float weight = 0f;

                if (i >= l && i <= r)
                {
                    float diff = mNodes[r].position - mNodes[l].position;
                    float pos = mNodes[i].position;

                    // diff == 0 indicates elements are at same place
                    if (diff != 0)
                    {
                        weight = diff - Mathf.Abs(mBlendPosition - pos);
                        weight = weight / diff;
                    }
                    else
                    {
                        weight = 1f;
                    }
                }

                mNodes[i].weight = weight;
            }
        }
    }
}