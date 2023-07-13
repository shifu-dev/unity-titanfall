namespace UnityEngine.Playables
{
    public class AnimationBlendTree2DSimpleDirectional : AnimationBlendTree2D
    {
        public AnimationBlendTree2DSimpleDirectional(PlayableGraph graph)
            : base(graph) { }

        public override void UpdateWeights()
        {
            // initialize all weights to 0
            for (var i = 0; i < _nodes.Length; i++)
            {
                _nodes[i].weight = 0f;
            }

            // handle fallback
            if (_count < 2)
            {
                if (_count == 1)
                {
                    _nodes[0].weight = 1;
                }

                return;
            }

            // handle special case when sampled exactly in the middle
            if (_blendPosition == Vector2.zero)
            {
                // if we have a center motion, give that one all the weight
                for (var i = 0; i < _count; i++)
                {
                    if (_nodes[i].position == Vector2.zero)
                    {
                        _nodes[i].weight = 1;
                        return;
                    }
                }

                // otherwise divide weight evenly
                float sharedWeight = 1.0f / _count;
                for (var i = 0; i < _count; i++)
                {
                    _nodes[i].weight = sharedWeight;
                }

                return;
            }

            int indexA = -1;
            int indexB = -1;
            int indexCenter = -1;
            float maxDotForNegCross = -100000.0f;
            float maxDotForPosCross = -100000.0f;
            for (var i = 0; i < _count; i++)
            {
                if (_nodes[i].position == Vector2.zero)
                {
                    if (indexCenter >= 0)
                        return;
                    indexCenter = i;
                    continue;
                }

                Vector2 posNormalized = _nodes[i].position.normalized;
                var dot = Vector2.Dot(posNormalized, _blendPosition);
                var cross = posNormalized.x * _blendPosition.y - posNormalized.y * _blendPosition.x;
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
                var a = _nodes[indexA].position;
                var b = _nodes[indexB].position;

                // Calculate weights using barycentric coordinates
                // (formulas from http://en.wikipedia.org/wiki/Barycentric_coordinate_system_%28mathematics%29 )
                float det = b.y * a.x - b.x * a.y; // Simplified from: (b.y-0)*(a.x-0) + (0-b.x)*(a.y-0);

                // TODO: Is x and y used correctly below??
                float wA = (b.y * _blendPosition.x - b.x * _blendPosition.y) / det; // Simplified from: ((b.y-0)*(l.x-0) + (0-b.x)*(l.y-0)) / det;
                float wB = (a.x * _blendPosition.y - a.y * _blendPosition.x) / det; // Simplified from: ((0-a.y)*(l.x-0) + (a.x-0)*(l.y-0)) / det;
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
                _nodes[indexA].weight = wA;
                _nodes[indexB].weight = wB;
            }

            if (indexCenter >= 0)
            {
                _nodes[indexCenter].weight = centerWeight;
            }
            else
            {
                // Give weight to all children when input is in the center
                float sharedWeight = 1.0f / _count;
                for (var i = 0; i < _count; i++)
                    _nodes[i].weight += sharedWeight * centerWeight;
            }
        }
    }
}