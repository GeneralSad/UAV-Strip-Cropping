using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_App.Drone_Movement.States
{
    internal class ReturningHomeState : IMovementState
    {
        public ParentState getParent()
        {
            return ParentState.NONE;
        }

        public void onEnter()
        {
        }

        public void onLeave()
        {
        }

        public IMovementState run(MovementEvent movementEvent)
        {
            if (MovementEvent.ReturnedHome == movementEvent)
            {
                return new IdleState();
            }

            return null;
        }

    
    }
}
