using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Logging;
using StateMachineCollection;
using UnityEngine;

public class DorfController
{
    [System.Serializable]
    private class SaveData : GenericSaveData<DorfController>
    {
        public IGenericSaveData currentStateSave;
        public string name;
    }

    private string name;
    StateMachine stateMachine;
    LilLogger logger;
    GridActor gridActor;

    public DorfController(String name, GridActor actor, LilLogger logger)
    {
        this.name = name;
        this.gridActor = actor;
        this.logger = logger;
        stateMachine = new StateMachine(new ChoosingJobState(gridActor, logger));
    }

    public DorfController(IGenericSaveData data, GridActor actor, LilLogger logger)
    {
        SaveData save = (SaveData)data;
        name = save.name;
        this.logger = logger;
        logger.Log("I'm being loaded");
        this.gridActor = actor;
        this.stateMachine = new StateMachine(LoadState(save.currentStateSave));
    }

    public void Update()
    {
        if (PauseManager.IsPaused) return;
        if (!stateMachine.IsTerminated())
        {
            stateMachine.Update();
        }
        else
        {
            logger.Log("DorfController without state");
        }
    }

    public IGenericSaveData Save()
    {
        logger.Log("I'm being saved");
        SaveData save = new SaveData();
        save.currentStateSave = stateMachine.GetSave();
        save.name = name;
        return save;
    }

    private State LoadState(IGenericSaveData currentStateSave)
    {
        Type type = currentStateSave.GetSaveType();
        if (type == typeof(ChoosingJobState))
        {
            return new ChoosingJobState(gridActor, currentStateSave, logger);
        }
        else if (type == typeof(DoJobState))
        {
            return new DoJobState(gridActor, currentStateSave, logger);
        }
        else
        {
            throw new Exception("Unknown state type " + type.ToString());
        }
    }
}
