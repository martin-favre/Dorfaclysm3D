using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachineCollection;

namespace StateMachineCollection
{
    public abstract class WaitingState : State
    {
        [System.Serializable]
        private class SaveData : GenericSaveData<WaitingState>
        {
            public IGenericSaveData parent;
            public IGenericSaveData timerSave;
            public bool elapsed;
        }

        SaveData data = new SaveData();

        public ITimer timer;

        // If no nextState is specified the machine will terminate on state transition instead
        public WaitingState(float waitDurationMs)
        {
            timer = SingletonProvider.MainTimerFactory.GetPausableTimer(waitDurationMs);
            timer.Elapsed += ((a, b) => { data.elapsed = true; });
            timer.Start();
        }

        public WaitingState(IGenericSaveData saveData) : base(((SaveData)saveData).parent)
        {
            data = (SaveData)saveData;
            timer = SingletonProvider.MainTimerFactory.GetPausableTimer(data.timerSave);
            if (!data.elapsed)
            {
                timer.Elapsed += ((a, b) => { data.elapsed = true; });
                timer.Start();
            }
        }
        public override State OnDuring()
        {
            if (data.elapsed)
            {
                State nextState = GetNextState();
                if (nextState != null)
                {
                    return nextState;
                }
                else
                {
                    TerminateMachine();
                }
            }
            return StateMachine.NoTransition();
        }

        public abstract State GetNextState();
        public override IGenericSaveData GetSave()
        {
            SaveData save = new SaveData();
            save.parent = base.GetSave();
            save.timerSave = timer.GetSave();
            return save;
        }
    }
}