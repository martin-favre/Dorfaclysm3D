using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachineCollection;

namespace StateMachineCollection
{
    public class WaitingState : State
    {
        State mNextState;
        readonly float mWaitDurationSecs;
        readonly float mStartTime;

        // If no nextState is specified the machine will terminate on state transition instead

        public WaitingState(float waitDurationSecs, State nextState)
        {
            mWaitDurationSecs = waitDurationSecs;
            mNextState = nextState;
            mStartTime = Time.time;
        }
        public override State OnDuring()
        {
            if (mStartTime + mWaitDurationSecs < Time.time)
            {
                if (mNextState != null)
                {
                    return mNextState;
                }
                else
                {
                    TerminateMachine();
                }
            }
            return StateMachine.NoTransition();
        }
    }
}