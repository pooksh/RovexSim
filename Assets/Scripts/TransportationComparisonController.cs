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
    [SerializeField] private int activeAGVCount = 1;        // number of AGVs active (1-3)
    
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
    
    private TimeManager timemgr;

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
        
        //  ensure activeAGVCount is within valid range (1-3)
        activeAGVCount = Mathf.Clamp(activeAGVCount, 1, 3);
        
        //  initialize AGV active states
        UpdateActiveAGVs();
        
        if (autoStartDemo) {
            StartCoroutine(DemoTaskLoop());
        }

        timemgr = (TimeManager)FindObjectOfType(typeof(TimeManager));
        if (timemgr == null) {
            Debug.LogError("Could not find an object with time manager component. Please add an object with appropriate managers");
        }
    }
    
    void Update() {
        // handle manual task assignment with keyboard
        HandleKeyboardInput();
        
        //  handle active AGV count changes via keyboard
        if (Input.GetKeyDown(KeyCode.Keypad1)) {
            SetActiveAGVCount(1);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad2)) {
            SetActiveAGVCount(2);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad3)) {
            SetActiveAGVCount(3);
        }
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
        Vector3 origin = waypoints[currentWaypointIndex].position;
        Vector3 destination = waypoints[(currentWaypointIndex + 1) % waypoints.Length].position;
        
        string taskId = $"ComparisonTask_{taskCounter++}";
        string description = $"Transport comparison from waypoint {currentWaypointIndex} to {(currentWaypointIndex + 1) % waypoints.Length}";
        string associatedMap = "WaypointsTest-TransportationComparisonController";
        TimeOfDay entry = new TimeOfDay(timemgr.GetTimeNow());
        // determine which system to assign to
        string assignedSystem = DetermineAssignmentSystem();
        
        if (assignedSystem == "AGV") {
            RoviTransporter availableAGV = GetAvailableAGV();
            if (availableAGV != null) {
                availableAGV.AssignNewTask(associatedMap, entry, origin, destination, taskId + "_AGV", description + " (AGV)");
                Debug.Log($"Assigned {taskId} to AGV: {availableAGV.gameObject.name}");
            }
            else {
                Debug.Log("No available AGVs, task skipped");
            }
        }
        else if (assignedSystem == "Porter") {
            PorterTransporter availablePorter = GetAvailablePorter();
            if (availablePorter != null) {
                availablePorter.AssignNewTask(associatedMap, entry, origin, destination, taskId + "_Porter", description + " (Porter)");
                Debug.Log($"Assigned {taskId} to Porter: {availablePorter.gameObject.name}");
            }
            else {
                Debug.Log("No available porters, task skipped");
            }
        }
        
        // move to next waypoint for next task
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }
    
    private string DetermineAssignmentSystem() {
        if (alternateAssignment) {
            // alternate between AGV and Porter
            lastAssignedToAGV = !lastAssignedToAGV;
            return lastAssignedToAGV ? "AGV" : "Porter";
        }
        else if (randomAssignment) {
            // random assignment
            return Random.Range(0, 2) == 0 ? "AGV" : "Porter";
        }
        else if (agvPriority) {
            // prefer AGV, fallback to Porter
            if (GetAvailableAGV() != null) return "AGV";
            else return "Porter";
        }
        else {
            // prefer Porter, fallback to AGV
            if (GetAvailablePorter() != null) return "Porter";
            else return "AGV";
        }
    }
    
    private RoviTransporter GetAvailableAGV() {
        // only check active AGVs
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
    
    private void AssignTaskToWaypoints(int originIndex, int destinationIndex, string system) {
        if (waypoints.Length > Mathf.Max(originIndex, destinationIndex)) {
            Vector3 origin = waypoints[originIndex].position;
            Vector3 destination = waypoints[destinationIndex].position;
            string taskId = $"ManualTask_{taskCounter++}";
            string associatedMap = "WaypointsTest2-TransportationComparisonController";
            TimeOfDay entry = new TimeOfDay(timemgr.GetTimeNow());
            
            if (system == "AGV") {
                RoviTransporter availableAGV = GetAvailableAGV();
                if (availableAGV != null) {
                    availableAGV.AssignNewTask(associatedMap, entry, origin, destination, taskId, $"Manual AGV task from WP{originIndex} to WP{destinationIndex}");
                    Debug.Log($"Manual AGV task assigned: {taskId}");
                }
                else {
                    Debug.Log("No available AGVs for manual task");
                }
            }
            else if (system == "Porter") {
                PorterTransporter availablePorter = GetAvailablePorter();
                if (availablePorter != null) {
                    availablePorter.AssignNewTask(associatedMap, entry, origin, destination, taskId, $"Manual porter task from WP{originIndex} to WP{destinationIndex}");
                    Debug.Log($"Manual porter task assigned: {taskId}");
                }
                else {
                    Debug.Log("No available porters for manual task");
                }
            }
        }
    }
    
    private void ToggleAssignmentStrategy() {
        if (alternateAssignment) {
            alternateAssignment = false;
            randomAssignment = true;
            Debug.Log("Assignment strategy: Random");
        }
        else if (randomAssignment) {
            randomAssignment = false;
            agvPriority = true;
            Debug.Log("Assignment strategy: AGV Priority");
        }
        else if (agvPriority) {
            agvPriority = false;
            alternateAssignment = true;
            Debug.Log("Assignment strategy: Alternate");
        }
    }
    
    void OnGUI() {
        if (!showDebugInfo) return;
        
        // AGV Fleet Status
        GUILayout.BeginArea(new Rect(10, 10, 400, 250));
        GUILayout.Label("AGV Fleet Status", GUI.skin.box);
        
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
        
        for (int i = 0; i < Mathf.Min(agvs.Length, activeAGVCount); i++) {
            if (agvs[i] != null) {
                RoviTransporter agv = agvs[i];
                GUILayout.Label($"{agv.gameObject.name}: {agv.GetCurrentState()} (Available: {agv.IsAvailable()})");
            }
        }
        GUILayout.EndArea();
        
        // Porter Fleet Status
        GUILayout.BeginArea(new Rect(10, 270, 400, 200));
        GUILayout.Label("Porter Fleet Status", GUI.skin.box);
        
        GUILayout.Label($"Porters in fleet: {porters.Length}");
        
        for (int i = 0; i < Mathf.Min(porters.Length, 3); i++) { // limit display to first 3 porters
            if (porters[i] != null) {
                PorterTransporter porter = porters[i];
                GUILayout.Label($"{porter.gameObject.name}: {porter.GetCurrentState()} (Available: {porter.IsAvailable()})");
            }
        }
        GUILayout.EndArea();
        
        // Comparison Statistics and Controls
        GUILayout.BeginArea(new Rect(420, 10, 350, 450));
        GUILayout.Label("Transportation Comparison", GUI.skin.box);
        
        GUILayout.Label("Assignment Strategy:");
        if (alternateAssignment) GUILayout.Label("  Alternate");
        else if (randomAssignment) GUILayout.Label("  Random");
        else if (agvPriority) GUILayout.Label("  AGV Priority");
        
        GUILayout.Space(10);
        GUILayout.Label("Controls:");
        GUILayout.Label("1, 2, 3 - Manual AGV tasks");
        GUILayout.Label("Q, W, R - Manual Porter tasks");
        GUILayout.Label("Keypad 1, 2, 3 - Set active AGVs");
        GUILayout.Label("E - Emergency stop all");
        GUILayout.Label("Space - Toggle assignment strategy");
        
        if (showComparisonStats) {
            GUILayout.Space(10);
            GUILayout.Label("Statistics:");
            GUILayout.Label($"AGV Tasks Completed: {agvTasksCompleted}");
            GUILayout.Label($"Porter Tasks Completed: {porterTasksCompleted}");
            
            if (agvTasksCompleted > 0) {
                GUILayout.Label($"AGV Avg Time: {agvTotalTime / agvTasksCompleted:F1}s");
            }
            if (porterTasksCompleted > 0) {
                GUILayout.Label($"Porter Avg Time: {porterTotalTime / porterTasksCompleted:F1}s");
            }
        }
        
        GUILayout.EndArea();
    }
}
