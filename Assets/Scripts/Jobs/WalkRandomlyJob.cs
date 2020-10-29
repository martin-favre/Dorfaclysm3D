
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachineCollection;
using System;

// Will choose a random position and walk to it
public class WalkRandomlyJob : IJob
{
    [System.Serializable]
    private class SaveData : GenericSaveData<WalkRandomlyJob>
    {
        public IGenericSaveData activeState;
    }
    private StateMachine machine;

    public WalkRandomlyJob(GridActor user)
    {
        Vector3Int size = GridMap.Instance.GetSize();
        Vector3Int pos = Helpers.GetRandom(Vector3Int.zero, size);
        machine = new StateMachine(new WalkRandomlyState(user, pos));
    }

    public WalkRandomlyJob(GridActor user, IGenericSaveData save)
    {
        SaveData saveData = (SaveData)save;
        if (saveData.activeState != null)
        {
            machine = new StateMachine(LoadState(user, saveData.activeState));
        }
    }

    private State LoadState(GridActor user, IGenericSaveData activeState)
    {
        return new WalkRandomlyState(user, activeState);
    }

    public IGenericSaveData GetSave()
    {
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
        public WalkRandomlyState(GridActor user, Vector3Int targetPos) : base(user, 0.2f)
        {
            this.targetPos = targetPos;
        }

        public WalkRandomlyState(GridActor user, IGenericSaveData save) : base(user, save)
        {
            
        }

        public override Vector3Int GetTargetPos()
        {
            Vector3Int actualPos;
            bool success = GridMapHelper.GetClosestPassablePosition(targetPos, 5, out actualPos);
            if (success)
            {
                return actualPos;
            }
            else
            {
                OnPathFindFail();
                return targetPos;
            }

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

