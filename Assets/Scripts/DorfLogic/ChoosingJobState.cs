using Logging;
using StateMachineCollection;

class ChoosingJobState : State
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
