using UnityEngine;
using System.Collections;

public class FpsDisplay : MonoBehaviour
{
    float deltaTime = 0.0f;
    SimpleValueDisplayer.ValueHook fpsHook;
    SimpleValueDisplayer.ValueHook msPerFrameHook;

    void Start()
    {
        fpsHook = SimpleValueDisplayer.Instance.RegisterValue();
		msPerFrameHook = SimpleValueDisplayer.Instance.RegisterValue();
    }
    void OnDestroy()
    {
        fpsHook.Dispose();
		msPerFrameHook.Dispose();
    }
    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string msString = string.Format("Ms: {0:0.0}", msec);
        string fpsString = string.Format("Fps: {0:0.}", fps);
		msPerFrameHook.UpdateValue(msString);
        fpsHook.UpdateValue(fpsString);
    }
}