using System.Threading.Tasks;
using UnityEngine;

namespace StateMachineCollection
{
    public abstract class WalkingState : State
    {
        StateMachine machine;
        readonly Task<Astar.Result> aStarTask;
        readonly GridActor user;
        readonly Vector3Int targetPos;
        readonly float timePerStepSecs;

        public WalkingState(GridActor user, Vector3Int targetPos, float secPerStep)
        {
            this.user = user;
            this.targetPos = targetPos;
            aStarTask = Task.Run(() => new Astar().CalculatePath(this.user.GetPos(), targetPos));
            machine = new StateMachine(new AwaitingAstarState(this));
            timePerStepSecs = secPerStep;
        }
        public override State OnDuring()
        {
            machine.Update();
            if(machine.IsTerminated()){
                // did we find a path and did we go all the way there?
                if(aStarTask.Result.foundPath && aStarTask.Result.path.Count == 0){
                    return OnReachedTarget();
                } else {
                    return OnPathFindFail();
                }
            }
            return StateMachine.NoTransition();
        }

        public abstract State OnReachedTarget();
        public abstract State OnPathFindFail();

        public Astar.FailReason GetFailReason(){
            return aStarTask.Result.failReason;
        }

        private class AwaitingAstarState : State
        {
            WalkingState mParent;

            public AwaitingAstarState(WalkingState parent)
            {
                mParent = parent;
            }
            public override State OnDuring()
            {
                if (mParent.aStarTask.IsCompleted)
                {
                    if (mParent.aStarTask.Result.foundPath)
                    {
                        return new TakeStepState(mParent);
                    }
                    else
                    {
                        TerminateMachine();
                    }
                }
                return StateMachine.NoTransition();
            }
        }

        private class TakeStepState : State
        {
            WalkingState mParent;

            public TakeStepState(WalkingState parent)
            {
                mParent = parent;
            }

            public override State OnDuring()
            {
                if (mParent.aStarTask.Result.path.Count > 0)
                {
                    Vector3Int nextPos = mParent.aStarTask.Result.path.Pop();
                    mParent.user.Move(nextPos);
                    return new WaitingState(mParent.timePerStepSecs, new TakeStepState(mParent));
                } 
                TerminateMachine();
                return StateMachine.NoTransition();
            }
        }

    }
}