using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachineCollection
{

    public abstract class State
    {

        private bool machineTerminated = false;

        public State() { }
        public State(IGenericSaveData save)
        {
            SaveData saveData = (SaveData)save;
            this.machineTerminated = saveData.machineTerminated;
        }
        public virtual void OnEntry() { }
        public abstract State OnDuring();
        public virtual void OnExit() { }
        protected void TerminateMachine()
        {
            machineTerminated = true;
        }
        public bool IsMachineTerminated()
        {
            return machineTerminated;
        }


        [System.Serializable]
        private class SaveData : GenericSaveData<State>
        {
            public bool machineTerminated;
        }

        public virtual IGenericSaveData GetSave()
        {
            SaveData save = new SaveData();
            save.machineTerminated = machineTerminated;
            return save;
        }
    }

    public class StateMachine
    {
        State activeState; 
        bool isFirstEntryExecuted = false;
        public StateMachine(State initialState)
        {   
            if (initialState == null) throw new System.Exception("Initial state is null");
            activeState = initialState;
        }

        public IGenericSaveData GetSave()
        {
            return activeState.GetSave();
        }

        public bool IsTerminated()
        {
            return activeState.IsMachineTerminated();
        }

        public void Update()
        {

            if (!activeState.IsMachineTerminated())
            {
                if(!isFirstEntryExecuted) {
                    activeState.OnEntry();
                    isFirstEntryExecuted = true;
                }
                State nextState = activeState.OnDuring();
                if (nextState != null)
                {
                    activeState.OnExit();
                    TransitToState(nextState);
                }
            }
        }

        private void TransitToState(State nextState)
        {
            activeState = nextState;
            if (!activeState.IsMachineTerminated())
            {
                activeState.OnEntry();
            }

        }
        public static State NoTransition()
        {
            return null;
        }
    }

}