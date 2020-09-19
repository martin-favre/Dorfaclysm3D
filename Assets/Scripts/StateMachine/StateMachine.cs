using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachineCollection
{

    public abstract class State
    {

        private bool mMachineTerminated = false;
        public virtual void OnEntry() { }
        public abstract State OnDuring();
        public virtual void OnExit() { }
        protected void TerminateMachine()
        {
            mMachineTerminated = true;
        }
        public bool IsMachineTerminated()
        {
            return mMachineTerminated;
        }
    }

    public class StateMachine
    {

        State mActiveState;
        public StateMachine(State initialState)
        {
            TransitToState(initialState);
        }

        public bool IsTerminated()
        {
            return mActiveState.IsMachineTerminated();
        }

        public void Update()
        {
            if (!mActiveState.IsMachineTerminated())
            {
                State nextState = mActiveState.OnDuring();
                if (nextState != null)
                {
                    mActiveState.OnExit();
                    TransitToState(nextState);
                }

            }
        }

        private void TransitToState(State nextState)
        {
            mActiveState = nextState;
            if (!mActiveState.IsMachineTerminated())
            {
                mActiveState.OnEntry();
            }

        }
        public static State NoTransition()
        {
            return null;
        }
    }

}