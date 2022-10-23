namespace UnityEngine.Playables
{
    public class AnimationBlendTree2DSimpleDirectional : AnimationBlendTree2D
    {
        public AnimationBlendTree2DSimpleDirectional(PlayableGraph graph)
            : base(graph) { }

        public override void UpdateWeights()
        {
            // initialize all weights to 0
            for (var i = 0; i < mNodes.Length; i++)
            {
                mNodes[i].weight = 0f;
            }

            // handle fallback
            if (mCount < 2)
            {
                if (mCount == 1)
                {
                    mNodes[0].weight = 1;
                }

                return;
            }

            // handle special case when sampled exactly in the middle
            if (mBlendPosition == Vector2.zero)
            {
                // if we have a center motion, give that one all the weight
                for (var i = 0; i < mCount; i++)
                {
                    if (mNodes[i].position == Vector2.zero)
                    {
                        mNodes[i].weight = 1;
                        return;
                    }
                }

                // otherwise divide weight evenly
                float sharedWeight = 1.0f / mCount;
                for (var i = 0; i < mCount; i++)
                {
                    mNodes[i].weight = sharedWeight;
                }

                return;
            }

            int indexA = -1;
            int indexB = -1;
            int indexCenter = -1;
            float maxDotForNegCross = -100000.0f;
            float maxDotForPosCross = -100000.0f;
            for (var i = 0; i < mCount; i++)
            {
                if (mNodes[i].position == Vector2.zero)
                {
                    if (indexCenter >= 0)
                        return;
                    indexCenter = i;
                    continue;
                }

                Vector2 posNormalized = mNodes[i].position.normalized;
                var dot = Vector2.Dot(posNormalized, mBlendPosition);
                var cross = posNormalized.x * mBlendPosition.y - posNormalized.y * mBlendPosition.x;
                if (cross > 0f)
                {
                    if (dot > maxDotForPosCross)
                    {
                        maxDotForPosCross = dot;
                        indexA = i;
                    }
                }
                else
                {
                    if (dot > maxDotForNegCross)
                    {
                        maxDotForNegCross = dot;
                        indexB = i;
                    }
                }
            }

            float centerWeight = 0;

            if (indexA < 0 || indexB < 0)
            {
                // Fallback if sampling point is not inside a triangle
                centerWeight = 1;
            }
            else
            {
                var a = mNodes[indexA].position;
                var b = mNodes[indexB].position;

                // Calculate weights using barycentric coordinates
                // (formulas from http://en.wikipedia.org/wiki/Barycentric_coordinate_system_%28mathematics%29 )
                float det = b.y * a.x - b.x * a.y; // Simplified from: (b.y-0)*(a.x-0) + (0-b.x)*(a.y-0);

                // TODO: Is x and y used correctly below??
                float wA = (b.y * mBlendPosition.x - b.x * mBlendPosition.y) / det; // Simplified from: ((b.y-0)*(l.x-0) + (0-b.x)*(l.y-0)) / det;
                float wB = (a.x * mBlendPosition.y - a.y * mBlendPosition.x) / det; // Simplified from: ((0-a.y)*(l.x-0) + (a.x-0)*(l.y-0)) / det;
                centerWeight = 1 - wA - wB;

                // Clamp to be inside triangle
                if (centerWeight < 0)
                {
                    centerWeight = 0;
                    float sum = wA + wB;
                    wA /= sum;
                    wB /= sum;
                }
                else if (centerWeight > 1)
                {
                    centerWeight = 1;
                    wA = 0;
                    wB = 0;
                }

                // Give weight to the two vertices on the periphery that are closest
                mNodes[indexA].weight = wA;
                mNodes[indexB].weight = wB;
            }

            if (indexCenter >= 0)
            {
                mNodes[indexCenter].weight = centerWeight;
            }
            else
            {
                // Give weight to all children when input is in the center
                float sharedWeight = 1.0f / mCount;
                for (var i = 0; i < mCount; i++)
                    mNodes[i].weight += sharedWeight * centerWeight;
            }
        }
    }
}