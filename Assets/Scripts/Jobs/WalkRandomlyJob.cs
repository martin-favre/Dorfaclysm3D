
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
        Vector3Int size = GridMap.Instance.GetSize();
        Vector3Int pos = Helpers.GetRandom(Vector3Int.zero, size);
        machine = new StateMachine(new WalkRandomlyState(user, pos, logger));
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
        if (type == typeof(WalkingState))
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

    private class WalkRandomlyState : WalkingState
    {
        Vector3Int targetPos;
        private readonly LilLogger logger;

        public WalkRandomlyState(GridActor user, Vector3Int targetPos, LilLogger logger) : base(user, 0.5f)
        {
            this.targetPos = targetPos;
            this.logger = logger;
            logger.Log("Initialized a WalkRandomlyState");
        }

        public WalkRandomlyState(GridActor user, IGenericSaveData save, LilLogger logger) : base(user, save)
        {
            this.logger = logger;
        }

        public override Vector3Int GetTargetPos()
        {
            Vector3Int actualPos;
            bool success = GridMapHelper.GetClosestPassablePosition(targetPos, 5, out actualPos);
            if (success)
            {
                logger.Log("I'll randomly walk to " + actualPos.ToString());
                return actualPos;
            }
            else
            {
                logger.Log("I couldn't find a good position to walk to");
                OnPathFindFail();
                return targetPos;
            }

        }

        public override State OnPathFindFail()
        {
            logger.Log("I could not get to where I wanted due to: " + GetFailReason().ToString());
            return new WaitState(logger);
            
        }

        public override State OnReachedTarget()
        {
            logger.Log("I reached where I wanted to go!");
            return new WaitState(logger);
        }
    }

    private class WaitState : WaitingState
    {
        private readonly LilLogger logger;

        public WaitState(LilLogger logger) : base(3)
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

