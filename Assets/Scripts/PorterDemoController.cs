using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimulationEvents;

// Demo controller script to demonstrate Porter movement functionality
// Shows how to assign tasks to PorterTransporter (porters)
public class PorterDemoController : MonoBehaviour
{
    [Header("Porter Demo Settings")]
    [SerializeField] private PorterTransporter[] porters;    
    [SerializeField] private PatientMetrics[] patients;
    [SerializeField] private Transform[] waypoints;     
    [SerializeField] private float demoTaskInterval = 12f;     // time between demo tasks (longer than AGV)
    [SerializeField] private bool autoStartDemo = true;        // start demo automatically
    
    [Header("UI Display")]
    [SerializeField] private bool showDebugInfo = true;        // show debug info on screen
    
    private int currentWaypointIndex = 0;
    private int taskCounter = 0;

    private TimeManager timemgr;
    
    void Start() {
        // find all PorterTransporter porters in the scene if not assigned
        if (porters == null || porters.Length == 0) {
            porters = FindObjectsOfType<PorterTransporter>();
            Debug.Log($"Found {porters.Length} porters in scene");
        }
        
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
    }
    
    private IEnumerator DemoTaskLoop() {
        while (true) {
            yield return new WaitForSeconds(demoTaskInterval);
            
            if (waypoints.Length >= 2 && porters.Length > 0) {
                AssignDemoTask();
            }
        }
    }
    
    private void AssignDemoTask() {
    // find an available porter
    PorterTransporter availablePorter = GetAvailablePorter();

    // added: also check that there are patients available
    if (availablePorter != null && waypoints.Length >= 2 && patients.Length > 0)
    {
        // create a task between two waypoints
        Vector3 origin = waypoints[currentWaypointIndex].position;
        Vector3 destination = waypoints[(currentWaypointIndex + 1) % waypoints.Length].position;

        // updated task details
        string associatedMap = "WaypointsTest-PorterDemo";
        string taskId = $"PatientTransport_{taskCounter++}";
        string description = "Transport non-critical patient";
        TimeOfDay entry = new TimeOfDay(timemgr.GetTimeNow());

        // pick a random patient from the array
        PatientMetrics patient = patients[Random.Range(0, patients.Length)];

        // create a Task object (if using the Task class from SimulationEvents)
        Task patientTask = new Task(associatedMap, entry, origin, destination, taskId, description);

        // link the patient to the task
        patient.AssignTask(patientTask);

        // assign the task to the porter as before
        availablePorter.AssignNewTask(patientTask);

        Debug.Log($"Assigned {taskId} to {availablePorter.gameObject.name} for patient {patient.patientId}");

        // move to next waypoint for next task
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }
    else if (availablePorter == null)
    {
        Debug.Log("No available porters for task assignment");
    }
}
    
    private PorterTransporter GetAvailablePorter() {
        // find the first available porter
        foreach (PorterTransporter porter in porters) {
            if (porter != null && porter.IsAvailable()) {
                return porter;
            }
        }
        return null;
    }
    
    private void HandleKeyboardInput() {
        // manual task assignment with letter keys for porters
        if (Input.GetKeyDown(KeyCode.Q)) {
            AssignTaskToWaypoints(0, 1);
        }
        else if (Input.GetKeyDown(KeyCode.W)) {
            AssignTaskToWaypoints(1, 2);
        }
        else if (Input.GetKeyDown(KeyCode.R)) {
            AssignTaskToWaypoints(2, 0);
        }
        else if (Input.GetKeyDown(KeyCode.T)) {
            // emergency stop all porters
            foreach (PorterTransporter porter in porters) {
                if (porter != null) {
                    porter.EmergencyStop();
                }
            }
            Debug.Log("Emergency stop activated for all porters");
        }
    }
    
    private void AssignTaskToWaypoints(int originIndex, int destinationIndex) {
        string associatedMap = "WaypointsTest-PorterDemo";
        TimeOfDay entry = new TimeOfDay(timemgr.GetTimeNow());
        if (waypoints.Length > Mathf.Max(originIndex, destinationIndex)) {
            PorterTransporter availablePorter = GetAvailablePorter();
            if (availablePorter != null) {
                Vector3 origin = waypoints[originIndex].position;
                Vector3 destination = waypoints[destinationIndex].position;
                string taskId = $"PorterManualTask_{taskCounter++}";
                                
                availablePorter.AssignNewTask(associatedMap, entry, origin, destination, taskId, $"Manual porter task from WP{originIndex} to WP{destinationIndex}");
                Debug.Log($"Manual porter task assigned: {taskId}");
            }
            else {
                Debug.Log("No available porters for manual task");
            }
        }
    }
    
    void OnGUI() {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 320, 400, 300)); // positioned below AGV UI
        GUILayout.Label("Porter Fleet Management Demo", GUI.skin.box);
        
        GUILayout.Label($"Porters in fleet: {porters.Length}");
        
        for (int i = 0; i < porters.Length; i++) {
            if (porters[i] != null) {
                PorterTransporter porter = porters[i];
                GUILayout.Label($"{porter.gameObject.name}:");
                GUILayout.Label($"  State: {porter.GetCurrentState()}");
                GUILayout.Label($"  Available: {porter.IsAvailable()}");
                GUILayout.Label($"  Tasks in queue: {porter.GetTaskQueueCount()}");
                GUILayout.Label($"  Position: {porter.GetCurrentPosition()}");
                GUILayout.Space(5);
            }
        }
        
        GUILayout.Label("Porter Controls:");
        GUILayout.Label("Q, W, R - Assign manual porter tasks");
        GUILayout.Label("T - Emergency stop all porters");
        
        GUILayout.EndArea();
    }
}
