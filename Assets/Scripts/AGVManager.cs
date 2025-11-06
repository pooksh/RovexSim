using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns and controls up to 3 AGVs. Attach this to an empty GameObject (e.g., "AGVManager").
/// Provide an AGV prefab that contains an AGVController component.
/// </summary>
public class AGVManager : MonoBehaviour
{
    [Header("AGV Config")]
    public GameObject AgvPrefab;            // assign prefab with AGVController
    [Range(1,3)]
    public int MaxAgvs = 1;

    private List<GameObject> _agvInstances = new List<GameObject>();

    [Header("Task points (set in the inspector or via code)")]
    public List<Transform> TaskPoints = new List<Transform>();

    public bool Running { get; private set; } = false;

    void Start()
    {
        // Optionally auto-create AGVs at start
        CreateAgvs(MaxAgvs);
    }

    public void CreateAgvs(int count)
    {
        // clamp to [1,3]
        count = Mathf.Clamp(count, 1, 3);
        MaxAgvs = count;

        // destroy existing
        foreach (var g in _agvInstances)
        {
            if (g != null) Destroy(g);
        }
        _agvInstances.Clear();

        // spawn with distinct start positions
        Vector3[] starts = new Vector3[] {
            new Vector3(-4f, 0f, -3f),
            new Vector3(-4f, 0f,  3f),
            new Vector3( 5f, 0f,  0f),
        };

        for (int i = 0; i < count; ++i)
        {
            var go = Instantiate(AgvPrefab, starts[i], Quaternion.identity, transform);
            var ctrl = go.GetComponent<AGVController>();
            if (ctrl != null) ctrl.AgvId = i + 1;
            go.name = $"AGV_{i+1}";
            _agvInstances.Add(go);
        }
    }

    public void StartSimulation()
    {
        if (Running) return;
        Running = true;

        // Assign tasks to each AGV (simple round-robin or distinct)
        int pointCount = TaskPoints.Count;
        if (pointCount == 0) {
            Debug.LogWarning("AGVManager: No TaskPoints assigned in the inspector.");
            return;
        }

        for (int i = 0; i < _agvInstances.Count; ++i)
        {
            var ctrl = _agvInstances[i].GetComponent<AGVController>();
            // Simple assignment: give each AGV 2 unique points (wrap)
            List<Vector3> tasks = new List<Vector3>();
            int baseIndex = (i * 2) % pointCount;
            tasks.Add(TaskPoints[(baseIndex) % pointCount].position);
            tasks.Add(TaskPoints[(baseIndex + 1) % pointCount].position);
            // optionally add a hub/return point if you have a central Transform named "Hub"
            ctrl.SetTasks(tasks);
        }
    }

    public void StopSimulation()
    {
        if (!Running) return;
        Running = false;
        foreach (var g in _agvInstances)
        {
            var c = g.GetComponent<AGVController>();
            c?.ClearTasks();
        }
    }

    public IEnumerable<AGVController> GetControllers()
    {
        foreach (var g in _agvInstances)
        {
            var c = g.GetComponent<AGVController>();
            if (c != null) yield return c;
        }
    }

    // Debug / status helper for UI or logging
    public string GetStatusReport()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var ctrl in GetControllers())
        {
            sb.AppendLine($"{ctrl.gameObject.name}: {ctrl.Status} (remaining {ctrl.RemainingTasks})");
        }
        return sb.ToString();
    }
}
