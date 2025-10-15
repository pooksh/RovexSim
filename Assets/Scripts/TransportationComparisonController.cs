using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimulationEvents;

// Unified controller for comparing AGV vs Porter transportation systems
// Manages both systems simultaneously with the same task queue
public class TransportationComparisonController : MonoBehaviour
{
    [Header("System Comparison Settings")]
    [SerializeField] private RoviTransporter[] agvs;       
    [SerializeField] private PorterTransporter[] porters;       
    [SerializeField] private Transform[] waypoints;     
    [SerializeField] private float demoTaskInterval = 8f;   // time between demo tasks
    [SerializeField] private bool autoStartDemo = true; // start demo automatically
    
    [Header("Task Assignment Strategy")]
    [SerializeField] private bool alternateAssignment = true;   // alternate between AGV and Porter
    [SerializeField] private bool randomAssignment = false; // random assignment to either system
    [SerializeField] private bool agvPriority = false;  // prefer AGV assignment
    
    [Header("UI Display")]
    [SerializeField] private bool showDebugInfo = true; // show debug info on screen
    [SerializeField] private bool showComparisonStats = true;  // show comparison statistics
    
    private int currentWaypointIndex = 0;
    private int taskCounter = 0;
    private bool lastAssignedToAGV = false; // for alternating assignment
    
    private int agvTasksCompleted = 0;
    private int porterTasksCompleted = 0;
    private float agvTotalTime = 0f;
    private float porterTotalTime = 0f;
    
    void Start() {
        // find all transporters in the scene if not assigned
        if (agvs == null || agvs.Length == 0) {
            agvs = FindObjectsOfType<RoviTransporter>();
            Debug.Log($"Found {agvs.Length} AGVs in scene");
        }
        
        if (porters == null || porters.Length == 0) {
            porters = FindObjectsOfType<PorterTransporter>();
            Debug.Log($"Found {porters.Length} porters in scene");
        }
        
        if (autoStartDemo) {
            StartCoroutine(DemoTaskLoop());
        }
    }
    
    void Update() {
        // handle manual task assignment with keyboard
        HandleKeyboardInput();
    }
    
    private IEnumerator DemoTaskLoop() {
        while (true) {
            yield return new WaitForSeconds(demoTaskInterval);
            
            if (waypoints.Length >= 2 && (agvs.Length > 0 || porters.Length > 0)) {
                AssignComparisonTask();
            }
        }
    }
    
    private void AssignComparisonTask() {
        // create a task between two waypoints
    }
    
    private string DetermineAssignmentSystem() {
    }
    
    private RoviTransporter GetAvailableAGV() {
        foreach (RoviTransporter agv in agvs) {
            if (agv != null && agv.IsAvailable()) {
                return agv;
            }
        }
        return null;
    }
    
    private PorterTransporter GetAvailablePorter() {
        foreach (PorterTransporter porter in porters) {
            if (porter != null && porter.IsAvailable()) {
                return porter;
            }
        }
        return null;
    }
    
    private void HandleKeyboardInput() {
        // combined controls for both systems.. these have just become convoluted demo controls. FIX later
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            AssignTaskToWaypoints(0, 1, "AGV");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            AssignTaskToWaypoints(1, 2, "AGV");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            AssignTaskToWaypoints(2, 0, "AGV");
        }
        else if (Input.GetKeyDown(KeyCode.Q)) {
            AssignTaskToWaypoints(0, 1, "Porter");
        }
        else if (Input.GetKeyDown(KeyCode.W)) {
            AssignTaskToWaypoints(1, 2, "Porter");
        }
        else if (Input.GetKeyDown(KeyCode.R)) {
            AssignTaskToWaypoints(2, 0, "Porter");
        }
        else if (Input.GetKeyDown(KeyCode.E)) {
            // emergency stop all transporters
            foreach (RoviTransporter agv in agvs) {
                if (agv != null) {
                    agv.EmergencyStop();
                }
            }
            foreach (PorterTransporter porter in porters) {
                if (porter != null) {
                    porter.EmergencyStop();
                }
            }
            Debug.Log("Emergency stop activated for all transporters");
        }
        else if (Input.GetKeyDown(KeyCode.Space)) {
            // toggle assignment strategy
            ToggleAssignmentStrategy();
        }
    }
    
    private void ToggleAssignmentStrategy() {
    }
    
    private void AssignTaskToWaypoints(int originIndex, int destinationIndex, string system) {
    }
    
    void OnGUI() {

}
