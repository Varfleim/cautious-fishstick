
using System;

namespace SandOcean.Warfare.TaskForce.Missions
{
    public class DFleetTargetPriority : IComparable
    {
        public DFleetTargetPriority(
            int targetEntity, float targetPriority)
        {
            this.targetEntity = targetEntity;
            this.targetPriority = targetPriority;
        }

        public readonly int targetEntity;
        public readonly float targetPriority;

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            DFleetTargetPriority otherTarget = obj as DFleetTargetPriority;

            if (otherTarget != null)
            {
                return targetPriority.CompareTo(otherTarget.targetPriority);
            }
            else
            {
                throw new ArgumentException("Object is not a DFleetRegionPriority");
            }
        }
    }
}