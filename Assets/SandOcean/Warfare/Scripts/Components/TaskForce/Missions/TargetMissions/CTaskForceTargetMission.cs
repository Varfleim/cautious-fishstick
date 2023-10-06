namespace SandOcean.Warfare.TaskForce.Missions
{
    public struct CTaskForceTargetMission
    {
        public CTaskForceTargetMission(int a)
        {
            missionStatus = TaskForceMissionStatus.None;
        }

        public TaskForceMissionStatus missionStatus;
    }
}