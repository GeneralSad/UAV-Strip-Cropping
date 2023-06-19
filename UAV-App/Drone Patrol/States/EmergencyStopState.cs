﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_App.Drone_Patrol.States
{
    public class EmergencyStopState : IPatrolState
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

        public IPatrolState HandleEvent(PatrolEvent patrolEvent)
        {
            if (PatrolEvent.ExpellDone == patrolEvent)
            {
                return new ScoutPatrolState();
            }

            return null;
        }


        Task IPatrolState.run()
        {
         return Task.FromResult(0);
        }
    }
}
