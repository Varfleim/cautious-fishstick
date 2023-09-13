
using System.Collections.Generic;

using Leopotam.EcsLite;

using SandOcean.Warfare.TaskForce.Template;
using SandOcean.Warfare.Ship;

namespace SandOcean.Warfare.TaskForce.Missions
{
    public enum TaskForceMissionStatus : byte
    {
        None,
        Movement,
        Waiting
    }
}

namespace SandOcean.Warfare.TaskForce
{
    public enum TaskForceMissionType : byte
    {
        PatrolSearchMission,
        TargetMissionStrikeGroup,
        TargetMissionReinforcement,
        HoldMission
    }

    public enum TaskForceMovementTargetType : byte
    {
        None,
        Region,
        TaskForce
    }

    public struct CTaskForce
    {
        public CTaskForce(
            EcsPackedEntity selfPE,
            EcsPackedEntity parentFleetPE)
        {
            this.selfPE = selfPE;
            template = null;

            this.parentFleetPE = parentFleetPE;

            currentRegionPE = new();
            previousRegionPE = new();
            movementTargetType = TaskForceMovementTargetType.None;
            movementTargetPE = new();


            activeMissionType = TaskForceMissionType.HoldMission;


            reinforcementRequest = null;
            ships = new();
            rand = 0;
        }

        public readonly EcsPackedEntity selfPE;
        public DTFTemplate template;

        public EcsPackedEntity parentFleetPE;


        public EcsPackedEntity currentRegionPE;
        public EcsPackedEntity previousRegionPE;
        public TaskForceMovementTargetType movementTargetType;
        public EcsPackedEntity movementTargetPE;


        public TaskForceMissionType activeMissionType;


        public DTFReinforcementRequest reinforcementRequest;
        public List<DShip> ships;

        public int rand;
    }
}