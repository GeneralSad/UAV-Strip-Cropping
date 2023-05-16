using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_App.Drone_Movement.States
{
    public class MovingState : IMovementState
    {
        public ParentState getParent()
        {
            return ParentState.FOLLOW_ROUTE;
        }

        public void onEnter()
        {
        }

        public void onLeave()
        {
        }

        public IMovementState run()
        {
            return null;
        }
    }
}
