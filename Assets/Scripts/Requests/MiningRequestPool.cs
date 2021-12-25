using System;
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
        BlockEffectMap.Instance.SetEffect(request.Position, BlockEffects.SelectedBlock);
    }

    protected override void OnRequestRemoved(MiningRequest request)
    {
        BlockEffectMap.Instance.RemoveEffect(request.Position, BlockEffects.SelectedBlock);
    }

    public override MiningRequest GetRequest(GridActor actor)
    {
        // Get the closest request
        ReturnCooledDownTasks(false);
        Vector3Int actorPos = actor.Position;
        int smallestDistance = int.MaxValue;
        MiningRequest closestReq = null;
        lock (lockObject)
        {
            if (Requests.Count == 0) return null;

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

    internal void CancelRequest(Vector3Int blockPos)
    {
        MiningRequest request = null;
        lock (lockObject)
        {

            PerformForEachRequest((req) =>
            {
                if (req.Position == blockPos)
                {
                    request = req;
                }
            });
        }
        if (request != null)
        {
            CancelRequest(request);
        }
    }
}
