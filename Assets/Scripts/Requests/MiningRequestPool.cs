using UnityEngine;

public class MiningRequestPool : RequestPool<MiningRequest>
{
    readonly static MiningRequestPool instance;

    public static MiningRequestPool Instance => instance;


    static MiningRequestPool()
    {
        instance = new MiningRequestPool();
    }
    public override MiningRequest GetRequest(GridActor actor)
    {
        // Get the closest request
        Vector3Int actorPos = actor.GetPos();
        int smallestDistance = int.MaxValue;
        MiningRequest closestReq = null;
        lock (lockObject)
        {
            foreach (MiningRequest req in requests)
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
