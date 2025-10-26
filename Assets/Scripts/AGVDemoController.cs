using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimulationEvents;

// demo controller script to demonstrate AGV movement functionality
// shows how to assign tasks to RoviTransporter AGVs
public class AGVDemoController : MonoBehaviour
{
    [Header("AGV Demo Settings")]
    [SerializeField] private RoviTransporter[] agvs;       
    [SerializeField] private Transform[] waypoints;     
    [SerializeField] private float demoTaskInterval = 10f;     // time between demo tasks
    [SerializeField] private bool autoStartDemo = true;        // start demo automatically
    [SerializeField] private int activeAGVCount = 1;           // number of AGVs active (1-3)
    
    [Header("UI Display")]
    [SerializeField] private bool showDebugInfo = true;        // show debug info on screen
    
    private int currentWaypointIndex = 0;
    private int taskCounter = 0;
    
    void Start() {
        //  find all RoviTransporter AGVs in the scene if not assigned
        if (agvs == null || agvs.Length == 0) {
            agvs = FindObjectsOfType<RoviTransporter>();
            Debug.Log($"Found {agvs.Length} AGVs in scene");
        }
        
        // ensure activeAGVCount is within valid range (1-3)
        activeAGVCount = Mathf.Clamp(activeAGVCount, 1, 3);
        
        //  initialize AGV active states
        UpdateActiveAGVs();
        
        if (autoStartDemo) {
            StartCoroutine(DemoTaskLoop());
        }
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
    
    private IEnumerator DemoTaskLoop() {
        while (true) {
            yield return new WaitForSeconds(demoTaskInterval);
            
            if (waypoints.Length >= 2 && agvs.Length > 0) {
                AssignDemoTask();
            }
        }
    }
    
    private void AssignDemoTask() {
        //  find an available AGV
        RoviTransporter availableAGV = GetAvailableAGV();
        
        if (availableAGV != null && waypoints.Length >= 2) {
            //  create a task between two waypoints
            Vector3 origin = waypoints[currentWaypointIndex].position;
            Vector3 destination = waypoints[(currentWaypointIndex + 1) % waypoints.Length].position;
            
            string taskId = $"DemoTask_{taskCounter++}";
            string description = $"Transport from waypoint {currentWaypointIndex} to {(currentWaypointIndex + 1) % waypoints.Length}";
            
            //  assign the task
            availableAGV.AssignNewTask(origin, destination, taskId, description);
            
            //  move to next waypoint for next task
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            
            Debug.Log($"Assigned {taskId} to {availableAGV.gameObject.name}");
        }
        else if (availableAGV == null) {
            Debug.Log("No available AGVs for task assignment");
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
        //  manual task assignment with number keys. just for testing, we can adjust to manual input later
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            AssignTaskToWaypoints(0, 1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            AssignTaskToWaypoints(1, 2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            AssignTaskToWaypoints(2, 0);
        }
        else if (Input.GetKeyDown(KeyCode.E)) {
            //  emergency stop all AGVs
            foreach (RoviTransporter agv in agvs) {
                if (agv != null) {
                    agv.EmergencyStop();
                }
            }
            Debug.Log("Emergency stop activated for all AGVs");
        }
    }
    
    private void AssignTaskToWaypoints(int originIndex, int destinationIndex) {
        if (waypoints.Length > Mathf.Max(originIndex, destinationIndex)) {
            RoviTransporter availableAGV = GetAvailableAGV();
            if (availableAGV != null) {
                Vector3 origin = waypoints[originIndex].position;
                Vector3 destination = waypoints[destinationIndex].position;
                string taskId = $"ManualTask_{taskCounter++}";
                
                availableAGV.AssignNewTask(origin, destination, taskId, $"Manual task from WP{originIndex} to WP{destinationIndex}");
                Debug.Log($"Manual task assigned: {taskId}");
            }
            else {
                Debug.Log("No available AGVs for manual task");
            }
        }
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
                GUILayout.Space(5);
                displayedCount++;
            }
        }
        
        GUILayout.Label("Controls:");
        GUILayout.Label("1, 2, 3 - Assign manual tasks");
        GUILayout.Label("4, 5, 6 - Set active AGV count");
        GUILayout.Label("E - Emergency stop all AGVs");
        
        GUILayout.EndArea();
    }
}
