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
        PatrolStarted = 1,
        PatrolDone = 2,
        PrepareDone = 3,
        ArrivedAtPoint = 4,
        NoHarmfullAnimalsFound = 5,
        HarmfullAnimalsFound = 6,
        ExpellDone = 7,
        StopRoute = 8,
        ContinueRoute = 9,

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
        IPatrolState run(PatrolEvent patrolEvent);
        void onEnter();
        void onLeave();
    }
}
