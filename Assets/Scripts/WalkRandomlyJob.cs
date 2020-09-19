
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachineCollection;

public class WalkRandomlyJob : IJob
{
    private StateMachine machine;

    public WalkRandomlyJob(GridActor user)
    {
        Vector3Int size = GridMap.GetSize();
        Vector3Int pos = Helpers.GetRandom(Vector3Int.zero, size);
        machine = new StateMachine(new WalkRandomlyState(user, pos));

    }

    public bool Work()
    {
        machine.Update();
        return machine.IsTerminated();
    }

    private class WalkRandomlyState : WalkingState
    {
        public WalkRandomlyState(GridActor user, Vector3Int targetPos) : base(user, targetPos, 0.2f)
        {
        }

        public override State OnPathFindFail()
        {
            TerminateMachine();
            return StateMachine.NoTransition();
        }

        public override State OnReachedTarget()
        {
            TerminateMachine();
            return StateMachine.NoTransition();
        }
    }
}

