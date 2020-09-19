using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using StateMachineCollection;
using UnityEngine;

public class DorfController : MonoBehaviour
{
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
        dorf.gridActor = new GridActor(obj, spawnPos);
        return dorf;
    }

    void Start()
    {
        stateMachine = new StateMachine(new ChoosingJobState(gridActor));
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
        transform.position = gridActor.GetPos() + new Vector3(.5f, 1, .5f);
    }

    private class ChoosingJobState : State
    {
        GridActor mActor;
        public ChoosingJobState(GridActor actor)
        {
            mActor = actor;
        }
        public override State OnDuring()
        {
            return new DoJobState(new WalkRandomlyJob(mActor), mActor);
        }
    }

    private class DoJobState : State
    {
        GridActor mActor;
        IJob mWork;
        public DoJobState(IJob work, GridActor actor)
        {
            mActor = actor;
            mWork = work;
        }
        public override State OnDuring()
        {
            if(mWork.Work()){
                return new ChoosingJobState(mActor);
            }
            return StateMachine.NoTransition();
        }
    }
}
