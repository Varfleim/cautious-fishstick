
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace SandOcean.Ship.Moving
{
    public class SShipGroupMovingStop : IEcsRunSystem
    {
        //������ ��������
        readonly EcsPoolInject<CShipGroup> shipGroupPool = default;

        readonly EcsFilterInject<Inc<CSGMoving>> sGMovingFilter = default;
        readonly EcsPoolInject<CSGMoving> sGMovingPool = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ���������� �������� ������ ��������
            foreach (int sGMovingEntity in sGMovingFilter.Value)
            {
                //���� ��������� �������� 
                ref CSGMoving sGMoving = ref sGMovingPool.Value.Get(sGMovingEntity);

                //���� ������ ����� ���� ����
                if (sGMoving.pathPoints.Count == 0)
                {
                    //���� ��������� ������ ��������
                    ref CShipGroup shipGroup = ref shipGroupPool.Value.Get(sGMovingEntity);

                    //��������� ������ �������� � ����� ��������
                    shipGroup.movingMode = Ship.ShipGroupMovingMode.Idle;

                    //������� ��������� �������� 
                    sGMovingPool.Value.Del(sGMovingEntity);
                }
            }
        }
    }
}