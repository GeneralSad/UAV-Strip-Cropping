using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_App.Drone_Movement.States
{
    public class HoverState : IMovementState
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

        public IMovementState run(MovementEvent movementEvent)
        {
            if (MovementEvent.MoveToPoint == movementEvent)
            {
                return new MovingState();
            } else if (MovementEvent.LandingDone == movementEvent)
            {
                return new LandingState();
            }

            return null;
        }
    }
}
