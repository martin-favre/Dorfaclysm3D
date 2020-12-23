using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class PausableTimerTests
    {
        int SToMs(double seconds) { return Mathf.RoundToInt((float)seconds * 1000); }

        [Test]
        public void TimerShouldWait()
        {
            int waitTimeMs = 100;
            bool elapsed = false;
            PausableTimer timer = new PausableTimer(waitTimeMs);
            timer.Elapsed += (a, b) =>
            {
                elapsed = true;
            };
            int margin = Mathf.RoundToInt(waitTimeMs * 0.1f);
            timer.Start();
            System.Threading.Thread.Sleep(waitTimeMs - margin);
            Assert.IsFalse(elapsed);
            System.Threading.Thread.Sleep(2 * margin); // is now time+margin
            Assert.IsTrue(elapsed);
        }

        [Test]
        public void TimerShouldBePausable()
        {
            int waitTimeMs = 100;
            bool elapsed = false;
            PausableTimer timer = new PausableTimer(waitTimeMs);
            timer.Elapsed += (a, b) =>
            {
                elapsed = true;
            };
            int margin = Mathf.RoundToInt(waitTimeMs * 0.1f);
            timer.Start();
            timer.Pause();

            System.Threading.Thread.Sleep(waitTimeMs + margin);
            Assert.IsFalse(elapsed);

            timer.Resume();
            System.Threading.Thread.Sleep(waitTimeMs);
            Assert.IsFalse(elapsed);
            System.Threading.Thread.Sleep(2 * margin); // is now time+margin
            Assert.IsTrue(elapsed);
        }

        [Test]
        public void TimerShouldBeSaveAndLoadable()
        {
            int waitTimeMs = 100;
            bool elapsed = false;
            PausableTimer timer = new PausableTimer(waitTimeMs);
            timer.Elapsed += (a, b) =>
            {
                elapsed = true;
            };
            int margin = Mathf.RoundToInt(waitTimeMs * 0.1f);
            timer.Start();



            // Wait half the time, then save
            System.Threading.Thread.Sleep((waitTimeMs + margin) / 2);
            IGenericSaveData save = timer.GetSave();
            Assert.IsFalse(elapsed);

            timer = new PausableTimer(save);
            timer.Elapsed += (a, b) =>
            {
                elapsed = true;
            };
            System.Threading.Thread.Sleep((waitTimeMs + margin) / 2);
            Assert.IsTrue(elapsed);
        }

        [Test]
        public void PausedTimerShouldBeSaveAndLoadable()
        {
            int waitTimeMs = 100;
            bool elapsed = false;
            PausableTimer timer = new PausableTimer(waitTimeMs);
            timer.Elapsed += (a, b) =>
            {
                elapsed = true;
            };
            int margin = Mathf.RoundToInt(waitTimeMs * 0.1f);
            timer.Start();



            // Wait half the time, then save
            System.Threading.Thread.Sleep((waitTimeMs + margin) / 2);
            timer.Pause();
            IGenericSaveData save = timer.GetSave();
            Assert.IsFalse(elapsed);

            timer = new PausableTimer(save);
            timer.Elapsed += (a, b) =>
            {
                elapsed = true;
            };
            System.Threading.Thread.Sleep((waitTimeMs + margin) / 2);
            Assert.IsFalse(elapsed);

            timer.Resume();
            System.Threading.Thread.Sleep((waitTimeMs + margin) / 2);
            Assert.IsTrue(elapsed);
        }


    }
}
