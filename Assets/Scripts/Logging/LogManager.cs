using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.IO;
using System;

namespace Logging
{

    public static class LogManager
    {
        static ConcurrentQueue<LogPackage> logPackages = new ConcurrentQueue<LogPackage>();
        static Thread loggingThread;

        readonly static string logFolderPath = Application.persistentDataPath + "/logs/";
        const string logEnding = ".log";
        const string fullLog = "main";

        static Destructor destructor = new Destructor();

        static bool running = false;

        private sealed class Destructor
        {
            ~Destructor()
            {
                running = false;
                loggingThread.Join();
            }
        }

        public static void Log(LogPackage package)
        {
            logPackages.Enqueue(package);
        }

        static void ClearOldLogs()
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(logFolderPath);

            foreach (FileInfo file in di.GetFiles())
            {
                if (file.Extension == logEnding)
                {
                    file.Delete();
                }
            }
        }

        static LogManager()
        {

            if (!Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }
            else
            {
                ClearOldLogs();
            }
            running = true;
            loggingThread = new Thread(LogLoop);
            loggingThread.Start();
        }

        static string GetFullPath(string name)
        {
            return logFolderPath + name + logEnding;
        }

        static string CreateMessage(LogPackage package)
        {
            string timeStamp = DateTime.Now.ToString("HH:mm:ss.ffff");
            return timeStamp + " " + package.Level.ToString() + ": " + package.Message;
        }

        static void LogToFile(string filename, LogPackage package)
        {
            StreamWriter w = File.AppendText(filename);
            w.WriteLine(CreateMessage(package));
            w.Close();
        }

        static void LogLoop()
        {
            while (running || !logPackages.IsEmpty)
            {
                if (!logPackages.IsEmpty)
                {
                    LogPackage package;
                    bool success = logPackages.TryDequeue(out package);
                    if (success)
                    {
                        string fullPath = GetFullPath(package.Key);
                        if (!File.Exists(fullPath))
                        {
                            File.Create(fullPath).Close();
                        }

                        string mainLogPath = GetFullPath(fullLog);
                        if (!File.Exists(mainLogPath))
                        {
                            File.Create(mainLogPath).Close();
                        }

                        LogToFile(mainLogPath, package);
                        LogToFile(fullPath, package);

                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }
    }

}