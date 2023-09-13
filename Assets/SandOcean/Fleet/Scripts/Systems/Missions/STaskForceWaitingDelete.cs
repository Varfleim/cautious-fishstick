
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Warfare.TaskForce.Missions;

namespace SandOcean.Warfare.Fleet
{
    public class STaskForceWaitingDelete : IEcsRunSystem
    {
        //�����
        readonly EcsFilterInject<Inc<CTaskForcePatrolMission, CTaskForceWaiting>> taskForcePatrolMissionFilter = default;
        readonly EcsPoolInject<CTaskForcePatrolMission> taskForcePatrolMissionPool = default;
        readonly EcsPoolInject<CTaskForceWaiting> taskForceWaitingPool = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������ ����������� ������ � ���������� ������� � ����������� ��������
            foreach (int taskForceEntity in taskForcePatrolMissionFilter.Value)
            {
                //���� ���������� ������
                ref CTaskForcePatrolMission tFPatrolMission = ref taskForcePatrolMissionPool.Value.Get(taskForceEntity);

                //���� ������ �� ��������� � ������ ��������
                if (tFPatrolMission.missionStatus != TaskForceMissionStatus.Waiting)
                {
                    //������� ��������� ��������
                    taskForceWaitingPool.Value.Del(taskForceEntity);
                }
            }
        }
    }
}