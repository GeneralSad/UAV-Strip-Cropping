﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_App.Drone_Patrol.States
{
    public class PreparingState : IPatrolState
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

        public IPatrolState run()
        {
            return null;
        }
    }
}
