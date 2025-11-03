using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimulationEvents;

public class AGVDemoController : MonoBehaviour
{
    [Header("AGV Settings")]
    [SerializeField] private RoviTransporter[] agvs;       
    [SerializeField] private Transform[] waypoints;     
    [SerializeField] private int activeAGVCount = 1;     // number of AGVs active (1-3)
    
    [Header("UI Display")]
    [SerializeField] private bool showDebugInfo = true;        // show debug info on screen

    private int taskCounter = 0;
    
    void Start() {
        // always find all RoviTransporter AGVs in the scene to ensure we have the correct count
        RoviTransporter[] foundAGVs = FindObjectsOfType<RoviTransporter>();
        
        // only use serialized array if it's explicitly set and matches the found count
        // otherwise refresh to get all AGVs
        if (agvs == null || agvs.Length == 0 || foundAGVs.Length != agvs.Length) {
            agvs = foundAGVs;
            Debug.Log($"Found {agvs.Length} AGVs in scene");
        }
        else {
            // check if any AGVs in the array are null, and refresh if needed
            bool agvFoundNull = false;
            foreach (RoviTransporter agv in agvs) {
                if (agv == null) {
                    agvFoundNull = true;
                    break;
                }
            }
            
            if (agvFoundNull) {
                agvs = foundAGVs;
                Debug.Log($"Found {agvs.Length} AGVs in scene (refreshed due to null references)");
            }
        }

        bool foundNull = false;
        foreach (Transform t in waypoints) {
            if (t == null) {
                foundNull = true;
            }
        }

        if (waypoints == null || waypoints.Length == 0 || foundNull) { // find waypoints automatically if under these conditions
            GameObject[] ways = GameObject.FindGameObjectsWithTag("Waypoint");
            waypoints = new Transform[ways.Length];
            for (int i = 0; i < ways.Length; i++) {
                waypoints[i] = ways[i].transform;
            }
        }
        
        // ensure activeAGVCount is within valid range (1-3)
        activeAGVCount = Mathf.Clamp(activeAGVCount, 1, 3);
        
        // initialize AGV active states
        UpdateActiveAGVs();
        
    }
    
    void Update() {
        //  handle manual task assignment with keyboard
        HandleKeyboardInput();
        
        //  handle active AGV count changes via keyboard
        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            SetActiveAGVCount(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5)) {
            SetActiveAGVCount(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6)) {
            SetActiveAGVCount(3);
        }
    }
    

    
    private RoviTransporter GetAvailableAGV() {
        //  find the first available AGV (only from active ones)
        for (int i = 0; i < agvs.Length && i < activeAGVCount; i++) {
            if (agvs[i] != null && agvs[i].IsAvailable()) {
                return agvs[i];
            }
        }
        return null;
    }
    
    private void SetActiveAGVCount(int count) {
        activeAGVCount = Mathf.Clamp(count, 1, Mathf.Min(3, agvs.Length));
        UpdateActiveAGVs();
        Debug.Log($"Active AGV count set to {activeAGVCount}");
    }
    
    private void UpdateActiveAGVs() {
        //  enable/disable AGVs based on active count
        for (int i = 0; i < agvs.Length; i++) {
            if (agvs[i] != null) {
                agvs[i].gameObject.SetActive(i < activeAGVCount);
            }
        }
    }
    
    private void HandleKeyboardInput() {
        // manual task assignment with number keys--move from current position to waypoint
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            AssignTaskToWaypoint(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            AssignTaskToWaypoint(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            AssignTaskToWaypoint(2);
        }
        else if (Input.GetKeyDown(KeyCode.E)) {
            // emergency stop all AGVs
            foreach (RoviTransporter agv in agvs) {
                if (agv != null) {
                    agv.EmergencyStop();
                }
            }
            Debug.Log("Emergency stop activated for all AGVs");
        }
    }
    
    private void AssignTaskToWaypoint(int waypointIndex) {
        RoviTransporter targetAGV = GetAvailableAGV();

        //  if no available AGV, find the AGV with the shortest task queue
        //  TODO: integrate w/ intelligent task assignment logic
        if (targetAGV == null)
        {
            targetAGV = GetAGVWithShortestQueue();
        }
        
        //  use new WaypointManager if available, otherwise fall back to old method
        if (targetAGV != null) {
            if (WaypointManager.Instance != null && waypointIndex < WaypointManager.Instance.GetWaypointCount()) {
                string taskId = $"ManualTask_{taskCounter++}";
                string waypointName = WaypointManager.Instance.GetWaypointName(waypointIndex);
                
                targetAGV.AssignTaskToWaypoint(waypointIndex, taskId, $"Manual task to {waypointName} (WP{waypointIndex})");
                
                if (targetAGV.IsAvailable()) {
                    Debug.Log($"Manual task assigned: {taskId} to {targetAGV.gameObject.name} - moving to {waypointName} (WP{waypointIndex})");
                } else {
                    Debug.Log($"Manual task queued: {taskId} to {targetAGV.gameObject.name} - will move to {waypointName} (WP{waypointIndex}) when available");
                }
            }
            else {
                if (waypoints.Length > waypointIndex) {
                    Vector3 currentPosition = targetAGV.GetCurrentPosition();
                    Vector3 destination = waypoints[waypointIndex].position;
                    string taskId = $"ManualTask_{taskCounter++}";
                    
                    targetAGV.AssignNewTask(currentPosition, destination, taskId, $"Manual task to WP{waypointIndex}");
                    
                    if (targetAGV.IsAvailable()) {
                        Debug.Log($"Manual task assigned: {taskId} to {targetAGV.gameObject.name} - moving to WP{waypointIndex}");
                    } else {
                        Debug.Log($"Manual task queued: {taskId} to {targetAGV.gameObject.name} - will move to WP{waypointIndex} when available");
                    }
                }
                else {
                    Debug.LogWarning($"Waypoint index {waypointIndex} is out of range");
                }
            }
        }
        else {
            Debug.LogWarning("No AGVs available for manual task assignment");
        }
    }
    
    private RoviTransporter GetAGVWithShortestQueue() {
        RoviTransporter bestAGV = null;
        int shortestQueue = int.MaxValue;
        
        for (int i = 0; i < agvs.Length && i < activeAGVCount; i++) {
            if (agvs[i] != null) {
                int queueCount = agvs[i].GetTaskQueueCount();
                if (queueCount < shortestQueue) {
                    shortestQueue = queueCount;
                    bestAGV = agvs[i];
                }
            }
        }
        
        return bestAGV;
    }
    
    void OnGUI() { // TODO: change to make it look closer to the figma UI!! just made this simple version for testing
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 350));
        GUILayout.Label("AGV Fleet Management Demo", GUI.skin.box);
        
        GUILayout.Label($"AGVs in fleet: {agvs.Length}");
        GUILayout.Label($"Active AGVs: {activeAGVCount}/3");
        
        //  active AGV count controls
        GUILayout.BeginHorizontal();
        if (GUILayout.Button($"1 AGV", activeAGVCount == 1 ? GUI.skin.box : GUI.skin.button)) {
            SetActiveAGVCount(1);
        }
        if (GUILayout.Button($"2 AGVs", activeAGVCount == 2 ? GUI.skin.box : GUI.skin.button)) {
            SetActiveAGVCount(2);
        }
        if (GUILayout.Button($"3 AGVs", activeAGVCount == 3 ? GUI.skin.box : GUI.skin.button)) {
            SetActiveAGVCount(3);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        
        //  display only active AGVs?
        int displayedCount = 0;
        for (int i = 0; i < agvs.Length && displayedCount < activeAGVCount; i++) {
            if (agvs[i] != null) {
                RoviTransporter agv = agvs[i];
                GUILayout.Label($"{agv.gameObject.name}:");
                GUILayout.Label($"  State: {agv.GetCurrentState()}");
                GUILayout.Label($"  Available: {agv.IsAvailable()}");
                GUILayout.Label($"  Tasks in queue: {agv.GetTaskQueueCount()}");
                GUILayout.Label($"  Position: {agv.GetCurrentPosition()}");
                if (agv.GetRerouteAttempts() > 0) {
                    GUILayout.Label($"  Reroute attempts: {agv.GetRerouteAttempts()}");
                }
                GUILayout.Space(5);
                displayedCount++;
            }
        }
        
        GUILayout.Label("Controls:");
        GUILayout.Label("1, 2, 3 - Move AGV to waypoint");
        GUILayout.Label("4, 5, 6 - Set active AGV count");
        GUILayout.Label("E - Emergency stop all AGVs");
        
        GUILayout.EndArea();
    }
}
