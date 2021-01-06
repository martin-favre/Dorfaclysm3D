public class RequestPoolUpdateEvent <T> where T : PlayerRequest
{
    public enum EventType {
        Returned,
        Finished,
        Cancelled
    }
    readonly T request;
    readonly EventType type;

    public T Request => request;

    public EventType Type => type;

    public RequestPoolUpdateEvent(T request, EventType type)
    {
        this.request = request;
        this.type = type;
    }

    
}