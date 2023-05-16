using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_App.Drone_Patrol
{
    public enum ParentState
    {
        NONE,
        READY,
        PATROUILLING
    }
    public interface IPatrolState
    {
        ParentState getParent();
        IPatrolState run();
        void onEnter();
        void onLeave();
    }
}
