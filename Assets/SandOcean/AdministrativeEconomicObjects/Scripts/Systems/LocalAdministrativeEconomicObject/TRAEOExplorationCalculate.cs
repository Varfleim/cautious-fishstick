
using System.Collections.Generic;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using SandOcean.Diplomacy;
using SandOcean.Ship;
using SandOcean.Ship.Moving;

namespace SandOcean.AEO.RAEO
{
    public struct TRAEOExplorationCalculate : IEcsThread<
        CRegionAEO,
        CExplorationORAEO,
        CShipGroup,
        COrganization>
    {
        public EcsWorld world;

        int[] regionAEOEntities;

        CRegionAEO[] regionAEOPool;
        int[] regionAEOIndices;

        CExplorationORAEO[] explorationOLAEOPool;
        int[] explorationOLAEOIndices;

        CShipGroup[] shipGroupPool;
        int[] shipGroupIndices;

        COrganization[] organizationPool;
        int[] organizationIndices;

        public void Init(
            int[] entities,
            CRegionAEO[] pool1, int[] indices1,
            CExplorationORAEO[] pool2, int[] indices2,
            CShipGroup[] pool3, int[] indices3,
            COrganization[] pool4, int[] indices4)
        {
            regionAEOEntities = entities;

            regionAEOPool = pool1;
            regionAEOIndices = indices1;

            explorationOLAEOPool = pool2;
            explorationOLAEOIndices = indices2;

            shipGroupPool = pool3;
            shipGroupIndices = indices3;

            organizationPool = pool4;
            organizationIndices = indices4;
        }

        public void Execute(int fromIndex, int beforeIndex)
        {
            //��� ������� RAEO � ������
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //���� ��������� RAEO
                int lAEOEntity = regionAEOEntities[a];
                ref CRegionAEO rAEO = ref regionAEOPool[regionAEOIndices[lAEOEntity]];

                //���� ������ ������ �������� � ������
                LinkedListNode<EcsPackedEntity> currentShipGroupNode = rAEO.landedShipGroups.First;
                //��� ������ ������ �������� � OLAEO
                while (currentShipGroupNode != null)
                {
                    //���� ��������� ������ ��������
                    currentShipGroupNode.Value.Unpack(world, out int shipGroupEntity);
                    ref CShipGroup shipGroup = ref shipGroupPool[shipGroupIndices[shipGroupEntity]];

                    //����
                    //���� ������ �������� ��������� � ������ �����������
                    if (shipGroup.movingMode == ShipGroupMovingMode.Idle)
                    {
                        //���� ��������� �����������-��������� ������ ��������
                        shipGroup.ownerOrganizationPE.Unpack(world, out int ownerOrganizationEntity);
                        ref COrganization ownerOrganization = ref organizationPool[organizationIndices[ownerOrganizationEntity]];

                        //���� ��������� EOLAEO �����������
                        rAEO.organizationRAEOs[ownerOrganization.selfIndex].organizationRAEOPE.Unpack(world, out int eOLAEOEntity);
                        ref CExplorationORAEO exOLAEO = ref explorationOLAEOPool[explorationOLAEOIndices[eOLAEOEntity]];

                        //���� ������� ������������ ������ ������
                        if (exOLAEO.explorationLevel < 10)
                        {
                            //����������� ������� ������������
                            exOLAEO.explorationLevel++;
                        }
                    }
                    //����

                    //���� ��������� ������ �������� � �������� �������
                    currentShipGroupNode = currentShipGroupNode.Next;
                }
            }
        }
    }
}