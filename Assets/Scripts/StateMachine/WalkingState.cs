using System;
using System.Threading.Tasks;
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
        }

        readonly StateMachine machine;
        Astar.Result astarResult;
        readonly GridActor user;
        Vector3Int targetPos;
        readonly float timePerStepSecs;

        public WalkingState(GridActor user, float secPerStep)
        {
            Debug.Assert(user != null);
            this.user = user;

            machine = new StateMachine(new AwaitingAstarState(this));
            timePerStepSecs = secPerStep;
        }

        public WalkingState(GridActor user, IGenericSaveData save) : base(((SaveData)save).parent)
        {
            this.user = user;
            SaveData saveData = (SaveData)save;
            this.timePerStepSecs = saveData.timePerStepSecs;
            this.targetPos = saveData.targetPos;
            this.astarResult = saveData.astarResult;

            if (saveData.activeState != null)
            {
                this.machine = new StateMachine(LoadState(saveData.activeState));
            }
            else
            {
                Debug.LogWarning("Walkingstate loaded without a state. Terminating.");
                TerminateMachine();
            }
        }

        public override void OnEntry()
        {
            targetPos = GetTargetPos();
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
                if (astarResult == null) throw new Exception("Can't be in WaitABitState without astar result");
                return new WaitABitState(this, activeState);
            }
            else if (type == typeof(TakeStepState))
            {
                if (astarResult == null) throw new Exception("Can't be in TakeStepState without astar result");
                return new TakeStepState(this);
            }
            else
            {
                throw new Exception("Unknown type " + type.ToString());
            }
        }


        public override IGenericSaveData GetSave()
        {
            SaveData save = new SaveData();
            save.parent = base.GetSave();
            save.activeState = machine.GetSave();
            save.timePerStepSecs = timePerStepSecs;
            save.targetPos = targetPos;
            if (astarResult != null)
            {
                save.astarResult = astarResult;
            }
            return save;
        }

        public override State OnDuring()
        {
            machine.Update();
            if (machine.IsTerminated())
            {
                // did we find a path and did we go all the way there?
                if (astarResult != null && astarResult.foundPath && astarResult.path.Count == 0)
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
            return astarResult != null ? astarResult.failReason : Astar.FailReason.NoFail;
        }

        private class AwaitingAstarState : State
        {
            WalkingState parent;
            Task<Astar.Result> astarTask;

            public AwaitingAstarState(WalkingState parent)
            {
                this.parent = parent;
                this.astarTask = Task.Run(() => new Astar().CalculatePath(parent.user.GetPos(), parent.GetTargetPos()));
            }
            [System.Serializable]
            private class SaveData : GenericSaveData<AwaitingAstarState>
            {
                // Explicitly empty, nothing to save.
                // No need to save the astar calculation
            }

            public override IGenericSaveData GetSave()
            {
                return new SaveData();
            }

            public override State OnDuring()
            {
                if (astarTask.IsCompleted)
                {
                    parent.astarResult = astarTask.Result;
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
                if (parent.astarResult.path.Count > 0)
                {
                    Vector3Int nextPos = parent.astarResult.path.Pop();
                    parent.user.Move(nextPos);
                    return new WaitABitState(parent);
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

            public WaitABitState(WalkingState parent) : base(parent.timePerStepSecs)
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