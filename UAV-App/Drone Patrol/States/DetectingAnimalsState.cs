using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_App.Drone_Patrol.States
{
    public class DetectingAnimalsState : IPatrolState
    {
        public ParentState getParent()
        {
            return ParentState.PATROUILLING;
        }

        public void onEnter()
        {
        }

        public void onLeave()
        {
        }

        public IPatrolState HandleEvent(PatrolEvent patrolEvent)
        {
            if (PatrolEvent.HarmfullAnimalsFound == patrolEvent)
            {
                return new ExpelAnimalsState();

            } else if (PatrolEvent.NoHarmfullAnimalsFound == patrolEvent)
            {
                return new ScoutPatrolState();

            }

            return null;
        }
    }
}
