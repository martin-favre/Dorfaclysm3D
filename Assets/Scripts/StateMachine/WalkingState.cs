using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using UnityEngine;

namespace StateMachineCollection
{
    public abstract class WalkingState : State
    {
        [System.Serializable]
        private class SaveData : GenericSaveData<WalkingState>
        {
            public IGenericSaveData parent;
            public IGenericSaveData activeState;
            public float timePerStepSecs;
            public Vector3Int targetPos;
            public Astar.Result astarResult;
            public int aStarMargin;
        }

        readonly StateMachine machine;
        readonly GridActor user;
        SaveData data = new SaveData();

        public WalkingState(GridActor user, float secPerStep) : this(user, secPerStep, 0){}
        public WalkingState(GridActor user, float secPerStep, int margin)
        {
            Debug.Assert(user != null);
            this.user = user;
            this.data.aStarMargin = margin;
            this.data.timePerStepSecs = secPerStep;

            machine = new StateMachine(new AwaitingAstarState(this));
        }

        public WalkingState(GridActor user, IGenericSaveData save) : base(((SaveData)save).parent)
        {
            this.user = user;
            this.data = (SaveData)save;

            if (data.activeState != null)
            {
                this.machine = new StateMachine(LoadState(data.activeState));
            }
            else
            {
                Debug.LogWarning("Walkingstate loaded without a state. Terminating.");
                TerminateMachine();
            }
        }

        public override void OnEntry()
        {
            data.targetPos = GetTargetPos();
        }

        private State LoadState(IGenericSaveData activeState)
        {

            Type type = activeState.GetSaveType();
            if (type == typeof(AwaitingAstarState))
            {
                return new AwaitingAstarState(this);
            }
            else if (type == typeof(WaitABitState))
            {
                if (data.astarResult == null) throw new Exception("Can't be in WaitABitState without astar result");
                return new WaitABitState(this, activeState);
            }
            else if (type == typeof(TakeStepState))
            {
                if (data.astarResult == null) throw new Exception("Can't be in TakeStepState without astar result");
                return new TakeStepState(this);
            }
            else
            {
                throw new Exception("Unknown type " + type.ToString());
            }
        }


        public override IGenericSaveData GetSave()
        {
            data.parent = base.GetSave();
            data.activeState = machine.GetSave();
            return data;
        }

        public override State OnDuring()
        {
            machine.Update();
            if (machine.IsTerminated())
            {
                // did we find a path and did we go all the way there?
                if (data.astarResult != null && data.astarResult.foundPath && data.astarResult.path.Count == 0)
                {
                    return OnReachedTarget();
                }
                else
                {
                    return OnPathFindFail();
                }
            }
            return StateMachine.NoTransition();
        }

        public abstract Vector3Int GetTargetPos();
        public abstract State OnReachedTarget();
        public abstract State OnPathFindFail();

        public Astar.FailReason GetFailReason()
        {
            return data.astarResult != null ? data.astarResult.failReason : Astar.FailReason.NoFail;
        }

        private class AwaitingAstarState : State
        {
            [System.Serializable]
            private class SaveData : GenericSaveData<AwaitingAstarState>
            {
                // Explicitly empty, nothing to save.
                // No need to save the astar calculation
            }
            WalkingState parent;
            Task<Astar.Result> astarTask;

            public AwaitingAstarState(WalkingState parent)
            {
                this.parent = parent;
            }

            public override void OnEntry()
            {
                this.astarTask = WorkStealingTaskScheduler.Run(() => { return new Astar().CalculatePath(parent.user.GetPos(), parent.targetPos, parent.aStarMargin); });
                // this.astarTask = Task.Run(() => new Astar().CalculatePath(parent.user.GetPos(), parent.targetPos));
            } 

            public override IGenericSaveData GetSave()
            {
                return new SaveData();
            }

            public override State OnDuring()
            {
                if (astarTask.IsCompleted)
                {
                    parent.data.astarResult = astarTask.Result;
                    Debug.Log("Calculation took: " + astarTask.Result.executionTime + "ms " + "Result " + astarTask.Result.failReason.ToString());
                    if (astarTask.Result.foundPath)
                    {
                        return new TakeStepState(parent);
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
            [System.Serializable]
            private class SaveData : GenericSaveData<TakeStepState>
            {
                public IGenericSaveData parent;
            }

            WalkingState parent;

            public TakeStepState(WalkingState parent)
            {
                this.parent = parent;
            }
            public TakeStepState(WalkingState parent, IGenericSaveData save) : base(((SaveData)save).parent)
            {
                this.parent = parent;
            }


            public override IGenericSaveData GetSave()
            {
                SaveData save = new SaveData();
                save.parent = base.GetSave();
                return save;
            }

            public override State OnDuring()
            {
                if (parent.data.astarResult.path.Count > 0)
                {
                    Vector3Int nextPos = parent.data.astarResult.path.Pop();
                    if(GridMap.Instance.IsPosFree(nextPos)){
                        parent.user.Move(nextPos);
                        return new WaitABitState(parent);
                    } else {
                        // Okay, whatever path we were walking along is no longer valid
                        // Try to figure out another one.
                        return new AwaitingAstarState(parent);
                    }
                }
                TerminateMachine();
                return StateMachine.NoTransition();
            }
        }

        class WaitABitState : WaitingState
        {
            [System.Serializable]
            private class SaveData : GenericSaveData<WaitABitState>
            {
                public IGenericSaveData parentSave;
            }

            WalkingState parent;

            public WaitABitState(WalkingState parent) : base(parent.data.timePerStepSecs)
            {
                this.parent = parent;
            }

            public WaitABitState(WalkingState parent, IGenericSaveData save) : base(((SaveData)save).parentSave)
            {
                this.parent = parent;
            }

            public override State GetNextState()
            {
                return new TakeStepState(parent);
            }

            public override IGenericSaveData GetSave()
            {
                SaveData save = new SaveData();
                save.parentSave = base.GetSave();
                return save;
            }


        }

    }
}