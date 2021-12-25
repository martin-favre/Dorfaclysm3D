using System;
using Logging;
using StateMachineCollection;

class DoJobState : State
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
