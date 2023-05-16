using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_App.Drone_Movement
{
    public class DroneMovementStateMachine
    {
        public List<IMovementState> parentStateHistory;
        public IMovementState activeState;

        public void switchState(IMovementState state)
        {

        }

        public void run()
        {

        }
    }
}
