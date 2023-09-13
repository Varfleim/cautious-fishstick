
using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using SandOcean.Warfare.Ship;

namespace SandOcean.Warfare.TaskForce.Template
{
    public struct TTaskForceReinforcementCheck : IEcsThread<
        SRTaskForceReinforcementCheck, CTaskForce>
    {
        public EcsWorld world;

        int[] taskForceEntities;

        SRTaskForceReinforcementCheck[] taskForceReinforcementCheckSelfRequestPool;
        int[] taskForceReinforcementCheckSelfRequestIndices;

        CTaskForce[] taskForcePool;
        int[] taskForceIndices;

        public void Init(
            int[] entities,
            SRTaskForceReinforcementCheck[] pool1, int[] indices1,
            CTaskForce[] pool2, int[] indices2)
        {
            taskForceEntities = entities;

            taskForceReinforcementCheckSelfRequestPool = pool1;
            taskForceReinforcementCheckSelfRequestIndices = indices1;

            taskForcePool = pool2;
            taskForceIndices = indices2;
        }

        public void Execute(int threadId, int fromIndex, int beforeIndex)
        {
            //��� ������ ����������� ������ � ������������ �������� ����������
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //���� ���������� � ������
                int taskForceEntity = taskForceEntities[a];
                ref SRTaskForceReinforcementCheck selfRequestComp = ref taskForceReinforcementCheckSelfRequestPool[taskForceReinforcementCheckSelfRequestIndices[taskForceEntity]];
                ref CTaskForce taskForce = ref taskForcePool[taskForceIndices[taskForceEntity]];

                //���� ������ ������
                DTFTemplate template = taskForce.template;

                //��� ������� ���� ������� � �������
                for (int b = 0; b < template.shipTypes.Length; b++)
                {
                    //���� ��� �������
                    DShipType shipType = template.shipTypes[b].shipType;

                    //������������, ������� �������� ������� ���� ���� � ������
                    int shipCount = 0;

                    //��� ������� ������� � ������
                    for (int c = 0; c < taskForce.ships.Count; c++)
                    {
                        //���� �������
                        DShip ship = taskForce.ships[c];

                        //���� ������� ��������� � ������� ����
                        if (ship.shipType == shipType)
                        {
                            //����������� ���������� ��������
                            shipCount += 1;
                        }
                    }

                    //���� ���������� �������� ������ ������������
                    if (shipCount < template.shipTypes[b].shipCount)
                    {
                        //������������ ������������� ���������� ��������
                        int requestedShipCount = template.shipTypes[b].shipCount - shipCount;

                        //���������, ���� �� ��� ������ ��� ������� � �������
                        bool isTypeRequested = false;

                        //��� ������� ������������ ���� �������
                        for (int c = 0; c < taskForce.reinforcementRequest.shipTypes.Count; c++)
                        {
                            //���� ��� ������� - �������
                            if (taskForce.reinforcementRequest.shipTypes[c].shipType == shipType)
                            {
                                //��������, ��� ������ ��� ���� � �������
                                isTypeRequested = true;

                                //�������� ���������� ����������� ��������
                                taskForce.reinforcementRequest.shipTypes[c].requestedShipCount = requestedShipCount;

                                //������� �� �����, ��������� ��� ������
                                break;
                            }
                        }

                        //���� ���� ��� � �������
                        if (isTypeRequested == false)
                        {
                            //����������� ������ ���������� ��������
                            taskForce.reinforcementRequest.shipTypes.Add(new(
                                shipType,
                                requestedShipCount));
                        }
                    }
                    //����� ������ �����������
                    else
                    {
                        //��� ������� ������������ ���� �������
                        for (int c = 0; c < taskForce.reinforcementRequest.shipTypes.Count; c++)
                        {
                            //���� ��� ������� - �������
                            if (taskForce.reinforcementRequest.shipTypes[c].shipType == shipType)
                            {
                                //���� ���������� ������������ �������� ����� ����
                                if (taskForce.reinforcementRequest.shipTypes[c].sentedShipCount == 0)
                                {
                                    //������� ��� ������� �� �������
                                    taskForce.reinforcementRequest.shipTypes.RemoveAt(c);

                                    //������� �� �����, ��������� ��� ������
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}