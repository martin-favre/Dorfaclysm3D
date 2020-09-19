using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachineCollection;

namespace StateMachineCollection
{
    public class WaitingState : State
    {
        State nextState;
        readonly float waitDurationSecs;
        readonly float startTime;

        // If no nextState is specified the machine will terminate on state transition instead

        public WaitingState(float waitDurationSecs, State nextState)
        {
            this.waitDurationSecs = waitDurationSecs;
            this.nextState = nextState;
            startTime = Time.time;
        }
        public override State OnDuring()
        {
            if (startTime + waitDurationSecs < Time.time)
            {
                if (nextState != null)
                {
                    return nextState;
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