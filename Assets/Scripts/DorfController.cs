using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using StateMachineCollection;
using UnityEngine;

public class DorfController : MonoBehaviour, ISaveableComponent
{
    [System.Serializable]
    private class SaveData : GenericSaveData<DorfController>
    {
        public IGenericSaveData currentStateSave;
    }

    GridActor gridActor;
    const string prefabName = "Prefabs/Dorf";
    static GameObject prefabObj;
    StateMachine stateMachine;
    private DorfController() { }

    public static DorfController InstantiateDorf(Vector3Int spawnPos)
    {
        if (prefabObj == null)
        {
            prefabObj = Resources.Load(prefabName) as GameObject;
            if (!prefabObj) throw new System.Exception("Could not load prefab " + prefabName);
        }
        GameObject obj = Instantiate(prefabObj) as GameObject;
        if (!obj) throw new System.Exception("Could not instantiate prefab " + prefabName);
        DorfController dorf = obj.GetComponent<DorfController>();
        if (!dorf) throw new System.Exception("No DorfController Component on " + prefabName);
        dorf.gridActor = dorf.GetComponent<GridActor>();
        if (!dorf.gridActor) throw new System.Exception("No GridActor on prefab " + prefabName);
        dorf.gridActor.Move(spawnPos);
        dorf.stateMachine = new StateMachine(new ChoosingJobState(dorf.gridActor));
        return dorf;
    }

    void Start()
    {
    }

    void Update()
    {
        if (!stateMachine.IsTerminated())
        {
            stateMachine.Update();
        }
        else
        {
            Debug.LogError("DorfController without state");
        }

        // Update the visual position
        // transform.position = gridActor.GetPos() + new Vector3(.5f, 1, .5f);
    }

    public IGenericSaveData Save()
    {
        SaveData save = new SaveData();
        save.currentStateSave = stateMachine.GetSave();
        return save;
    }

    public void Load(IGenericSaveData data)
    {
        gridActor = GetComponent<GridActor>();
        SaveData save = (SaveData)data;
        this.stateMachine = new StateMachine(LoadState(save.currentStateSave));
    }

    private State LoadState(IGenericSaveData currentStateSave)
    {
        Type type = currentStateSave.GetSaveType();
        if (type == typeof(ChoosingJobState))
        {
            return new ChoosingJobState(gridActor, currentStateSave);
        }
        else if (type == typeof(DoJobState))
        {
            return new DoJobState(gridActor, currentStateSave);
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
        public ChoosingJobState(GridActor actor)
        {
            this.actor = actor;
        }

        public ChoosingJobState(GridActor actor, IGenericSaveData save) : base(((SaveData)save).parent)
        {
            this.actor = actor;
        }
        public override IGenericSaveData GetSave()
        {
            SaveData save = new SaveData();
            save.parent = base.GetSave();
            return save;
        }


        public override State OnDuring()
        {
            IJob job;
            if (MoveItemRequestPool.Instance.HasRequests())
            {
                job = new MoveItemJob(actor, MoveItemRequestPool.Instance.GetRequest(actor));
            }
            else if (MiningRequestPool.Instance.HasRequests())
            {
                job = new MiningJob(actor, MiningRequestPool.Instance.GetRequest(actor));
            }
            else
            {
                job = new WalkRandomlyJob(actor);
            }
            return new DoJobState(job, actor);
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
        IJob work;

        public DoJobState(GridActor actor, IGenericSaveData save) : base(((SaveData)save).parent)
        {
            this.actor = actor;
            this.work = LoadWork(((SaveData)save).workSave);
        }

        private IJob LoadWork(IGenericSaveData workSave)
        {
            return new WalkRandomlyJob(this.actor, workSave);
        }

        public DoJobState(IJob work, GridActor actor)
        {
            this.actor = actor;
            this.work = work;
        }
        public override IGenericSaveData GetSave()
        {
            SaveData save = new SaveData();
            save.parent = base.GetSave();
            save.workSave = this.work.GetSave();
            return save;
        }
        public override State OnDuring()
        {
            if (work.Work())
            {
                return new ChoosingJobState(actor);
            }
            return StateMachine.NoTransition();
        }
    }
}
