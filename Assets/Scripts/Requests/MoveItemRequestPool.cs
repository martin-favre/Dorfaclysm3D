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
