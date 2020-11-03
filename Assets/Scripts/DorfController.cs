using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Logging;
using StateMachineCollection;
using UnityEngine;

public class DorfController : MonoBehaviour, ISaveableComponent
{
    [System.Serializable]
    private class SaveData : GenericSaveData<DorfController>
    {
        public IGenericSaveData currentStateSave;
        public string name;
    }

    GridActor gridActor;
    const string prefabName = "Prefabs/Dorf";
    static GameObject prefabObj;
    StateMachine stateMachine;
    LilLogger logger;

    private void Start()
    {
        if (logger == null) logger = new LilLogger(gameObject.name);
        logger.Log("I started");
    }


    public static DorfController InstantiateDorf(Vector3Int spawnPos)
    {
        if (prefabObj == null)
        {
            prefabObj = Resources.Load(prefabName) as GameObject;
            if (!prefabObj) throw new System.Exception("Could not load prefab " + prefabName);
        }
        GameObject obj = Instantiate(prefabObj) as GameObject;
        obj.name = "Dorf_" + obj.GetInstanceID().ToString();
        if (!obj) throw new System.Exception("Could not instantiate prefab " + prefabName);
        DorfController dorf = obj.GetComponent<DorfController>();
        if (!dorf) throw new System.Exception("No DorfController Component on " + prefabName);
        dorf.gridActor = dorf.GetComponent<GridActor>();
        if (!dorf.gridActor) throw new System.Exception("No GridActor on prefab " + prefabName);
        dorf.gridActor.Move(spawnPos);
        dorf.logger = new LilLogger(obj.name);
        dorf.stateMachine = new StateMachine(new ChoosingJobState(dorf.gridActor, dorf.logger));
        return dorf;
    }

    void Update()
    {
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
        save.name = gameObject.name;
        return save;
    }

    public void Load(IGenericSaveData data)
    {
        SaveData save = (SaveData)data;
        gameObject.name = save.name;
        if (logger == null) logger = new LilLogger(gameObject.name);
        logger.Log("I'm being loaded");
        gridActor = GetComponent<GridActor>();
        this.stateMachine = new StateMachine(LoadState(save.currentStateSave));
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

    private class ChoosingJobState : State
    {
        [System.Serializable]
        private class SaveData : GenericSaveData<ChoosingJobState>
        {
            public IGenericSaveData parent;
        }

        GridActor actor;
        private readonly LilLogger logger;

        public ChoosingJobState(GridActor actor, LilLogger logger)
        {
            this.actor = actor;
            this.logger = logger;
        }

        public ChoosingJobState(GridActor actor, IGenericSaveData save, LilLogger logger) : base(((SaveData)save).parent)
        {
            this.actor = actor;
            this.logger = logger;
        }
        public override IGenericSaveData GetSave()
        {
            logger.Log("Saving my ChoosingJobState");
            SaveData save = new SaveData();
            save.parent = base.GetSave();
            return save;
        }


        public override State OnDuring()
        {
            logger.Log("Picking a job");
            IJob job;
            if (MoveItemRequestPool.Instance.HasRequests())
            {
                logger.Log("Got a MoveItemJob!");
                job = new MoveItemJob(actor, MoveItemRequestPool.Instance.GetRequest(actor), logger);
            }
            else if (MiningRequestPool.Instance.HasRequests())
            {
                logger.Log("Got a MiningJob!");
                job = new MiningJob(actor, MiningRequestPool.Instance.GetRequest(actor), logger);
            }
            else
            {
                logger.Log("Found no job, will walk randomly");
                job = new WalkRandomlyJob(actor, logger);
            }
            return new DoJobState(job, actor, logger);
        }
    }

    private class DoJobState : State
    {
        [System.Serializable]
        private class SaveData : GenericSaveData<DoJobState>
        {
            public IGenericSaveData parent;
            public IGenericSaveData workSave;
        }
        GridActor actor;
        private readonly LilLogger logger;
        IJob work;

        public DoJobState(GridActor actor, IGenericSaveData save, LilLogger logger) : base(((SaveData)save).parent)
        {
            this.actor = actor;
            this.logger = logger;
            this.work = LoadWork(((SaveData)save).workSave);
            logger.Log("Loading my DoJobState");
        }

        private IJob LoadWork(IGenericSaveData workSave)
        {
            Type type = workSave.GetSaveType();
            logger.Log("Loading work");
            if (type == typeof(WalkRandomlyJob))
            {
                logger.Log("Apparently I was WalkingRandomly");
                return new WalkRandomlyJob(this.actor, workSave, logger);
            }
            else if (type == typeof(MiningJob))
            {
                logger.Log("Apparently I was Mining");
                return new MiningJob(this.actor, workSave, logger);
            }
            else if (type == typeof(MoveItemJob))
            {
                logger.Log("Apparently I was Moving an item");
                return new MoveItemJob(this.actor, workSave, logger);
            }
            else
            {
                throw new Exception("Unknown type " + type.ToString());
            }

        }

        public DoJobState(IJob work, GridActor actor, LilLogger logger)
        {
            this.actor = actor;
            this.logger = logger;
            this.work = work;
            logger.Log("Initializing my DoJobState");
        }
        public override IGenericSaveData GetSave()
        {
            logger.Log("Saving my DoJobState");
            SaveData save = new SaveData();
            save.parent = base.GetSave();
            save.workSave = this.work.GetSave();
            return save;
        }
        public override State OnDuring()
        {
            if (work.Work())
            {
                logger.Log("Finished my work");
                return new ChoosingJobState(actor, logger);
            }
            return StateMachine.NoTransition();
        }
    }
}
