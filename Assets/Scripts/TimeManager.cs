using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private float simulationDuration = 180f;
    [SerializeField] private int minutesInDay = 1440;
    [SerializeField] private int clockTick = 5;
    [SerializeField] TMP_Text timeTextField;

    private float simulationTick;
    private float incrementThreshold;
    private float timer = 0.0f;
    private TimeOfDay currentTime = new TimeOfDay(0, 0);
    private bool simulationComplete = false;
    private bool taskManagerActive = true;
    private bool ShowCurrentTime = true;
    private bool endCalled = false;

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
            if (enableDebugLogs) {
                Debug.LogWarning("Task manager is not currently active.");
            }
            taskManagerActive = false;   
        }

        if (ShowCurrentTime && timeTextField == null) {
            Debug.LogError($"Either disable time tracking or input an object with a text component.");
        }
        else if (ShowCurrentTime && timeTextField != null) {
            timeTextField.text = $"{currentTime.StringTime()}";
        }
    }

    void Update() // TODO: needs time warping / pausing / restarting functionality
    {           
        if (timer >= incrementThreshold && !simulationComplete) {
            if (taskManagerActive) {

                taskmgr.UpdateManager(currentTime);
            }
            if (ShowCurrentTime) {
                timeTextField.text = $"{currentTime.StringTime()}";
            }
            if (enableDebugLogs) {
                // Debug.Log("The time is " + currentTime.StringTime() + " and " + currentTime.StringTimeAMPM() + " using the 12 hour AM/PM clock.");
                Debug.Log("Tick called at " + currentTime.StringTime());
            }
            incrementThreshold += simulationTick;
            currentTime.IncrementTimeByMinutues(clockTick);
        }

        if (timer >= simulationDuration && !simulationComplete) {
            simulationComplete = true;
        }
        if (timer >= simulationDuration && simulationComplete && !endCalled) {
            endCalled = true;
            taskmgr.FinishSimulation();
        }

        timer += Time.deltaTime;

    }

    public TimeOfDay GetTimeNow() {
        return currentTime;
    }

}
