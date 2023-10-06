
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace SandOcean.Warfare.Fleet.Moving
{
    public class STaskForcePathfindingRequestDelete : IEcsRunSystem
    {
        //������� ������
        readonly EcsFilterInject<Inc<SRTaskForceFindPath>> taskForceFindPathSelfRequestFilter = default;
        readonly EcsPoolInject<SRTaskForceFindPath> taskForceFindPathSelfRequestPool = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ����������� ������ ����
            foreach (int selfRequestEntity in taskForceFindPathSelfRequestFilter.Value)
            {
                //������� ���������� � ��������, �������� ������ ���������� ����������� ������
                taskForceFindPathSelfRequestPool.Value.Del(selfRequestEntity);
            }
        }
    }
}