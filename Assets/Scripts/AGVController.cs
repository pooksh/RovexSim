using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple AGV controller: follows a list of Vector3 waypoints, reports state.
/// Designed to be simple and extensible (collision avoidance, navmesh, etc. can be added later).
/// </summary>
public class AGVController : MonoBehaviour
{
    public int AgvId = 1;
    public float Speed = 2.0f; // meters/second
    public float ArriveThreshold = 0.1f;

    private List<Vector3> _waypoints = new List<Vector3>();
    private int _currentIndex = 0;
    public bool IsRunning => _waypoints != null && _currentIndex < _waypoints.Count;
    public string Status { get; private set; } = "idle";

    void Update()
    {
        if (!IsRunning) return;

        Vector3 target = _waypoints[_currentIndex];
        Vector3 dir = target - transform.position;
        float dist = dir.magnitude;
        if (dist <= ArriveThreshold)
        {
            _currentIndex++;
            if (!IsRunning)
            {
                Status = "done";
                return;
            }
            Status = "moving";
            return;
        }

        // move towards target
        Vector3 move = dir.normalized * Speed * Time.deltaTime;
        if (move.magnitude > dist) move = dir; // don't overshoot
        transform.position += move;

        // simple facing
        if (move.sqrMagnitude > 0.0001f)
        {
            transform.forward = Vector3.Slerp(transform.forward, move.normalized, 0.2f);
        }

        Status = "moving";
    }

    public void SetTasks(List<Vector3> waypoints)
    {
        _waypoints = waypoints ?? new List<Vector3>();
        _currentIndex = 0;
        Status = _waypoints.Count > 0 ? "moving" : "idle";
    }

    public void ClearTasks()
    {
        _waypoints = new List<Vector3>();
        _currentIndex = 0;
        Status = "idle";
    }

    // Utility: exposes remaining tasks count
    public int RemainingTasks => Mathf.Max(0, _waypoints.Count - _currentIndex);
}
