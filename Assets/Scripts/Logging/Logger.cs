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
            LogManager.Log(new LogPackage(name, message, level));
        }
        public void Log(string message)
        {
            LogManager.Log(new LogPackage(name, message, LogLevel.Info));
        }

    }

}