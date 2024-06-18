using System;
using UnityEngine;

/// <summary> [Two Click Tools Timer Utility] Timer t = new Timer(optional string: "Timing a complex loop");
/// <para>( Timing starts on new Timer() )"</para>
///...
///<para>t.Lap("test"): time elapsed so far, will print:  "test 50ms"</para>
///<para>At the end use:  t.End(); This will print:  "Timing a complex loop: 123ms"</para>
/// </summary>


public class Timer
{
    const int kMaxLapTimes = 100;
    
    public static bool globalTimerDisplayEnabled = true;
    private DateTime startTime;
    private float timeDelta = 0, timeAtLastLap = 0;
    private float[] lapTimes = new float[kMaxLapTimes];
    private string[] lapStrings = new string[kMaxLapTimes];
    private int numLapTimes = 0;
    private string description;
    public bool enabled = true;

    public Timer(string str = "", bool enabled = true)
    {
        Start();
        description = str;
        this.enabled = enabled;
    }

    public void Start()
    {
        startTime = System.DateTime.Now;
    }

    public float End(bool print = true, float printIfmsMoreThan = 0)
    {
        timeDelta = (float)System.DateTime.Now.Subtract(startTime).TotalMilliseconds;
        if (print == true && enabled && globalTimerDisplayEnabled)
        {
            description += " : ";
            for (int i = 0; i < numLapTimes; i++)
            {
                description += " (" + lapStrings[i] + " " + lapTimes[i].ToString("F2") + ") ";
            }
            if (timeDelta > printIfmsMoreThan)
                Debug.Log(description + timeDelta.ToString("F2") + "ms\n");
        }
        return timeDelta;
    }

    public float Lap(string lapStr = "")
    {
        ClearLapArrays();

        timeDelta = (float)System.DateTime.Now.Subtract(startTime).TotalMilliseconds;
        float lapTime = timeDelta - timeAtLastLap;
        timeAtLastLap = timeDelta;
        lapStrings[numLapTimes] = lapStr + ": ";
        lapTimes[numLapTimes++] = lapTime;
        return lapTime;
    }


    public void Reset()
    {
        Start();
    }
    /// <summary> Gets time in ms </summary>

    public float GetTime()
    {
        timeDelta = (float)System.DateTime.Now.Subtract(startTime).TotalMilliseconds;
        return timeDelta;
    }
    private void ClearLapArrays()
    {
        if (numLapTimes >= kMaxLapTimes)
        {
            Array.Clear(lapTimes, 0, lapTimes.Length);

            for (int i = 0; i < lapStrings.Length; i++)
            {
                lapStrings[i] = string.Empty;
            }
        }
        numLapTimes = 0;
    }
}