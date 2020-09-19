using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachineCollection
{

    public abstract class State
    {

        private bool machineTerminated = false;
        public virtual void OnEntry() { }
        public abstract State OnDuring();
        public virtual void OnExit() { }
        protected void TerminateMachine()
        {
            machineTerminated = true;
        }
        public bool IsMachineTerminated()
        {
            return machineTerminated;
        }
    }

    public class StateMachine
    {

        State activeState;
        public StateMachine(State initialState)
        {
            TransitToState(initialState);
        }

        public bool IsTerminated()
        {
            return activeState.IsMachineTerminated();
        }

        public void Update()
        {
            if (!activeState.IsMachineTerminated())
            {
                State nextState = activeState.OnDuring();
                if (nextState != null)
                {
                    activeState.OnExit();
                    TransitToState(nextState);
                }

            }
        }

        private void TransitToState(State nextState)
        {
            activeState = nextState;
            if (!activeState.IsMachineTerminated())
            {
                activeState.OnEntry();
            }

        }
        public static State NoTransition()
        {
            return null;
        }
    }

}