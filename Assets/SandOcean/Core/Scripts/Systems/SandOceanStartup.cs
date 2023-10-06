
using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedSystems;
using Leopotam.EcsLite.Unity.Ugui;

using SandOcean.UI;
using SandOcean.UI.Events;
using SandOcean.UI.Camera;
using SandOcean.Time;
using SandOcean.Player;
using SandOcean.Map;
using SandOcean.Organization;
using SandOcean.AEO.RAEO;
using SandOcean.Economy.Building;
using SandOcean.Technology;
using SandOcean.Designer;
using SandOcean.Warfare.Fleet;
using SandOcean.Warfare.Fleet.Moving;
using SandOcean.Warfare.TaskForce;
using SandOcean.Warfare.TaskForce.Template;
using SandOcean.Warfare.TaskForce.Missions;

namespace SandOcean
{
    public class SandOceanStartup : MonoBehaviour
    {
        EcsWorld world;
        EcsSystems perFrameSystems;
        EcsSystems perTickSystems;

        [SerializeField] EcsUguiEmitter uguiSpaceEmitter;
        [SerializeField] EcsUguiEmitter uguiUIEmitter;

        public StaticData staticData;

        public SceneData sceneData;
        public ContentData contentData;
        public MapGenerationData mapGenerationData;
        public RegionsData regionsData;
        public OrganizationsData organizationsData;
        public DesignerData designerData;
        public InputData inputData;


        public RuntimeData runtimeData;

        public EUI eUI;

        void Start()
        {
            world = new EcsWorld();
            perFrameSystems = new EcsSystems(world);
            perTickSystems = new EcsSystems(world);
            RuntimeData runtimeData = new();

            Random.InitState(sceneData.seed);
            Formulas.random = new System.Random(sceneData.seed);

            //�������, ���������� ������ ����
            perFrameSystems
                //�������� ������ � �����
                .Add(new SLoad())

                //������� ���������� ������� ����
                .Add(new STechnologyWorkshopCalculation())

                //������, ���������� �� ������ ����� ����
                //������ ���������� ��� ������� �� ������ "������ ����� ����"
                .AddGroup("NewGame", false, null,
                //������ ������������� ����� ����
                new SNewGameInitializationMain(),

                //��������� ��������� � �������
                new SMapTerrainClimateControl(),

                //���������� ��������
                new SPlayerControl(),

                //������� ���������� ������ ����� ����
                new STechnologyNewGameCalculation(),
                //��������� �����������

                //���������� �������������
                new SOrganizationControl(),
                //�������������� ���������������
                new SMapInitializerControl())
                //������ ����������� � SEventControl � ��� �� �����

                //��������� �����
                .Add(new SUIInput())

                //���������� ��������
                .Add(new SPlayerControl())
                //���������� �������������
                .Add(new SOrganizationControl())

                //������� ���������� �������������
                .Add(new STechnologyGameCalculation())

                //���������� �������, ������������ �������� � ���������
                .Add(new SFleetControl())

                //���������� ���������, ����������� �� ���� ������
                .Add(new SEventControl())

                //������������
                .Add(new SUIDisplay())
                //���������� ������
                .Add(new SMapControl())

                //���������� ������ �� ����
                .Add(new SSave())

                //������� �������
                .Add(new SEventClear())

                .AddWorld(new EcsWorld(), "uguiSpaceEventsWorld")
                .InjectUgui(uguiSpaceEmitter, "uguiSpaceEventsWorld")
                .AddWorld(new EcsWorld(), "uguiUIEventsWorld")
                .InjectUgui(uguiUIEmitter, "uguiUIEventsWorld")

                .Inject(
                staticData,
                sceneData,
                contentData,
                mapGenerationData,
                regionsData,
                organizationsData,
                designerData,
                inputData,
                runtimeData)
                .Inject(eUI);

            //�������, ���������� ������ ���
            perTickSystems
                //���������� RAEO
                .Add(new SRAEOControl())
                //���������� ������������
                .Add(new SBuildingControl())

                //���������� �������, ������������ �������� � ���������
                .Add(new SFleetControl())

                //������ ����� 1
                //���������� �������� ������
                .Add(new SMTFleetMissionControl())

                //����������
                //�������� ������������� ���������� �� ����������� �������
                .Add(new SMTTaskForceReinforcementCheck())
                //����������� ����� ���������� � ������ �������� ����� ����������
                .Add(new SMTReserveFleetReinforcementRequest())
                //�������� ����� ���������� � ��� �� ���������� �� ������ ���������� � 
                .Add(new STaskForceReinforcementCreating())
                //����������

                //�����, ����������� ��������� ����� � �������� ����� ���������� ������� �����
                .Add(new SMTFleetPMissionSearchTargetAssign())
                //������� ������, �������� ��������� ����� �� �������� ������������ �����������
                .Add(new SMTFleetTMissionStrikeGroupTargetAssign())
                //������ ����� 1

                //����� ���� 
                .Add(new STaskForcePathfindingRequestAssign())
                .Add(new SMTTaskForcePathfinding())
                .Add(new STaskForcePathfindingRequestDelete())
                //����� ���� 
                //�����������
                .Add(new SMTTaskForceMovement())
                .Add(new STaskForceMovementStop())
                //�����������

                //������ ����� 2
                //�������� ���� ������
                .Add(new STaskForceTargetCheck())

                //���������� �����
                .Add(new STaskForceReinforcement())

                //�����, ���������� ������ ������������ ��������

                //������� ������,

                //�����, ���������� ������� �������� � ������� ���������� ��������� ��������
                .Add(new SMTFleetPMissionSearchRegionUpdate())
                //������� ������
                //.Add(new SMTFleetTMissionStrikeGroupSecond())

                //�������� ���������� ��������
                .Add(new STaskForceWaitingDelete())
                
                //�������� ������ ����������� �����
                .Add(new STaskForceEmptyCheck())
                //������ ����� 2

                //������������ RAEO ������ EcORAEO
                .Add(new SMTEcORAEOExplorationCalculate())
                //������������ RAEO ������ �������� � RAEO
                .Add(new SMTRAEOExplorationCalculate())

                //���������� ���������� ����� ����
                .Add(new SUITickUpdate())
                
                .Inject(
                staticData,
                sceneData,
                contentData,
                mapGenerationData,
                regionsData,
                organizationsData,
                designerData,
                inputData,
                runtimeData)
                .Inject(eUI);

            perFrameSystems.Init();
            perTickSystems.Init();

            TimeTickSystem.Create();


            TimeTickSystem.OnTick += delegate (object sender, TimeTickSystem.OnTickEventArgs e)
            {
                if (runtimeData.isGameActive == true)
                {
                    Debug.Log("Stage 1 " + System.DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss:fff"));
                    perTickSystems.Run();
                    Debug.Log("Stage 2 " + System.DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss:fff"));
                }
            };
        }

        void Update()
        {
            perFrameSystems?.Run();
        }

        void OnDestroy()
        {
            if (perFrameSystems != null)
            {
                perFrameSystems.Destroy();
                perFrameSystems = null;
            }
            if (perTickSystems != null)
            {
                perTickSystems.Destroy();
                perTickSystems = null;
            }

            if (world != null)
            {
                world.Destroy();
                world = null;
            }
        }
    }
}