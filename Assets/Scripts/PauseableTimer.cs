using System.Diagnostics;
using System.Timers;

// thanks https://stackoverflow.com/questions/26964192/c-sharp-how-to-pause-a-timer

public class PausableTimer : Timer
{
    [System.Serializable]
    private class SaveData : GenericSaveData<PausableTimer>
    {
        public double initialInterval;
        public bool resumed;
        public double remainingAfterPause;

        public bool wasPaused;

    }

    SaveData data = new SaveData();
    private readonly Stopwatch stopwatch = new Stopwatch();

    public PausableTimer(double interval) : base(interval)
    {
        data.initialInterval = interval;
        Elapsed += OnElapsed;
    }

    public PausableTimer(IGenericSaveData save)
    {
        data = (SaveData)save;
        Elapsed += OnElapsed;
        bool wasPaused = data.wasPaused;
        Resume();
        if (wasPaused)
        {
            Pause();
        }
    }

    public new void Start()
    {
        ResetStopwatch();
        base.Start();
    }

    private void OnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
    {
        if (data.resumed)
        {
            data.resumed = false;
            Stop();
            Interval = data.initialInterval;
            Start();
        }

        ResetStopwatch();
    }

    private void ResetStopwatch()
    {
        stopwatch.Reset();
        stopwatch.Start();
    }

    public void Pause()
    {
        data.wasPaused = true;
        Stop();
        stopwatch.Stop();
        data.remainingAfterPause = Interval - stopwatch.Elapsed.TotalMilliseconds;
    }

    public void Resume()
    {
        data.wasPaused = false;
        data.resumed = true;
        Interval = data.remainingAfterPause;
        data.remainingAfterPause = 0;
        Start();
    }

    public IGenericSaveData GetSave()
    {
        bool wasPaused = data.wasPaused;
        Pause();
        data.wasPaused = wasPaused;
        return data;
    }

}