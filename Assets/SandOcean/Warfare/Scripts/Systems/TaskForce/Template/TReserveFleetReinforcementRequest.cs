
using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using SandOcean.Warfare.Fleet;
using SandOcean.Warfare.Ship;

namespace SandOcean.Warfare.TaskForce.Template
{
    public struct TReserveFleetReinforcementRequest : IEcsThread<
        SRReserveFleetReinforcementCheck, CFleet, CReserveFleet>
    {
        public EcsWorld world;

        int[] fleetEntities;

        SRReserveFleetReinforcementCheck[] reserveFleetReinforcementCheckSelfRequestPool;
        int[] reserveFleetReinforcementCheckSelfRequestIndices;

        CFleet[] fleetPool;
        int[] fleetIndices;

        CReserveFleet[] reserveFleetPool;
        int[] reserveFleetIndices;

        public void Init(
            int[] entities,
            SRReserveFleetReinforcementCheck[] pool1, int[] indices1,
            CFleet[] pool2, int[] indices2,
            CReserveFleet[] pool3, int[] indices3)
        {
            fleetEntities = entities;

            reserveFleetReinforcementCheckSelfRequestPool = pool1;
            reserveFleetReinforcementCheckSelfRequestIndices = indices1;

            fleetPool = pool2;
            fleetIndices = indices2;

            reserveFleetPool = pool3;
            reserveFleetIndices = indices3;
        }

        public void Execute(int threadId, int fromIndex, int beforeIndex)
        {
            //��� ������� ���������� ����� � ������������ �������� ����������
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //���� ����������, ���� � ��� ��������� ���������� �����
                int reserveFleetEntity = fleetEntities[a];
                ref SRReserveFleetReinforcementCheck selfRequestComp = ref reserveFleetReinforcementCheckSelfRequestPool[reserveFleetReinforcementCheckSelfRequestIndices[reserveFleetEntity]];
                ref CFleet fleet = ref fleetPool[fleetIndices[reserveFleetEntity]];
                ref CReserveFleet reserveFleet = ref reserveFleetPool[reserveFleetIndices[reserveFleetEntity]];

                //���� � ����� ���� ������� � �������
                if (true)
                {
                    //��� ������� ��������� �����
                    for (int b = 0; b < reserveFleet.ownedFleetPEs.Count; b++)
                    {
                        //���� �������� ����
                        reserveFleet.ownedFleetPEs[b].Unpack(world, out int ownedFleetEntity);
                        ref CFleet ownedFleet = ref fleetPool[fleetIndices[ownedFleetEntity]];

                        //��� ������� ������� ����������
                        for (int c = 0; c < ownedFleet.tFReinforcementRequests.Count; c++)
                        {
                            //���� ���� ������� ��������
                            if (ownedFleet.tFReinforcementRequests[c].shipTypes.Count > 0)
                            {
                                //���� ������ ����������
                                DTFReinforcementRequest tFReinforcementRequest = ownedFleet.tFReinforcementRequests[c];

                                //������ ������ ����� ������ ����������
                                DTaskForceReinforcementCreating taskForceReinforcementCreating = new(
                                    ownedFleet.ownedTaskForcePEs[c]);

                                //��� ������� ������������ ���� �������
                                for (int d = 0; d < tFReinforcementRequest.shipTypes.Count; d++)
                                {
                                    //���� ���������� ����������� �������� ������ ���������� ������������
                                    if (tFReinforcementRequest.shipTypes[d].requestedShipCount
                                        > tFReinforcementRequest.shipTypes[d].sentedShipCount)
                                    {
                                        //����������, ������� �������� ����� ���������
                                        int requiredShipCount = tFReinforcementRequest.shipTypes[d].requestedShipCount
                                            - tFReinforcementRequest.shipTypes[d].sentedShipCount;

                                        //����
                                        //������ ��������� ��� ������������ ���� �������
                                        DCountedShipType shipType = new(
                                            tFReinforcementRequest.shipTypes[d].shipType,
                                            requiredShipCount);

                                        //������� � � ������ ���������� ����� ��������
                                        taskForceReinforcementCreating.shipTypes.Add(shipType);

                                        //��������� ���������� ������������ ��� ���������� ��������
                                        tFReinforcementRequest.shipTypes[d].sentedShipCount += requiredShipCount;
                                        //����
                                    }
                                }

                                //���� ������ ������ �� ����
                                if (taskForceReinforcementCreating.shipTypes.Count > 0)
                                {
                                    //������� ������ � ������
                                    reserveFleet.requestedTaskForceReinforcements.Add(taskForceReinforcementCreating);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}