
using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Unity.Ugui;

using SandOcean.UI;
using SandOcean.UI.Camera;
using SandOcean.Time;
using SandOcean.Player;
using SandOcean.Map;
using SandOcean.Diplomacy;
using SandOcean.AEO.RAEO;
using SandOcean.Technology;
using SandOcean.Designer;
using SandOcean.Ship;
using SandOcean.Ship.Moving;

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

            perFrameSystems
                //�������� ������ � �����
                .Add(new SLoad())

                //������������� ����� ���� - ���
                .Add(new SNewGameInitializationMain())

                //�������� �������
                .Add(new SPlayerInitialization())
                //�������� �������
                .Add(new SOrganizationInitialization())

                //������� ����������
                .Add(new STechnologyCalculation())

                //��������� �����
                .Add(new SHexGrid())

                //������������� ����� ���� - �������
                .Add(new SNewGameInitializationShips())

                //������������� ������
                .Add(new SUICameraInitialization())
                //��������� �����
                .Add(new SUIInput())

                //���������� ���������, ����������� �� ���� ������
                .Add(new SEventsControl())

                //������������
                .Add(new SUIDisplay())
                //���������� ������
                .Add(new SMapControl())

                //���������� ������ �� ����
                .Add(new SSave())

                //������� �������
                .Add(new SEventsClear())

                .AddWorld(new EcsWorld(), "uguiSpaceEventsWorld")
                .InjectUgui(uguiSpaceEmitter, "uguiSpaceEventsWorld")
                .AddWorld(new EcsWorld(), "uguiUIEventsWorld")
                .InjectUgui(uguiUIEmitter, "uguiUIEventsWorld")

                .Inject(
                staticData,
                sceneData,
                contentData,
                mapGenerationData,
                designerData,
                inputData,
                runtimeData)
                .Inject(eUI);

            perTickSystems
                //�������� ��������� ��������
                .Add(new SShipGroupCreating())

                //�������� ����� ��������
                //���������� ��������
                .Add(new SMTShipGroupPathfinding())
                //������� �������� ��������
                .Add(new SMTShipGroupMoving())
                //����� ��������, �������� ������, � ��������� ������
                .Add(new SMTRegionShipGroupOwnershipChange())
                //������� �������� �� ������� � ������
                .Add(new SRegionShipGroupOwnershipChange())
                
                //��������� ������� ����� �������� �� ������ �������
                .Add(new SMTRegionShipGroupLanding())
                //��������� ������� ����� �������� �� ������ RAEO
                .Add(new SMTRAEOShipGroupLanding())
                //��������� ������� ����� �������� �� ������ ORAEO
                .Add(new SMTORAEOShipGroupLanding())

                //�������� ����������� ����� ��������� ��������
                .Add(new SShipGroupMovingStop())
                //�������� ����� ��������

                //����� ��������� RAEO
                .Add(new SRAEOChangeOwner())

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