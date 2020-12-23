using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachineCollection;
using System;
using Logging;

public class MiningJob : IJob
{
    [System.Serializable]
    private class SaveData : GenericSaveData<MiningJob>
    {
        public IGenericSaveData activeState;
    }

    StateMachine machine;
    private readonly LilLogger logger;

    public MiningJob(GridActor actor, MiningRequest request, LilLogger logger)
    {
        Debug.Assert(request != null);
        this.logger = logger;
        Debug.Log("Started a mining job");
        machine = new StateMachine(new WalkToBlockState(actor, request, logger));
    }

    public MiningJob(GridActor actor, IGenericSaveData save, LilLogger logger)
    {
        this.logger = logger;
        SaveData saveData = (SaveData)save;
        if (saveData.activeState != null)
        {
            machine = new StateMachine(LoadState(actor, saveData.activeState));
        }

    }

    private State LoadState(GridActor actor, IGenericSaveData activeState)
    {
        logger.Log("Loading my state");
        Type type = activeState.GetSaveType();
        if (type == typeof(WalkToBlockState))
        {
            logger.Log("Apparently I was in a WalkToBlockState");
            return new WalkToBlockState(actor, activeState, logger);
        }
        else if (type == typeof(MineBlockState))
        {
            logger.Log("Apparently I was in a WalkToBlockState");
            return new MineBlockState(actor, activeState, logger);
        }
        else
        {
            throw new Exception("Unknown type " + type.ToString());
        }
    }

    public IGenericSaveData GetSave()
    {
        logger.Log("Saving my MiningJob");
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

        private readonly MiningRequest request;
        private readonly LilLogger logger;

        public WalkToBlockState(GridActor user, MiningRequest request, LilLogger logger) : base(user, 100, 1)
        {
            this.request = request;
            this.logger = logger;
            logger.Log("Initialized a WalkToBlockState with request " + request);
            Debug.Assert(this.request != null);
        }
        public WalkToBlockState(GridActor user, IGenericSaveData saveData, LilLogger logger) : base(user, ((SaveData)saveData).parent)
        {
            this.logger = logger;
            SaveData save = saveData as SaveData;
            this.request = save.request;
            logger.Log("Loaded a WalkToBlockState with request " + request);
            Debug.Assert(this.request != null);
        }
        public override IGenericSaveData GetSave()
        {
            logger.Log("Saving my WalkToBlockState");
            return new SaveData() { parent = base.GetSave(), request = this.request };
        }


        public override Vector3Int GetTargetPos()
        {
            return request.Position;
        }

        public override State OnPathFindFail()
        {
            logger.Log("I failed at finding my path");
            if (!IsMachineTerminated())
            {
                // Avoid returning twice
                logger.Log("I'm returning the request");
                MiningRequestPool.Instance.ReturnRequest(request);
            }
            TerminateMachine();
            return StateMachine.NoTransition();
        }

        public override State OnReachedTarget()
        {
            logger.Log("I reached the block!");
            return new MineBlockState(user, request, logger);
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
        private readonly LilLogger logger;

        public MineBlockState(GridActor user, MiningRequest request, LilLogger logger)
        {
            this.user = user;
            this.request = request;
            this.logger = logger;
            logger.Log("Entering MineBlockState with request " + request);
        }
        public MineBlockState(GridActor user, IGenericSaveData saveData, LilLogger logger) : base(((SaveData)saveData).parent)
        {
            this.user = user;
            SaveData save = saveData as SaveData;
            this.request = save.request;
            logger.Log("Loading MineBlockState with request " + request);
        }
        public override IGenericSaveData GetSave()
        {
            logger.Log("Saving my MineBlockState");
            return new SaveData() { parent = base.GetSave(), request = this.request };
        }

        public override State OnDuring()
        {
            if (!request.IsCancelled())
            {
                Block block;
                GridMap.Instance.TryGetBlock(request.Position, out block);
                if (block != null && block.GetType() == request.BlockType)
                {
                    logger.Log("Mined the block at " + request.Position);
                    GridMap.Instance.SetBlock(request.Position, new AirBlock());
                    logger.Log("Finished the request! " + request);
                    MiningRequestPool.Instance.FinishRequest(request);
                }
                else
                {
                    logger.Log("Could not mine block, it did not match request", LogLevel.Warning);
                    MiningRequestPool.Instance.CancelRequest(request);
                }
            }
            else
            {
                logger.Log("My request was cancelled");
            }
            TerminateMachine();
            return StateMachine.NoTransition();
        }
    }
}
