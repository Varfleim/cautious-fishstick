
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
                new SMapTerrainClimateControl(),

                //Управление игроками
                new SPlayerControl(),

                //Рассчёт технологий начала новой игры
                new STechnologyNewGameCalculation(),
                //Генератор организаций

                //Управление организациями
                new SOrganizationControl(),
                //Распределитель инициализаторов
                new SMapInitializerControl())
                //Группа выключается в SEventControl в том же кадре

                //Обработка ввода
                .Add(new SUIInput())

                //Управление игроками
                .Add(new SPlayerControl())
                //Управление организациями
                .Add(new SOrganizationControl())

                //Рассчёт технологий внутриигровой
                .Add(new STechnologyGameCalculation())

                //Управление флотами, оперативными группами и шаблонами
                .Add(new SFleetControl())

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
                organizationsData,
                designerData,
                inputData,
                runtimeData)
                .Inject(eUI);

            //Системы, работающие каждый тик
            perTickSystems
                //Управление RAEO
                .Add(new SRAEOControl())
                //Управление сооружениями
                .Add(new SBuildingControl())

                //Управление флотами, оперативными группами и шаблонами
                .Add(new SFleetControl())

                //Миссии флота 1
                //Управление миссиями флотов
                .Add(new SMTFleetMissionControl())

                //Пополнение
                //Проверка необходимости пополнения по оперативным группам
                .Add(new SMTTaskForceReinforcementCheck())
                //Определение целей пополнения и запрос создания групп пополнения
                .Add(new SMTReserveFleetReinforcementRequest())
                //Создание групп пополнения и тут же назначение им миссий пополнения и 
                .Add(new STaskForceReinforcementCreating())
                //Пополнение

                //Поиск, перемещение свободных групп в наиболее давно посещённые регионы флота
                .Add(new SMTFleetPMissionSearchTargetAssign())
                //Ударная группа, отправка свободных групп на перехват обнаруженных противников
                .Add(new SMTFleetTMissionStrikeGroupTargetAssign())
                //Миссии флота 1

                //Поиск пути 
                .Add(new STaskForcePathfindingRequestAssign())
                .Add(new SMTTaskForcePathfinding())
                .Add(new STaskForcePathfindingRequestDelete())
                //Поиск пути 
                //Перемещение
                .Add(new SMTTaskForceMovement())
                .Add(new STaskForceMovementStop())
                //Перемещение

                //Миссии флота 2
                //Проверка цели группы
                .Add(new STaskForceTargetCheck())

                //Пополнение групп
                .Add(new STaskForceReinforcement())

                //Поиск, выполнение поиска оперативными группами

                //Ударная группа,

                //Поиск, обновление времени ожидания и времени последнего посещения регионов
                .Add(new SMTFleetPMissionSearchRegionUpdate())
                //Ударная группа
                //.Add(new SMTFleetTMissionStrikeGroupSecond())

                //Удаление компонента ожидания
                .Add(new STaskForceWaitingDelete())
                
                //Проверка пустых оперативных групп
                .Add(new STaskForceEmptyCheck())
                //Миссии флота 2

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