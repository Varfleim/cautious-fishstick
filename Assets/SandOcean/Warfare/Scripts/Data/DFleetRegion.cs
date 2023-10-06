
using System;
using System.Collections.Generic;

using Leopotam.EcsLite;

using SandOcean.Map;

namespace SandOcean.Warfare.Fleet
{
    public class DFleetRegion : IEquatable<DFleetRegion>
    {
        public DFleetRegion(
            EcsPackedEntity regionPE)
        {
            this.regionPE = regionPE;

            neighbourRegions = new List<DFleetRegion>(6);

            searchMissionLastTime = 0;
        }

        public readonly EcsPackedEntity regionPE;

        public List<DFleetRegion> neighbourRegions;

        public void AddNeighbours(
            ref CHexRegion currentRegion,
            List<DFleetRegion> fleetRegions)
        {
            //��� ������� ������ �������� �������
            for (int a = 0; a < currentRegion.neighbourRegionPEs.Length; a++)
            {
                //��� ������� ������� �����
                for (int b = 0; b < fleetRegions.Count; b++)
                {
                    //���� ��� ����� �������� �������
                    if (fleetRegions[b].regionPE.EqualsTo(currentRegion.neighbourRegionPEs[a]) == true)
                    {
                        //�� ������� ��� � ������ ������� �������� ������� �����
                        neighbourRegions.Add(fleetRegions[b]);

                        //� ������� ������� ������ ����� � ������ ������� �������
                        fleetRegions[b].neighbourRegions.Add(this);

                        //������� �� �����
                        break;
                    }
                }
            }
        }
        public void RemoveNeigbours()
        {
            //��� ������� ��������� ������� �����
            for (int a = 0; a < neighbourRegions.Count; a++)
            {
                //������� ������� ������ �� ������ �������
                neighbourRegions[a].neighbourRegions.Remove(this);
            }
        }

        public int searchMissionLastTime;


        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            DFleetRegion objAsFleetRegion = obj as DFleetRegion;

            if (objAsFleetRegion == null)
            {
                return false;
            }
            else
            {
                return Equals(objAsFleetRegion);
            }
        }
        public override int GetHashCode()
        {
            return regionPE.GetHashCode();
        }
        public bool Equals(DFleetRegion other)
        {
            if (other == null)
            {
                return false;
            }

            return regionPE.EqualsTo(other.regionPE);
        }
    }
}