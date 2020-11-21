using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logging
{

    public class LilLogger
    {
        readonly string name;
        public LilLogger(string name)
        {
            this.name = name;
        }

        public void Log(string message, LogLevel level)
        {
            // If it's important, show it in the unity editor
            if (level == LogLevel.Error)
            {
                Debug.LogError(message);
            }
            else if (level == LogLevel.Warning)
            {
                Debug.LogWarning(message);
            }

            // Then send it to be written to file
            LogManager.Log(new LogPackage(name, message, level));
        }
        public void Log(string message)
        {
            LogManager.Log(new LogPackage(name, message, LogLevel.Info));
        }

    }

}