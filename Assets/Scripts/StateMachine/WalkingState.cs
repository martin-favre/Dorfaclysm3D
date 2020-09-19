using System.Threading.Tasks;
using UnityEngine;

namespace StateMachineCollection
{
    public abstract class WalkingState : State
    {
        StateMachine mMachine;
        readonly Task<Astar.Result> mAstarTask;
        readonly GridActor mUser;
        readonly Vector3Int mTargetPos;
        readonly float mTimePerStepSecs;

        public WalkingState(GridActor user, Vector3Int targetPos, float secPerStep)
        {
            mUser = user;
            mTargetPos = targetPos;
            mAstarTask = Task.Run(() => new Astar().CalculatePath(mUser.GetPos(), targetPos));
            mMachine = new StateMachine(new AwaitingAstarState(this));
            mTimePerStepSecs = secPerStep;
        }
        public override State OnDuring()
        {
            mMachine.Update();
            if(mMachine.IsTerminated()){
                // did we find a path and did we go all the way there?
                if(mAstarTask.Result.foundPath && mAstarTask.Result.path.Count == 0){
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
            return mAstarTask.Result.failReason;
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
                if (mParent.mAstarTask.IsCompleted)
                {
                    if (mParent.mAstarTask.Result.foundPath)
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
                if (mParent.mAstarTask.Result.path.Count > 0)
                {
                    Vector3Int nextPos = mParent.mAstarTask.Result.path.Pop();
                    mParent.mUser.Move(nextPos);
                    return new WaitingState(mParent.mTimePerStepSecs, new TakeStepState(mParent));
                } 
                TerminateMachine();
                return StateMachine.NoTransition();
            }
        }

    }
}