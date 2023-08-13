
using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedSystems;
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
        public RegionsData regionsData;
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

            //Системы, работающие каждый кадр
            perFrameSystems
                //Загрузка данных с диска
                .Add(new SLoad())

                //Рассчёт технологий запуска игры
                .Add(new STechnologyWorkshopCalculation())

                //Группа, отвечающая за начало новой игры
                //Группа включается при нажатии на кнопку "начать новую игру"
                .AddGroup("NewGame", false, null,
                //Начало инициализации новой игры
                new SNewGameInitializationMain(),

                //Генератор ландшафта и климата
                new SHexasphere(),

                //Создание игроков
                new SPlayerControl(),

                //Рассчёт технологий начала новой игры
                new STechnologyNewGameCalculation(),
                //Генератор организаций

                //Создание организаций
                new SOrganizationControl(),
                //Распределитель инициализаторов
                new SMapInitializerControl())
                //Группа выключается в SEventControl в том же кадре

                //Создание игроков
                .Add(new SPlayerControl())
                //Создание организаций
                .Add(new SOrganizationControl())

                //Рассчёт технологий внутриигровой
                .Add(new STechnologyGameCalculation())

                //Обработка ввода
                .Add(new SUIInput())

                //Управление событиями, приходящими со всех систем
                .Add(new SEventControl())

                //Визуализация
                .Add(new SUIDisplay())
                //Управление картой
                .Add(new SMapControl())

                //Сохранение данных на диск
                .Add(new SSave())

                //Очистка событий
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
                designerData,
                inputData,
                runtimeData)
                .Inject(eUI);

            //Системы, работающие каждый тик
            perTickSystems
                //Создание сущностей кораблей
                //.Add(new SShipGroupCreating())

                //Движение групп кораблей
                //Построение маршрута
                //.Add(new SMTShipGroupPathfinding())
                //Рассчёт движения кораблей
                //.Add(new SMTShipGroupMoving())
                //Вынос кораблей, меняющих регион, в отдельный список
                //.Add(new SMTRegionShipGroupOwnershipChange())
                //Перенос кораблей из региона в регион
                //.Add(new SRegionShipGroupOwnershipChange())
                
                //Обработка посадки групп кораблей на уровне региона
                //.Add(new SMTRegionShipGroupLanding())
                //Обработка посадки групп кораблей на уровне RAEO
                //.Add(new SMTRAEOShipGroupLanding())
                //Обработка посадки групп кораблей на уровне ORAEO
                //.Add(new SMTORAEOShipGroupLanding())

                //Удаление компонентов после окончания движения
                //.Add(new SShipGroupMovingStop())
                //Движение групп кораблей

                //Смена владельца RAEO
                .Add(new SRAEOChangeOwner())

                //Исследование RAEO силами EcORAEO
                .Add(new SMTEcORAEOExplorationCalculate())
                //Исследование RAEO силами кораблей в RAEO
                .Add(new SMTRAEOExplorationCalculate())

                //Обновление интерфейса после тика
                .Add(new SUITickUpdate())
                
                .Inject(
                staticData,
                sceneData,
                contentData,
                mapGenerationData,
                regionsData,
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