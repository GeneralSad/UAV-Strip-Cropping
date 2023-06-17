using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_App.Drone_Patrol.States
{
    public class IdleState : IPatrolState
    {
        public ParentState getParent()
        {
            return ParentState.READY;
        }

        public void onEnter()
        {
        }

        public void onLeave()
        {
        }

        public IPatrolState HandleEvent(PatrolEvent patrolEvent)
        {
            if (PatrolEvent.StartScoutPatrol == patrolEvent)
            {
                return new PreparingState();
            }
            
            return null;
        }
    }
}
