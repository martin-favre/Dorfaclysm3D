
public interface ITimerFactory
{
    ITimer GetPausableTimer(double interval);
    ITimer GetPausableTimer(IGenericSaveData save);
}

public class TimerFactory : ITimerFactory
{
    static readonly TimerFactory instance = new TimerFactory();

    public static TimerFactory Instance => instance;

    public ITimer GetPausableTimer(double interval)
    {
        return new PausableTimer(interval);
    }
    public ITimer GetPausableTimer(IGenericSaveData save)
    {
        return new PausableTimer(save);
    }

}