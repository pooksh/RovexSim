using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSimulation : MonoBehaviour
{
    [SerializeField] private bool enableDebugLogs = true;

    public float simulationDuration = 180f;
    public int minutesInDay = 1440;
    public int clockTick = 5;

    private float simulationTick;
    private float incrementThreshold;
    private float timer = 0.0f;
    private TimeOfDay currentTime = new TimeOfDay(0, 0);
    private bool simulationComplete = false;

    private TaskManager taskmgr;

    float Convert24HourTimeToSimulationTime(TimeOfDay time)
    {
        float totalMins = (float) time.TotalMinutesSinceDawn();
        return simulationDuration * (totalMins / (float) minutesInDay);
    }

    void Start()
    {
        // calculate how many game seconds is 5 fake minutes based on simulation duration
        simulationTick = simulationDuration * ((float)clockTick/(float)minutesInDay);
        incrementThreshold = simulationTick;
        taskmgr = GetComponent<TaskManager>();
        if (taskmgr == null)
        {
            taskmgr = gameObject.AddComponent<TaskManager>();
            if (enableDebugLogs)
                Debug.Log($"Added TaskManager to {gameObject.name}");
                // todo: test this
        }
    }

    void Update() // TODO: needs time warping / pausing / restarting functionality
    {           
        if (timer >= incrementThreshold && !simulationComplete) {
            incrementThreshold += simulationTick;
            currentTime.IncrementTimeByMinutues(clockTick);
            Debug.Log("The time is " + currentTime.StringTime() + " and " + currentTime.StringTimeAMPM() + " using the 12 hour AM/PM clock.");
            taskmgr.UpdateManager(currentTime);
        }

        if (timer >= simulationDuration && !simulationComplete) {
            simulationComplete = true;
        }

        timer += Time.deltaTime;

    }



}
