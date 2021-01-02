using System;
using System.Collections;
using System.Collections.Generic;
using Logging;
using UnityEngine;

public class PauseManager : MonoBehaviour, IObservable<bool>
{
    static bool isPaused = false;

    List<IObserver<bool>> observers = new List<IObserver<bool>>();

    public static bool IsPaused { get => isPaused; }
    public static PauseManager Instance { get => instance; }

    static PauseManager instance;

    LilLogger logger;

    public IDisposable Subscribe(IObserver<bool> observer)
    {
        return new GenericUnsubscriber<bool>(observers, observer);
    }

    private void Awake()
    {
        logger = new LilLogger("PauseManager");
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            logger.Log("PauseManager was not null, is this a second instance?", Logging.LogLevel.Warning);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) // TODO change to space when on windows
        {
            if (IsPaused)
            {
                UnpauseGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void UpdateObservers()
    {
        foreach (var observer in observers)
        {
            observer.OnNext(IsPaused);
        }

    }

    public void PauseGame()
    {
        if (isPaused != true)
        {
            isPaused = true;
            UpdateObservers();
        }
    }

    public void UnpauseGame()
    {
        if (isPaused != false)
        {
            isPaused = false;
            UpdateObservers();
        }

    }

    void OnDestroy()
    {
        // OnCompleted modifies the observers collection
        // clone it to avoid problems
        IObserver<bool>[] tmpObservers = observers.ToArray();
        foreach (var observer in tmpObservers)
        {
            observer.OnCompleted();
        }
    }
}
