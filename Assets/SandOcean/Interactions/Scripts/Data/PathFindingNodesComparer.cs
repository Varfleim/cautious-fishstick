
using System.Collections.Generic;

namespace SandOcean.Map.Pathfinding
{
    public class PathFindingNodesComparer : IComparer<int>
    {
        DPathFindingNodeFast[] m;

        public PathFindingNodesComparer(
            DPathFindingNodeFast[] nodes)
        {
            m = nodes;
        }

        public int Compare(
            int a, int b)
        {
            if (m[a].priority > m[b].priority)
            {
                return 1;
            }
            else if (m[a].priority < m[b].priority)
            {
                return -1;
            }
            return 0;
        }

        public void SetMatrix(
            DPathFindingNodeFast[] nodes)
        {
            m = nodes;
        }
    }
}