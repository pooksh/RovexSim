using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages multiple AGVs within the simulation.
/// Integrates with SimulationTimeManager to schedule and coordinate tasks over time.
/// </summary>
public class AGVManager : MonoBehaviour
{
    [Header("AGV Settings")]
    [Tooltip("AGV prefab to spawn.")]
    public GameObject AGVPrefab;

    [Tooltip("Maximum number of AGVs in the simulation (up to 3).")]
    [Range(1, 3)]
    public int MaxAGVs = 3;

    [Tooltip("Spawn points for each AGV.")]
    public List<Transform> SpawnPoints = new List<Transform>();

    [Tooltip("Task destinations for AGVs.")]
    public List<Transform> TaskPoints = new List<Transform>();

    private List<GameObject> activeAGVs = new List<GameObject>();
    private bool isRunning = false;

    void Start()
    {
        // Optional: automatically start simulation
        if (SimulationTimeManager.Instance != null && SimulationTimeManager.Instance.AutoStart)
        {
            InitializeAGVs();
            StartSimulation();
        }
    }

    /// <summary>
    /// Spawns AGVs up to the specified maximum.
    /// </summary>
    public void InitializeAGVs()
    {
        CleanupAGVs();

        int spawnCount = Mathf.Min(MaxAGVs, SpawnPoints.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            var spawn = SpawnPoints[i];
            var agv = Instantiate(AGVPrefab, spawn.position, spawn.rotation);
            agv.name = $"AGV_{i + 1}";
            activeAGVs.Add(agv);
        }

        Debug.Log($"[AGVManager] Initialized {activeAGVs.Count} AGVs.");
    }

    /// <summary>
    /// Starts the simulation and schedules tasks using the SimulationTimeManager.
    /// </summary>
    public void StartSimulation()
    {
        if (isRunning) return;
        if (SimulationTimeManager.Instance == null)
        {
            Debug.LogError("[AGVManager] SimulationTimeManager is missing from the scene!");
            return;
        }

        isRunning = true;
        SimulationTimeManager.Instance.StartSimulation();

        // Assign each AGV a different starting task
        for (int i = 0; i < activeAGVs.Count; i++)
        {
            var ctrl = activeAGVs[i].GetComponent<AGVController>();
            if (ctrl != null && TaskPoints.Count > 0)
            {
                int targetIndex = i % TaskPoints.Count;
                ctrl.SetTasks(new List<Vector3> { TaskPoints[targetIndex].position });
            }
        }

        Debug.Log("[AGVManager] Simulation started — tasks assigned.");

        // Schedule timed events
        ScheduleTimedEvents();
    }

    private void ScheduleTimedEvents()
    {
        var sim = SimulationTimeManager.Instance;

        // After 10 seconds, assign new destinations
        sim.ScheduleEvent(10f, () =>
        {
            Debug.Log("[AGVManager] 10s reached — assigning new tasks.");
            AssignNextTaskBatch();
        });

        // After 20 seconds, pause simulation
        sim.ScheduleEvent(20f, () =>
        {
            Debug.Log("[AGVManager] 20s reached — pausing simulation.");
            sim.Pause();
        });

        // After 25 seconds, resume
        sim.ScheduleEvent(25f, () =>
        {
            Debug.Log("[AGVManager] 25s reached — resuming simulation.");
            sim.Resume();
        });

        // After 40 seconds, stop simulation
        sim.ScheduleEvent(40f, () =>
        {
            Debug.Log("[AGVManager] 40s reached — stopping simulation.");
            StopSimulation();
        });
    }

    private void AssignNextTaskBatch()
    {
        if (TaskPoints.Count == 0) return;

        foreach (var agv in activeAGVs)
        {
            var ctrl = agv.GetComponent<AGVController>();
            if (ctrl != null)
            {
                int randomIndex = Random.Range(0, TaskPoints.Count);
                ctrl.SetTasks(new List<Vector3> { TaskPoints[randomIndex].position });
            }
        }
    }

    /// <summary>
    /// Stops all AGVs and clears their tasks.
    /// </summary>
    public void StopSimulation()
    {
        if (!isRunning) return;

        foreach (var agv in activeAGVs)
        {
            var ctrl = agv.GetComponent<AGVController>();
            ctrl?.ClearTasks();
        }

        SimulationTimeManager.Instance.Stop();
        isRunning = false;
        Debug.Log("[AGVManager] Simulation stopped.");
    }

    /// <summary>
    /// Destroys all active AGVs.
    /// </summary>
    private void CleanupAGVs()
    {
        foreach (var agv in activeAGVs)
        {
            if (agv != null)
                Destroy(agv);
        }
        activeAGVs.Clear();
    }

    void OnDestroy()
    {
        CleanupAGVs();
    }
}
