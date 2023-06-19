using DJI.WindowsSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UAV_App.Drone_Movement;
using UAV_App.Drone_Patrol.States;

namespace UAV_App.Drone_Patrol
{
    public class PatrolStateMachine
    {
        private Dictionary<ParentState, IPatrolState> parentStateHistory;
        private Stack<PatrolEvent> eventStack;
        private IPatrolState activeState;

        public PatrolStateMachine()
        {
            parentStateHistory = new Dictionary<ParentState, IPatrolState>();
            eventStack = new Stack<PatrolEvent>();
        }

        public void SwitchState(IPatrolState state)
        {
            if (activeState != null)
            {
                activeState.onLeave(); //On leave

                switch (activeState.getParent())
                {
                    case ParentState.NONE:
                        break;
                    case ParentState.READY:
                        parentStateHistory[ParentState.READY] = activeState;
                        break;
                    case ParentState.PATROUILLING:
                        parentStateHistory[ParentState.PATROUILLING] = activeState;
                        break;
                }
            }

            Console.WriteLine($"state changed from {this.activeState} to {state}");
            activeState = state;
            activeState.onEnter(); // On enter
        }

        public void HandleEvent(PatrolEvent patrolEvent)
        {
            IPatrolState returnState = activeState.HandleEvent(patrolEvent);

            if (returnState != null)
            {
                SwitchState(returnState);
            }
        }

        private bool History(ParentState historyLevel)
        {
            IPatrolState state;
            switch (historyLevel)
            {

                case ParentState.READY:
                    state = parentStateHistory[ParentState.READY];
                    if (state != null)
                    {
                        activeState = state;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case ParentState.PATROUILLING:
                    state = parentStateHistory[ParentState.PATROUILLING];
                    if (state != null)
                    {
                        activeState = state;
                        return true;
                    }
                    else
                    {
                        return History(ParentState.READY);
                    }
            }

            return false;
        }

        public void StartScoutPatrol()
        {
            HandleEvent(PatrolEvent.StartScoutPatrol);
        }

        public void PrepareDone()
        {
            HandleEvent(PatrolEvent.PrepareDone);
        }

        public void ConnectionLost()
        {
            SwitchState(new ConnectingState());

        }

        public void ConnectionGained()
        {
            if (!History(ParentState.PATROUILLING)) SwitchState(new IdleState());

        }

        public void ExpellAnimals()
        {
             HandleEvent(PatrolEvent.ExpellAnimals);
        }

        internal void MissionDone()
        {
            HandleEvent(PatrolEvent.MissionDone);
        }
    }
}
