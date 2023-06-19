using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_App.Drone_Patrol
{
    public enum PatrolEvent
    {
        None = 0,
        StartScoutPatrol = 1,
        MissionDone = 2,
        PrepareDone = 3,
        ExpellAnimals = 4,
        NoHarmfullAnimalsFound = 5,
        HarmfullAnimalsFound = 6,
        ExpellDone = 7,
        StopRoute = 8,
        ContinueRoute = 9,
        LandingDone = 10,
    }

    public enum ParentState
    {
        NONE,
        READY,
        PATROUILLING
    }
    public interface IPatrolState
    {
        ParentState getParent();
        IPatrolState HandleEvent(PatrolEvent patrolEvent);
        void onEnter();
        void onLeave();
        Task run();
    }
}
