using UnityEngine;

namespace Serilog
{
    public struct FrameProperty
    {
        public override string ToString()
        {
            return Time.frameCount.ToString();
        }
    }
}