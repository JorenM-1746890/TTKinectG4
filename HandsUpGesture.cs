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
        public bool Update(Skeleton skeleton)
        {
            // Both hands up
            if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.ElbowRight].Position.Y && skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.ElbowLeft].Position.Y)
            {
                GestureRecognized(this, new EventArgs());
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    internal class ClapGesture
    {
        public event EventHandler ClapRecognized;
        double threshold = 0.4;
        public bool Update(Skeleton skeleton)
        {
            // Both hands up
            if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.Head].Position.Y && skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.Head].Position.Y)
            {
                if (Math.Abs(skeleton.Joints[JointType.HandRight].Position.X - skeleton.Joints[JointType.HandLeft].Position.X) < threshold &&
                    Math.Abs(skeleton.Joints[JointType.HandRight].Position.Y - skeleton.Joints[JointType.HandLeft].Position.Y) < threshold &&
                    Math.Abs(skeleton.Joints[JointType.HandRight].Position.Z - skeleton.Joints[JointType.HandLeft].Position.Z) < threshold)
                {
                    ClapRecognized(this, new EventArgs());
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
