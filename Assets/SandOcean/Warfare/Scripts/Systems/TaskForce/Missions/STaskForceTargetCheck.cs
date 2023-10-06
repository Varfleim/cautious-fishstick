
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Map;
using SandOcean.Warfare.TaskForce.Template;

namespace SandOcean.Warfare.TaskForce.Missions
{
    public class STaskForceTargetCheck : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;


        //�����
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //�����
        readonly EcsPoolInject<CTaskForce> taskForcePool = default;
        readonly EcsPoolInject<CTaskForcePatrolMission> tFPatrolMissionPool = default;
        readonly EcsPoolInject<CTaskForceTargetMission> tFTargetMissionPool = default;
        readonly EcsPoolInject<CTaskForceTMissionStrikeGroup> tFTargetMissionStrikeGroupPool = default;
        readonly EcsPoolInject<CTaskForceTMissionReinforcement> tFTargetMissionReinforcementPool = default;

        readonly EcsPoolInject<CTaskForceWaiting> taskForceWaitingPool = default;

        //������� ������
        readonly EcsFilterInject<Inc<CTaskForce, CTaskForcePatrolMission, SRTaskForceTargetCheck>> tFPatrolMissionFilter = default;
        readonly EcsFilterInject<Inc<CTaskForce, CTaskForceTargetMission, SRTaskForceTargetCheck>> tFTargetMissionFilter = default;
        readonly EcsPoolInject<SRTaskForceTargetCheck> tFTargetCheckSelfRequestPool = default;

        readonly EcsPoolInject<SRTaskForceReinforcement> tFReinforcementSelfRequestPool = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������ ����������� ������, ����������� ���������� ������ � ����������� ����
            foreach (int taskForceEntity in tFPatrolMissionFilter.Value)
            {
                //���� ������ � ���������� ������
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);
                ref CTaskForcePatrolMission tFPMission = ref tFPatrolMissionPool.Value.Get(taskForceEntity);

                //���� ������ ��������� � ������ ��������, �� ��� ��������� ������ �������� ������
                if (tFPMission.missionStatus == TaskForceMissionStatus.Movement)
                {
                    //��������� ������ ��������� �������� 
                    TaskForceAssignWaiting(taskForceEntity, ref tFPMission);
                }
                //�����, ���� ������ ��������� � ������ �������
                else
                {

                }

                tFTargetCheckSelfRequestPool.Value.Del(taskForceEntity);
            }

            //��� ������ ����������� ������, ����������� ������� ������ � ����������� ����
            foreach (int taskForceEntity in tFTargetMissionFilter.Value)
            {
                //���� ������ � ������� ������
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);
                ref CTaskForceTargetMission tFTargetMission = ref tFTargetMissionPool.Value.Get(taskForceEntity);

                //���� ������ ��������� � ������ ��������
                if (tFTargetMission.missionStatus == TaskForceMissionStatus.Movement)
                {
                    //���� ���� ������ - ����������� ������
                    if (taskForce.movementTargetType == TaskForceMovementTargetType.TaskForce)
                    {
                        //���� ������� ������
                        taskForce.movementTargetPE.Unpack(world.Value, out int targetTaskForceEntity);
                        ref CTaskForce targetTaskForce = ref taskForcePool.Value.Get(targetTaskForceEntity);

                        //���� ������� ������ ��������� � ������� ������� ������, �� ������� ���������� ���������
                        if (targetTaskForce.currentRegionPE.EqualsTo(taskForce.currentRegionPE) == true)
                        {
                            //���������, � ���� �������� �������, �������������� ������ ������
                            TaskForceTargetTaskForceCheck(
                                taskForceEntity,
                                ref taskForce, ref tFTargetMission,
                                ref targetTaskForce);

                            UnityEngine.Debug.LogWarning("! " + "����������� ��������" + " ! " + targetTaskForce.rand);
                        }
                        //�����, ���� ������� ������ ��������� � ���������� ������� ������� ������,
                        //� ������� ������ ��������� � ���������� ������� ������� ������
                        else if (targetTaskForce.currentRegionPE.EqualsTo(taskForce.previousRegionPE) == true
                            && targetTaskForce.previousRegionPE.EqualsTo(taskForce.currentRegionPE) == true)
                        {
                            //���� � ������� ������ ��� ���������� ������ ������� ������
                            if (tFTargetMissionStrikeGroupPool.Value.Has(targetTaskForceEntity) == false
                                //��� ����� ������� ������ �� �������� ������� ������
                                || targetTaskForce.movementTargetPE.EqualsTo(taskForce.selfPE) == false)
                            {
                                //�� ������� ������ ��������� ������� � ���� ���������� �������
                                //- ����� ������� ���������� � ������������ � �������

                                //���� ������� ������ ������� ������
                                taskForce.currentRegionPE.Unpack(world.Value, out int currentRegionEntity);
                                ref CHexRegion currentRegion = ref regionPool.Value.Get(currentRegionEntity);

                                //������� ������ �� ������ ����� � �������
                                currentRegion.taskForcePEs.Remove(taskForce.selfPE);

                                //���� ���������� ������ ������� ������
                                taskForce.previousRegionPE.Unpack(world.Value, out int previousRegionEntity);
                                ref CHexRegion previousRegion = ref regionPool.Value.Get(previousRegionEntity);

                                //������� ������ � ������ ����� � �������
                                previousRegion.taskForcePEs.Add(taskForce.selfPE);

                                //������ ������� ������ ������� ������
                                taskForce.currentRegionPE = previousRegion.selfPE;

                                //������� ���������� ������ ������� ������
                                taskForce.previousRegionPE = new();


                                //���������, � ���� �������� �������, �������������� ������ ������
                                TaskForceTargetTaskForceCheck(
                                    taskForceEntity,
                                    ref taskForce, ref tFTargetMission,
                                    ref targetTaskForce);


                                UnityEngine.Debug.LogWarning("! " + "��������� � ���� ������� ������� ������, ������ �� �� ���" + " ! " + targetTaskForce.rand);
                            }
                            //����� ������ �������� ���� �� �����, � ������� ���������� � ���������� ������� ��� ������, ������� ��������� ���������
                            else
                            {
                                //����
                                if (0.75f >= 0.5f) //UnityEngine.Random.value
                                {
                                    //���������� ������� � ����������

                                    //���� ������� ������ ������� ������
                                    taskForce.currentRegionPE.Unpack(world.Value, out int currentRegionEntity);
                                    ref CHexRegion currentRegion = ref regionPool.Value.Get(currentRegionEntity);

                                    //������� ������ �� ������ ����� � �������
                                    currentRegion.taskForcePEs.Remove(taskForce.selfPE);

                                    //���� ���������� ������ ������� ������
                                    taskForce.previousRegionPE.Unpack(world.Value, out int previousRegionEntity);
                                    ref CHexRegion previousRegion = ref regionPool.Value.Get(previousRegionEntity);

                                    //������� ������ � ������ ����� � �������
                                    previousRegion.taskForcePEs.Add(taskForce.selfPE);

                                    //������ ������� ������ ������� ������
                                    taskForce.currentRegionPE = previousRegion.selfPE;

                                    //������� ���������� ������ ������� ������
                                    taskForce.previousRegionPE = new();

                                    UnityEngine.Debug.LogWarning("! " + "������� ������ ���������� �������� ������" + " ! " + targetTaskForce.rand);
                                }
                                else
                                {
                                    //���������� ������� � ����������

                                    //���� ������� ������ ������� ������
                                    targetTaskForce.currentRegionPE.Unpack(world.Value, out int currentRegionEntity);
                                    ref CHexRegion currentRegion = ref regionPool.Value.Get(currentRegionEntity);

                                    //������� ������ �� ������ ����� � �������
                                    currentRegion.taskForcePEs.Remove(targetTaskForce.selfPE);

                                    //���� ���������� ������ ������� ������
                                    targetTaskForce.previousRegionPE.Unpack(world.Value, out int previousRegionEntity);
                                    ref CHexRegion previousRegion = ref regionPool.Value.Get(previousRegionEntity);

                                    //������� ������ � ������ ����� � �������
                                    previousRegion.taskForcePEs.Add(targetTaskForce.selfPE);

                                    //������ ������� ������ ������� ������
                                    targetTaskForce.currentRegionPE = previousRegion.selfPE;

                                    //������� ���������� ������ ������� ������
                                    targetTaskForce.previousRegionPE = new();

                                    UnityEngine.Debug.LogWarning("! " + "���� ������� ������ �������� ������" + " ! " + targetTaskForce.rand);
                                }
                                //����

                                //���������, � ���� �������� �������, �������������� ������ ������
                                TaskForceTargetTaskForceCheck(
                                    taskForceEntity,
                                    ref taskForce, ref tFTargetMission,
                                    ref targetTaskForce);
                            }
                        }
                        //����� ������ �����������, ������� �� ���������, ��������� ���������� ��������
                        else
                        {
                            //��������� ������ � ����� ��������
                            tFTargetMission.missionStatus = TaskForceMissionStatus.Movement;


                            UnityEngine.Debug.LogWarning("! " + "�����������" + " ! " + targetTaskForce.rand);
                        }
                    }

                    tFTargetCheckSelfRequestPool.Value.Del(taskForceEntity);
                }
            }
        }

        void TaskForceTargetTaskForceCheck(
            int taskForceEntity,
            ref CTaskForce taskForce, ref CTaskForceTargetMission tFTargetMission,
            ref CTaskForce targetTaskForce)
        {
            //���� ������ ����� ��������� ������ ������� ������
            if (tFTargetMissionStrikeGroupPool.Value.Has(taskForceEntity) == true)
            {
                //��������� ������ � ����� ���

                //����������� ������ ���

                //����
                //��������� ������ � ������ �����, ��� ����� ���������� �
                tFTargetMission.missionStatus = TaskForceMissionStatus.None;
                //����
            }
            //�����, ���� ������ ����� ��������� ������ ����������
            else if (tFTargetMissionReinforcementPool.Value.Has(taskForceEntity) == true)
            {
                //����������� ���������� ������� ������� �������
                TaskForceAssignReinforcementSelfRequest(
                    taskForceEntity,
                    ref targetTaskForce);
            }
        }

        void TaskForceAssignWaiting(
            int taskForceEntity, ref CTaskForcePatrolMission tFPMission)
        {
            //��������� ����������� ������ ��������� ��������
            ref CTaskForceWaiting tFWaiting = ref taskForceWaitingPool.Value.Add(taskForceEntity);

            //��������� �������� ������ ����������
            tFWaiting = new(0);

            //���������, ��� ������ ��������� � ������ ��������
            tFPMission.missionStatus = TaskForceMissionStatus.Waiting;
        }

        void TaskForceAssignBattle()
        {

        }

        void TaskForceAssignReinforcementSelfRequest(
            int taskForceEntity,
            ref CTaskForce targetTaskForce)
        {
            //��������� ����������� ������ ��������� ����������
            ref SRTaskForceReinforcement requestComp = ref tFReinforcementSelfRequestPool.Value.Add(taskForceEntity);

            //��������� ������ �����������
            requestComp = new(targetTaskForce.selfPE);
        }
    }
}