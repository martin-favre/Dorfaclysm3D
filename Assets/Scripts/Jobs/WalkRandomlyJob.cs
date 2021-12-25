
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachineCollection;
using System;
using Logging;

// Will choose a random position and walk to it
public class WalkRandomlyJob : IJob
{
    [System.Serializable]
    private class SaveData : GenericSaveData<WalkRandomlyJob>
    {
        public IGenericSaveData activeState;
    }
    private StateMachine machine;
    private readonly LilLogger logger;

    public WalkRandomlyJob(GridActor user, LilLogger logger)
    {
        this.logger = logger;
        logger.Log("Starting a WalkRandomlyJob");
        Vector3Int size = SingletonProvider.MainGridMap.GetSize();

        machine = new StateMachine(new WalkRandomlyState(user, logger));
    }

    public WalkRandomlyJob(GridActor user, IGenericSaveData save, LilLogger logger)
    {
        this.logger = logger;
        SaveData saveData = (SaveData)save;
        logger.Log("Loading a WalkRandomlyJob");
        if (saveData.activeState != null)
        {
            machine = new StateMachine(LoadState(user, saveData.activeState));
        }

    }

    private State LoadState(GridActor user, IGenericSaveData activeState)
    {
        logger.Log("Loading a WalkRandomlyState");
        Type type = activeState.GetSaveType();
        if (type == typeof(WalkRandomlyState))
        {
            return new WalkRandomlyState(user, activeState, logger);
        }
        else if (type == typeof(WaitingState))
        {
            return new WaitState(logger);
        }
        else
        {
            throw new Exception("Unknown type " + type.ToString());
        }
    }

    public IGenericSaveData GetSave()
    {
        logger.Log("Saving my WalkRandomlyState");
        SaveData save = new SaveData();
        save.activeState = machine.GetSave();
        return save;
    }

    public bool Work()
    {
        if (machine != null)
        {
            machine.Update();
            return machine.IsTerminated();
        }
        else
        {
            return true;
        }
    }

    private class WalkRandomlyState : State
    {
        Vector3Int targetPos;
        private readonly LilLogger logger;

        private readonly GridActor user;
        [System.Serializable]
        private class SaveData : GenericSaveData<WalkRandomlyState>
        {
            public IGenericSaveData parent;
        }
        public override IGenericSaveData GetSave()
        {
            SaveData save = new SaveData();
            save.parent = base.GetSave();
            return save;
        }

        public WalkRandomlyState(GridActor user, LilLogger logger)
        {
            this.logger = logger;
            this.user = user;
            logger.Log("Initialized a WalkRandomlyState");
        }

        public WalkRandomlyState(GridActor user, IGenericSaveData save, LilLogger logger) : base(save)
        {
            this.logger = logger;
            this.user = user;
        }

        public override State OnDuring()
        {
            Vector3Int currPos = user.Position;
            foreach (var delta in DeltaPositions.GetRandomDeltaPositions3D())
            {
                Vector3Int newPos = currPos + delta;
                if (Astar.IsStepValid(newPos, currPos, delta))
                {
                    user.Move(newPos);
                    break;
                }
            }
            return new WaitState(this.logger);
        }
    }

    private class WaitState : WaitingState
    {
        private readonly LilLogger logger;

        public WaitState(LilLogger logger) : base(1000)
        {
            this.logger = logger;
            logger.Log("Starting my WaitState");
        }

        public override State GetNextState()
        {
            TerminateMachine();
            logger.Log("Finished waiting, i'm finished walking randomly");
            return StateMachine.NoTransition();
        }
    }
}

