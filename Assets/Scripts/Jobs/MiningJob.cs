using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachineCollection;
using System;

public class MiningJob : IJob
{
    [System.Serializable]
    private class SaveData : GenericSaveData<MiningJob>
    {
        public IGenericSaveData activeState;
    }

    StateMachine machine;

    public MiningJob(GridActor actor, MiningRequest request)
    {
        Debug.Assert(request != null);
        Debug.Log("Started a mining job");
        machine = new StateMachine(new WalkToBlockState(actor, request));
    }

    public MiningJob(GridActor actor, IGenericSaveData save)
    {
        SaveData saveData = (SaveData)save;
        if (saveData.activeState != null)
        {
            machine = new StateMachine(LoadState(actor, saveData.activeState));
        }

    }

    private State LoadState(GridActor actor, IGenericSaveData activeState)
    {
        Type type = activeState.GetSaveType();
        if (type == typeof(WalkToBlockState))
        {
            return new WalkToBlockState(actor, activeState);
        }
        else if (type == typeof(MineBlockState))
        {
            return new MineBlockState(actor, activeState);
        }
        else
        {
            throw new Exception("Unknown type " + type.ToString());
        }
    }

    public IGenericSaveData GetSave()
    {
        return new SaveData() { activeState = machine.GetSave() };
    }

    public bool Work()
    {
        machine.Update();
        return machine.IsTerminated();
    }

    private class WalkToBlockState : WalkingState
    {
        [System.Serializable]
        private class SaveData : GenericSaveData<WalkToBlockState>
        {
            public IGenericSaveData parent;
            public MiningRequest request;
        }

        private readonly GridActor user;
        private readonly MiningRequest request;

        public WalkToBlockState(GridActor user, MiningRequest request) : base(user, 0.3f)
        {
            this.user = user;
            this.request = request;
            Debug.Assert(this.request != null);
        }
        public WalkToBlockState(GridActor user, IGenericSaveData saveData) : base(user, ((SaveData)saveData).parent)
        {
            this.user = user;
            SaveData save = saveData as SaveData;
            this.request = save.request;
            Debug.Assert(this.request != null);
        }
        public override IGenericSaveData GetSave()
        {
            return new SaveData() { parent = base.GetSave(), request = this.request };
        }


        public override Vector3Int GetTargetPos()
        {
            Vector3Int actualPos;
            Debug.Assert(request != null);
            bool success = GridMapHelper.GetClosestPassablePosition(request.Position, 1, out actualPos);
            if (success)
            {
                return actualPos;
            }
            else
            {
                OnPathFindFail();
                return request.Position;
            }
        }

        public override State OnPathFindFail()
        {
            MiningRequestPool.Instance.ReturnRequest(request);
            TerminateMachine();
            return StateMachine.NoTransition();
        }

        public override State OnReachedTarget()
        {
            return new MineBlockState(user, request);
        }
    }

    private class MineBlockState : State
    {
        [System.Serializable]
        private class SaveData : GenericSaveData<MineBlockState>
        {
            public IGenericSaveData parent;
            public MiningRequest request;
        }

        private readonly GridActor user;
        private readonly MiningRequest request;

        public MineBlockState(GridActor user, MiningRequest request)
        {
            this.user = user;
            this.request = request;
        }
        public MineBlockState(GridActor user, IGenericSaveData saveData) : base(((SaveData)saveData).parent)
        {
            this.user = user;
            SaveData save = saveData as SaveData;
            this.request = save.request;
        }
        public override IGenericSaveData GetSave()
        {
            return new SaveData() { parent = base.GetSave(), request = this.request };
        }

        public override State OnDuring()
        {
            if (!request.IsCancelled())
            {
                Block block;
                GridMap.TryGetBlock(request.Position, out block);
                if (block != null && block.Type == request.BlockType)
                {
                    GridMap.SetBlock(request.Position, new AirBlock());
                    request.Finish();
                }
            }
            TerminateMachine();
            return StateMachine.NoTransition();
        }
    }
}
