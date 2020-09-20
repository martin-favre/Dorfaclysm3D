using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachineCollection;

namespace StateMachineCollection
{
    public abstract class WaitingState : State
    {
        readonly float waitDurationSecs;
        readonly float startTime;


        // If no nextState is specified the machine will terminate on state transition instead
        public WaitingState(float waitDurationSecs)
        {
            this.waitDurationSecs = waitDurationSecs;
            startTime = Time.time;
        }

        public WaitingState(IGenericSaveData saveData) : base(((SaveData)saveData).parent)
        {
            SaveData save = (SaveData)saveData;
            this.waitDurationSecs = save.waitDurationSecs;
            this.startTime = Time.time - save.passedTime;
        }
        public override State OnDuring()
        {
            if (startTime + waitDurationSecs < Time.time)
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
        [System.Serializable]
        private class SaveData : GenericSaveData<WaitingState>
        {
            public IGenericSaveData parent;
            public float waitDurationSecs;
            public float passedTime;
        }
        public override IGenericSaveData GetSave()
        {
            SaveData save = new SaveData();
            save.parent = base.GetSave();
            save.waitDurationSecs = waitDurationSecs;
            save.passedTime = Time.time - startTime;
            return save;
        }
    }
}