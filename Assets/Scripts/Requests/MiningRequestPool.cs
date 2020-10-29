using UnityEngine;

public class MiningRequestPool : RequestPool<MiningRequest>
{
    readonly static MiningRequestPool instance;

    public static MiningRequestPool Instance => instance;


    static MiningRequestPool()
    {
        instance = new MiningRequestPool();
    }
    private MiningRequestPool() { }

    protected override void OnRequestAdded(MiningRequest request)
    {
        BlockEffectMap.SetEffect(request.Position, BlockEffects.SelectedBlock);
    }

    protected override void OnRequestRemoved(MiningRequest request)
    {
        BlockEffectMap.RemoveEffect(request.Position, BlockEffects.SelectedBlock);
    }

    public override MiningRequest GetRequest(GridActor actor)
    {
        // Get the closest request
        ReturnCooledDownTasks(false);
        Vector3Int actorPos = actor.GetPos();
        int smallestDistance = int.MaxValue;
        MiningRequest closestReq = null;
        lock (lockObject)
        {
            foreach (MiningRequest req in Requests)
            {
                Vector3Int diff = req.Position - actorPos;
                int distance = diff.sqrMagnitude;
                if (distance < smallestDistance)
                {
                    closestReq = req;
                    smallestDistance = distance;
                }
            }
        }
        HandOutRequest(closestReq);
        return closestReq;
    }
}
