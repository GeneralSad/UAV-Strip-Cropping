using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAV_App.Drone_Movement;

namespace UAV_App.Drone_Patrol
{
    public class PatrolStateMachine
    {
        public List<IPatrolState> parentStateHistory;
        public IPatrolState activeState;

        public void switchState(IPatrolState state)
        {

        }

        public void run()
        {

        }
    }
}
