﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_App.Drone_Movement.States
{
    public class PreparingState : IMovementState
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
            if (MovementEvent.PrepareDone == movementEvent)
            {
                return new HoverState();
            }

            return null;
        }
    }
}