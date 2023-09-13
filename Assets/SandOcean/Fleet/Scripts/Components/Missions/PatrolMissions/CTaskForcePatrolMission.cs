namespace SandOcean.Warfare.TaskForce.Missions
{
    public struct CTaskForcePatrolMission
    {
        public CTaskForcePatrolMission(int a)
        {
            missionStatus = TaskForceMissionStatus.None;
        }

        public TaskForceMissionStatus missionStatus;
    }
}