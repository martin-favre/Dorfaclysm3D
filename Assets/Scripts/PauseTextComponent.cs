using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PauseTextComponent : MonoBehaviour
{
    TMP_Text text;
    Logging.LilLogger logger;

    SimpleObserver<bool> pauseObserver;
    void Start()
    {
        logger = new Logging.LilLogger("PauseTextComponent");
        text = GetComponent<TMP_Text>();
        if (text != null)
        {
            text.enabled = false;
        }
        else
        {
            logger.Log("No text found on PauseTextComponent", Logging.LogLevel.Warning);
        }
        pauseObserver = new SimpleObserver<bool>(PauseManager.Instance, (paused) =>
        {
            text.enabled = paused;
        });
    }
}
