using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiHandler : MonoBehaviour
{
    // Start is called before the first frame update
    static UiHandler instance;
    Action<PlayerComponent.RequestState> requestCallback;

    public static UiHandler Instance { get => instance; }

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
    }

    public void SubscribeToRequestTypeChanges(Action<PlayerComponent.RequestState> callback)
    {
        requestCallback = callback;
    }
    public void UnsubscribeToRequestTypeChanges(Action<PlayerComponent.RequestState> callback)
    {
        requestCallback = null;
    }


    public void OnRequestTypeChanged(int state)
    {
        PlayerComponent.RequestState request = (PlayerComponent.RequestState)state;
        if (requestCallback != null)
        {
            requestCallback(request);
        }
    }
}
