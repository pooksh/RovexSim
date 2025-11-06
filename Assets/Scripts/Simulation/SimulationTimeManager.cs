using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages simulation time progression and timed events.
/// Supports adjustable time rate, pausing, and event scheduling.
/// </summary>
public class SimulationTimeManager : MonoBehaviour
{
    public static SimulationTimeManager Instance { get; private set; }

    [Header("Simulation Time Settings")]
    [Tooltip("Simulation speed multiplier (1x = real time).")]
    [Range(0f, 20f)]
    public float TimeScale = 1f;

    [Tooltip("Start simulation automatically on play.")]
    public bool AutoStart = true;

    public float CurrentTime { get; private set; } = 0f;
    public bool IsRunning { get; private set; } = false;

    private List<TimedEvent> _eventQueue = new List<TimedEvent>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (AutoStart)
            StartSimulation();
    }

    void Update()
    {
        if (!IsRunning) return;

        float delta = UnityEngine.Time.deltaTime * TimeScale;
        CurrentTime += delta;

        // Process any events scheduled for <= current time
        for (int i = _eventQueue.Count - 1; i >= 0; i--)
        {
            if (CurrentTime >= _eventQueue[i].TriggerTime)
            {
                _eventQueue[i].Action?.Invoke();
                _eventQueue.RemoveAt(i);
            }
        }
    }

    public void StartSimulation()
    {
        IsRunning = true;
        CurrentTime = 0f;
    }

    public void Pause()
    {
        IsRunning = false;
    }

    public void Resume()
    {
        IsRunning = true;
    }

    public void Stop()
    {
        IsRunning = false;
        CurrentTime = 0f;
        _eventQueue.Clear();
    }

    /// <summary>
    /// Schedules a one-time event at a specific simulation timestamp (in seconds).
    /// </summary>
    public void ScheduleEvent(float triggerTime, Action action)
    {
        if (action == null) return;
        _eventQueue.Add(new TimedEvent(triggerTime, action));
        _eventQueue.Sort((a, b) => a.TriggerTime.CompareTo(b.TriggerTime));
    }

    /// <summary>
    /// Example helper for debugging or manual events.
    /// </summary>
    public void ScheduleEventAfterDelay(float delay, Action action)
    {
        ScheduleEvent(CurrentTime + delay, action);
    }

    private class TimedEvent
    {
        public float TriggerTime;
        public Action Action;
        public TimedEvent(float t, Action a)
        {
            TriggerTime = t;
            Action = a;
        }
    }
}
