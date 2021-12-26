public static class SingletonProvider
{
    static T Getter<T>(T obj, T sourceObject)
    {
        if (obj == null) obj = sourceObject;
        return obj;
    }

    private static IRequestPool<MoveItemRequest> moveItemRequestPool;
    public static IRequestPool<MoveItemRequest> MainMoveItemRequestPool
    {
        get => Getter(moveItemRequestPool, MoveItemRequestPool.Instance);
        set => moveItemRequestPool = value;
    }

    private static IRequestPool<MiningRequest> miningRequestPool;
    public static IRequestPool<MiningRequest> MainMiningRequestPool
    {
        get => Getter(miningRequestPool, MiningRequestPool.Instance);
        set => miningRequestPool = value;
    }

    private static IGridMap gridMap;
    public static IGridMap MainGridMap
    {
        get => Getter(gridMap, GridMap.Instance);
        set => gridMap = value;
    }

    private static ITimerFactory timerFactory;
    public static ITimerFactory MainTimerFactory
    {
        get => Getter(timerFactory, TimerFactory.Instance);
        set => timerFactory = value;
    }

}