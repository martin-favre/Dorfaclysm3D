
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Items;
using StateMachineCollection;
using UnityEngine;

public class MoveItemJob : IJob
{
    [System.Serializable]
    private class SaveData : GenericSaveData<MoveItemJob>
    {
        public IGenericSaveData activeState;
    }

    StateMachine machine;

    public MoveItemJob(GridActor actor, MoveItemRequest request)
    {
        Debug.Log("Started a MoveItemJob");
        machine = new StateMachine(new FindItemState(actor, request));
    }

    public MoveItemJob(GridActor user, IGenericSaveData save)
    {
        SaveData saveData = (SaveData)save;
        if (saveData.activeState != null)
        {
            machine = new StateMachine(LoadState(user, saveData.activeState));
        }
    }

    private State LoadState(GridActor user, IGenericSaveData activeState)
    {
        Type type = activeState.GetSaveType();
        if (type == typeof(FindItemState))
        {
            return new FindItemState(user, activeState);
        }
        else if (type == typeof(WalkToItemState))
        {
            return new WalkToItemState(user, activeState);
        }
        else if (type == typeof(WalkToTargetState))
        {
            return new WalkToTargetState(user, activeState);
        }
        else
        {
            throw new Exception("Unknown type " + type.ToString());
        }
    }

    public IGenericSaveData GetSave()
    {
        return new SaveData() { activeState = machine.GetSave() };
    }

    public bool Work()
    {
        machine.Update();
        return machine.IsTerminated();
    }

    private class FindItemState : State
    {
        [System.Serializable]
        private class SaveData : GenericSaveData<FindItemState>
        {
            public IGenericSaveData parent;
            public MoveItemRequest request;
        }

        Task<Vector3Int> findItemTask;
        private readonly GridActor actor;
        private readonly MoveItemRequest request;

        public FindItemState(GridActor actor, MoveItemRequest request)
        {
            this.actor = actor;
            this.request = request;
        }

        public FindItemState(GridActor actor, IGenericSaveData saveData) : base(((SaveData)saveData).parent)
        {
            this.actor = actor;
            SaveData save = saveData as SaveData;
            this.request = save.request;
        }

        public override IGenericSaveData GetSave()
        {
            return new SaveData() { parent = base.GetSave(), request = this.request };
        }
        public override void OnEntry()
        {
            this.findItemTask = Task.Run(() => FindItem(actor.GetPos(), request.TypeToFind));
        }
        public override State OnDuring()
        {
            if (findItemTask.IsCompleted)
            {
                if (findItemTask.Result != actor.GetPos())
                {
                    return new WalkToItemState(actor, request, findItemTask.Result);
                }
                else
                {
                    Debug.Log("MoveItemJob, FindItemState, Could not find item");
                    MoveItemRequestPool.Instance.ReturnRequest(request);
                    TerminateMachine();
                }
            }
            return StateMachine.NoTransition();
        }

        private Task<Vector3Int> FindItem(Vector3Int origin, ItemType itemToFind)
        {
            // breadth first search for the itemtype
            HashSet<Vector3Int> failedPositions = new HashSet<Vector3Int>();
            Queue<Vector3Int> testPositions = new Queue<Vector3Int>();
            testPositions.Enqueue(origin);
            while (testPositions.Count > 0)
            {
                Vector3Int current = testPositions.Dequeue();
                failedPositions.Add(current);
                InventoryComponent[] inventories = ItemMap.GetInventories(current);
                foreach (InventoryComponent inventory in inventories)
                {
                    if (inventory.HasItem(itemToFind))
                    {
                        return Task.FromResult(current);
                    }
                }

                foreach (Vector3Int delta in DeltaPositions.DeltaPositions3D)
                {
                    Vector3Int nextPos = current + delta;
                    if (Astar.IsStepValid(nextPos, current, delta) && !failedPositions.Contains(nextPos))
                    {
                        testPositions.Enqueue(nextPos);
                    }
                }
            }
            return Task.FromResult(origin); // Return originPos if fail. The next step should fail fast.
        }
    }

    private class WalkToItemState : WalkingState
    {

        [System.Serializable]
        private class SaveData : GenericSaveData<WalkToItemState>
        {
            public IGenericSaveData parent;
            public MoveItemRequest request;
            public Vector3Int target;
        }

        private readonly GridActor actor;
        private readonly MoveItemRequest request;
        private readonly Vector3Int target;

        public WalkToItemState(GridActor actor, MoveItemRequest request, Vector3Int target) : base(actor, 0.3f)
        {
            this.actor = actor;
            this.request = request;
            this.target = target;
        }

        public WalkToItemState(GridActor actor, IGenericSaveData saveData) : base(actor, ((SaveData)saveData).parent)
        {
            this.actor = actor;
            SaveData save = saveData as SaveData;
            this.request = save.request;
            this.target = save.target;
        }

        public override IGenericSaveData GetSave()
        {
            return new SaveData() { parent = base.GetSave(), request = this.request, target = this.target };
        }

        public override Vector3Int GetTargetPos()
        {
            return target;
        }

        public override State OnPathFindFail()
        {
            TerminateMachine();
            Debug.Log("MoveItemJob, could not find path to item due to " + GetFailReason().ToString());
            MoveItemRequestPool.Instance.ReturnRequest(request);
            return StateMachine.NoTransition();
        }

        public override State OnReachedTarget()
        {
            GridActor[] actors = GridActorMap.GetGridActors(actor.GetPos());
            foreach (GridActor actor in actors)
            {
                InventoryComponent comp = actor.GetComponent<InventoryComponent>();
                if (comp && comp.HasItem(request.TypeToFind))
                {
                    return new WalkToTargetState(this.actor, request, comp.GetItem(request.TypeToFind));
                }
            }
            Debug.Log("MoveItemJob, no item at target");
            return OnPathFindFail();
        }
    }

    private class WalkToTargetState : WalkingState
    {
        [System.Serializable]
        private class SaveData : GenericSaveData<WalkToTargetState>
        {
            public IGenericSaveData parent;
            public MoveItemRequest request;
            public Item item;
        }

        private readonly GridActor actor;
        private readonly MoveItemRequest request;
        private readonly Item item;

        public WalkToTargetState(GridActor actor, MoveItemRequest request, Item item) : base(actor, 0.3f)
        {
            this.actor = actor;
            this.request = request;
            this.item = item;
        }

        public WalkToTargetState(GridActor actor, IGenericSaveData saveData) : base(actor, ((SaveData)saveData).parent)
        {
            this.actor = actor;
            SaveData save = saveData as SaveData;
            this.request = save.request;
            this.item = save.item;
        }

        public override IGenericSaveData GetSave()
        {
            return new SaveData() { parent = base.GetSave(), request = this.request, item = this.item };
        }

        public override Vector3Int GetTargetPos()
        {
            return request.PositionToMoveTo;
        }

        public override State OnPathFindFail()
        {
            Debug.Log("MoveItemJob, WalkToTargetState, OnPathFindFail " + GetFailReason().ToString());
            TerminateMachine();
            GridMap.PutItem(actor.GetPos(), item);
            MoveItemRequestPool.Instance.ReturnRequest(request);
            return StateMachine.NoTransition();
        }

        public override State OnReachedTarget()
        {
            GridActor[] actors = GridActorMap.GetGridActors(this.request.PositionToMoveTo);
            foreach (GridActor actor in actors)
            {
                BlockBuildingSite comp = actor.gameObject.GetComponent<BlockBuildingSite>();
                if (comp)
                {
                    comp.GetComponent<InventoryComponent>().AddItem(item);
                    MoveItemRequestPool.Instance.FinishRequest(request);
                    TerminateMachine();
                    return StateMachine.NoTransition();
                }
            }
            GridMap.PutItem(this.request.PositionToMoveTo, item);
            return OnPathFindFail();
        }
    }
}
