using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_App.Drone_Movement
{
    public enum MovementEvent
    {
        None = 0,
        Prepare = 1,
        PrepareDone = 2,
        MoveToPoint = 3,
        ArrivedAtPoint = 4,
        RouteDone = 5,
        EmergencyStop = 6,
        EmergencyLand = 7,
        EmergencyReturn = 8,
        ContinueRoute = 9,
        LandingDone = 10,
        ReturnedHome = 11,
    }
    public enum ParentState
    {
        NONE,
        FOLLOW_ROUTE
    }
    public interface IMovementState
    {
        ParentState getParent();
        IMovementState run(MovementEvent movementEvent);
        void onEnter();
        void onLeave();
    }
}
