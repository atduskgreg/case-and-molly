// Version 1.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System.Collections.Generic;
using System.Diagnostics;

namespace Exploder
{
    static public class Profiler
    {
        private static readonly Dictionary<string, Stopwatch> timeSegments = new Dictionary<string, Stopwatch>();

        static public void Start(string key)
        {
            Stopwatch timer = null;

            if (timeSegments.TryGetValue(key, out timer))
            {
                timer.Reset();
                timer.Start();
            }
            else
            {
                timer = new Stopwatch();
                timer.Start();
                timeSegments.Add(key, timer);
            }
        }

        static public void End(string key)
        {
            timeSegments[key].Stop();
        }

        static public string[] PrintResults()
        {
            var result = new string[timeSegments.Count];
            var i = 0;

            foreach (var timeSegment in timeSegments)
            {
                result[i++] = timeSegment.Key + " " + timeSegment.Value.ElapsedMilliseconds.ToString() + " [ms]";
            }

            return result;
        }
    }
}
