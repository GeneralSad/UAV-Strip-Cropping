using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_App.Drone_Movement
{
    public enum ParentState
    {
        NONE,
        FOLLOW_ROUTE
    }
    public interface IMovementState
    {
        ParentState getParent();
        IMovementState Run();
        void OnEnter();
        void OnLeave();
    }
}
