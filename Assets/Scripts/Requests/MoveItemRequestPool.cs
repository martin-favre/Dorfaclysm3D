using UnityEngine;

public class MoveItemRequestPool : RequestPool<MoveItemRequest>
{
    readonly static MoveItemRequestPool instance;

    public static MoveItemRequestPool Instance => instance;


    static MoveItemRequestPool()
    {
        instance = new MoveItemRequestPool();
    }

    public override MoveItemRequest GetRequest(GridActor actor)
    {
        ReturnCooledDownTasks(false);
        // just get first best
        MoveItemRequest request = null;
        lock (lockObject)
        {
            if (Requests.Count == 0) return null;
            foreach (MoveItemRequest req in Requests)
            {
                request = req;
                break;
            }
        }

        HandOutRequest(request);
        return request;
    }

}
