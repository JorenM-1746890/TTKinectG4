using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    internal class HandsUpGesture
    {
        public event EventHandler GestureRecognized;
        public void Update(Skeleton skeleton)
        {
            // Both hands up
            if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.ElbowRight].Position.Y && skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.ElbowLeft].Position.Y)
            {
                GestureRecognized(this, new EventArgs());
            }
        }
    }
}
