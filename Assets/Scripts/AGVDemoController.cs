using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimulationEvents;

// demo controller script to demonstrate AGV movement functionality
// shows how to assign tasks to RoviTransporter AGVs
public class AGVDemoController : MonoBehaviour
{
    [Header("Demo Settings")]
    [SerializeField] private RoviTransporter[] agvs;       
    [SerializeField] private Transform[] waypoints;     
    [SerializeField] private float demoTaskInterval = 10f;     // time between demo tasks
    [SerializeField] private bool autoStartDemo = true;        // start demo automatically
    
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
        
        if (autoStartDemo) {
            StartCoroutine(DemoTaskLoop());
        }
    }
    
    void Update() {
        //  handle manual task assignment with keyboard
        HandleKeyboardInput();
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
        //  find the first available AGV
        foreach (RoviTransporter agv in agvs) {
            if (agv != null && agv.IsAvailable()) {
                return agv;
            }
        }
        return null;
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
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.Label("AGV Fleet Management Demo", GUI.skin.box);
        
        GUILayout.Label($"AGVs in fleet: {agvs.Length}");
        
        for (int i = 0; i < agvs.Length; i++) {
            if (agvs[i] != null) {
                RoviTransporter agv = agvs[i];
                GUILayout.Label($"{agv.gameObject.name}:");
                GUILayout.Label($"  State: {agv.GetCurrentState()}");
                GUILayout.Label($"  Available: {agv.IsAvailable()}");
                GUILayout.Label($"  Tasks in queue: {agv.GetTaskQueueCount()}");
                GUILayout.Label($"  Position: {agv.GetCurrentPosition()}");
                GUILayout.Space(5);
            }
        }
        
        GUILayout.Label("Controls:");
        GUILayout.Label("1, 2, 3 - Assign manual tasks");
        GUILayout.Label("E - Emergency stop all AGVs");
        
        GUILayout.EndArea();
    }
}
