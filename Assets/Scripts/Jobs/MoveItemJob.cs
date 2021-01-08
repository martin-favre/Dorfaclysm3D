
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Items;
using Logging;
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
    private readonly LilLogger logger;

    public MoveItemJob(GridActor actor, MoveItemRequest request, LilLogger logger)
    {
        this.logger = logger;
        logger.Log("Started a MoveItemJob");
        machine = new StateMachine(new FindItemState(actor, request, logger));
    }

    public MoveItemJob(GridActor user, IGenericSaveData save, LilLogger logger)
    {
        this.logger = logger;
        logger.Log("Loading a MoveItemJob");
        SaveData saveData = (SaveData)save;
        if (saveData.activeState != null)
        {
            machine = new StateMachine(LoadState(user, saveData.activeState));
        }

    }

    private State LoadState(GridActor user, IGenericSaveData activeState)
    {
        Type type = activeState.GetSaveType();
        logger.Log("Loading my state");
        if (type == typeof(FindItemState))
        {
            logger.Log("I was in a FindItemState");
            return new FindItemState(user, activeState, logger);
        }
        else if (type == typeof(WalkToItemState))
        {
            logger.Log("I was in a WalkToItemState");
            return new WalkToItemState(user, activeState, logger);
        }
        else if (type == typeof(WalkToTargetState))
        {
            logger.Log("I was in a WalkToTargetState");
            return new WalkToTargetState(user, activeState, logger);
        }
        else
        {
            throw new Exception("Unknown type " + type.ToString());
        }
    }

    public IGenericSaveData GetSave()
    {
        logger.Log("Saving my MoveItemJob");
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
        private readonly LilLogger logger;

        public FindItemState(GridActor actor, MoveItemRequest request, LilLogger logger)
        {
            this.actor = actor;
            this.request = request;
            this.logger = logger;
            logger.Log("Initialized my FindItemState with request " + request);
        }

        public FindItemState(GridActor actor, IGenericSaveData saveData, LilLogger logger) : base(((SaveData)saveData).parent)
        {
            this.actor = actor;
            this.logger = logger;
            SaveData save = saveData as SaveData;
            this.request = save.request;
            logger.Log("Loaded my FindItemState with request " + request);
        }

        public override IGenericSaveData GetSave()
        {
            logger.Log("Saved my FindItemState with request " + request);
            return new SaveData() { parent = base.GetSave(), request = this.request };
        }
        public override void OnEntry()
        {
            logger.Log("Trying to find an item...");
            this.findItemTask = Task.Run(() => FindItem(actor.Position, request.TypeToFind));
        }
        public override State OnDuring()
        {
            if (findItemTask.IsCompleted)
            {
                logger.Log("I finished looking for an item");
                if (!findItemTask.IsCanceled)
                {
                    logger.Log("I found an item at " + findItemTask.Result);
                    return new WalkToItemState(actor, request, findItemTask.Result, logger);
                }
                else
                {
                    logger.Log("MoveItemJob, FindItemState, Could not find item");
                    MoveItemRequestPool.Instance.ReturnRequest(request);
                    TerminateMachine();
                }
            }
            return StateMachine.NoTransition();
        }

        private Task<Vector3Int> FindItem(Vector3Int origin, Type itemToFind)
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
            return null;
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
        private readonly LilLogger logger;

        public WalkToItemState(GridActor actor, MoveItemRequest request, Vector3Int target, LilLogger logger) : base(actor, 100)
        {
            this.actor = actor;
            this.request = request;
            this.target = target;
            this.logger = logger;
            logger.Log("Initialized my WalkToItemState with request " + request);
        }

        public WalkToItemState(GridActor actor, IGenericSaveData saveData, LilLogger logger) : base(actor, ((SaveData)saveData).parent)
        {
            this.actor = actor;
            this.logger = logger;
            SaveData save = saveData as SaveData;
            this.request = save.request;
            this.target = save.target;
            logger.Log("Loaded my WalkToItemState with request " + request);
        }

        public override IGenericSaveData GetSave()
        {
            logger.Log("Saving my WalkToItemState");
            return new SaveData() { parent = base.GetSave(), request = this.request, target = this.target };
        }

        public override Vector3Int GetTargetPos()
        {
            return target;
        }

        public override State OnPathFindFail()
        {
            TerminateMachine();
            logger.Log("MoveItemJob, could not find path to item due to " + GetFailReason().ToString());
            MoveItemRequestPool.Instance.ReturnRequest(request);
            return StateMachine.NoTransition();
        }

        public override State OnReachedTarget()
        {
            GridActor[] actors = GridActorMap.GetGridActors(actor.Position);
            foreach (GridActor actor in actors)
            {
                InventoryComponent comp = actor.GetComponent<InventoryComponent>();
                if (comp && comp.HasItem(request.TypeToFind))
                {
                    logger.Log("Found my item at " + actor.Position);
                    return new WalkToTargetState(this.actor, request, comp.GetItem(request.TypeToFind), logger);
                }
            }
            logger.Log("MoveItemJob, no item at target");
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
        private readonly LilLogger logger;

        public WalkToTargetState(GridActor actor, MoveItemRequest request, Item item, LilLogger logger) : base(actor, 100, 1)
        {
            this.actor = actor;
            this.request = request;
            this.item = item;
            this.logger = logger;
            logger.Log("Initialized my WalkToTargetState with request " + request);
        }

        public WalkToTargetState(GridActor actor, IGenericSaveData saveData, LilLogger logger) : base(actor, ((SaveData)saveData).parent)
        {
            this.actor = actor;
            this.logger = logger;
            SaveData save = saveData as SaveData;
            this.request = save.request;
            this.item = save.item;
            logger.Log("Loaded my WalkToTargetState with request " + request);
        }

        public override IGenericSaveData GetSave()
        {
            logger.Log("Saving my WalkToTargetState");
            return new SaveData() { parent = base.GetSave(), request = this.request, item = this.item };
        }

        public override Vector3Int GetTargetPos()
        {
            return request.PositionToMoveTo;
        }

        public override State OnPathFindFail()
        {
            logger.Log("MoveItemJob, WalkToTargetState, OnPathFindFail " + GetFailReason().ToString());
            logger.Log("Dropping my item");
            TerminateMachine();
            GridMap.Instance.PutItem(actor.Position, item);
            MoveItemRequestPool.Instance.ReturnRequest(request);
            return StateMachine.NoTransition();
        }

        public override State OnReachedTarget()
        {
            GridActor[] actors = GridActorMap.GetGridActors(this.request.PositionToMoveTo);
            foreach (GridActor actor in actors)
            {
                if (actor.Guid.Equals(this.request.TargetGuid))
                {
                    logger.Log("Found my target GridActor, giving it my item");

                    InventoryComponent inventory = actor.GetComponent<InventoryComponent>();
                    if (inventory)
                    {
                        inventory.AddItem(item);
                        MoveItemRequestPool.Instance.FinishRequest(request);
                    }
                    else
                    {
                        MoveItemRequestPool.Instance.CancelRequest(request);
                        logger.Log("My target GridActor did not have an inventory", LogLevel.Error);
                    }
                    TerminateMachine();
                    return StateMachine.NoTransition();
                }
            }
            logger.Log("Did not find my target GridActor at the target, dropping my item");
            GridMap.Instance.PutItem(this.request.PositionToMoveTo, item);
            return OnPathFindFail();
        }
    }
}
