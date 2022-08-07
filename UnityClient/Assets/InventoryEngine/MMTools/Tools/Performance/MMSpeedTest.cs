using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

namespace MoreMountains.Tools
{
    /// <summary>
    /// A struct to store data associated to speed tests
    /// </summary>
    public struct MMSpeedTestItem
    {
        /// the name of the test, has to be unique
        public string TestID;
        /// a stopwatch to compute time
        public Stopwatch Timer;
        /// <summary>
        /// Creates a speed test with the specified ID and starts the timer
        /// </summary>
        /// <param name="testID"></param>
        public MMSpeedTestItem(string testID)
        {
            TestID = testID;
            Timer = Stopwatch.StartNew();
        }
    }

    /// <summary>
    /// Use this class to run performance tests in your code. 
    /// It'll output the time spent between the StartTest and the EndTest calls
    /// Make sure to use a unique ID for both calls
    /// </summary>
    public static class MMSpeedTest 
    {
        private static readonly Dictionary<string, MMSpeedTestItem> _speedTests = new Dictionary<string, MMSpeedTestItem>();

        /// <summary>
        /// Starts a speed test of the specified ID
        /// </summary>
        /// <param name="testID"></param>
        public static void StartTest(string testID)
        {
            if (_speedTests.ContainsKey(testID))
            {
                _speedTests.Remove(testID);
            }

            MMSpeedTestItem item = new MMSpeedTestItem(testID);
            _speedTests.Add(testID, item);
        }

        /// <summary>
        /// Stops a speed test of the specified ID
        /// </summary>
        public static void EndTest(string testID)
        {
            if (!_speedTests.ContainsKey(testID))
            {
                return;
            }

            _speedTests[testID].Timer.Stop();
            float elapsedTime = _speedTests[testID].Timer.ElapsedMilliseconds / 1000f;
            _speedTests.Remove(testID);

            UnityEngine.Debug.Log("<color=red>MMSpeedTest</color> [Test "+testID+"] test duration : "+elapsedTime+"s");
        }        
    }
}
