using UnityEngine;
using System;
using System.Collections.Generic;

public class SimpleValueDisplayer : MonoBehaviour
{

    public class ValueHook : IDisposable
    {
        string lastValue = "";

        public string LastValue { get => lastValue; }

        public void UpdateValue(string value)
        {
            lastValue = value;
        }

        public void Dispose()
        {
            SimpleValueDisplayer.Instance.UnregisterValue(this);
        }
    }

    static SimpleValueDisplayer instance;
    List<ValueHook> hooks = new List<ValueHook>();
    GUIStyle style = new GUIStyle();
    public static SimpleValueDisplayer Instance { get => instance; }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = Screen.height * 2 / 100;
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
    }

    public ValueHook RegisterValue()
    {
        ValueHook hook = new ValueHook();
        hooks.Add(hook);
        return hook;
    }

    public void UnregisterValue(ValueHook hook) {
        hooks.Remove(hook);
    }
    void OnGUI() {
        int distanceBetweenValues = 10;
        for(int i = 0; i < hooks.Count; i++) {
            Rect rect = new Rect(10 ,distanceBetweenValues*i, Screen.width, Screen.height * 2 / 100);
            GUI.Label(rect, hooks[i].LastValue, style);
        }

    }
}