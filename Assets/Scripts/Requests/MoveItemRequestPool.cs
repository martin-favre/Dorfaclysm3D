using UnityEngine;

public class MoveItemRequestPool : RequestPool<MoveItemRequest>
{
    readonly static MoveItemRequestPool instance;

    public static MoveItemRequestPool Instance => instance;


    static MoveItemRequestPool() {
        instance = new MoveItemRequestPool();
    }
    public override MoveItemRequest GetRequest(GridActor actor)
    {
        // just get first best
        MoveItemRequest request = null;
        foreach (MoveItemRequest req in requests)
        {
            request = req;
            break;
        }

        HandOutRequest(request);
        return request;
    }

}
