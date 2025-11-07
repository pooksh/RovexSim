using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// provides a centralized waypoint management system for task assignment.
// allows assigning tasks by waypoint ID, name, or reference.

public class WaypointManager : MonoBehaviour{
    private static WaypointManager instance;
    public static WaypointManager Instance => instance;

    [Header("Waypoint Configuration")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private string[] waypointNames;
    [SerializeField] private bool autoFindWaypoints = true;

    private Dictionary<string, int> waypointNameToIndex = new Dictionary<string, int>();
    private Dictionary<string, Transform> waypointNameToTransform = new Dictionary<string, Transform>();

    private Dictionary<int, WaypointData> waypointData = new Dictionary<int, WaypointData>();

    private void Awake(){
        if (instance == null){
            instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }

    private void Start(){
        InitializeWaypoints();
    }

    private void InitializeWaypoints(){
        //  auto-find waypoints if not assigned
        if (waypoints == null || waypoints.Length == 0 || autoFindWaypoints){
            GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag("Waypoint");
            waypoints = new Transform[waypointObjects.Length];
            for (int i = 0; i < waypointObjects.Length; i++){
                waypoints[i] = waypointObjects[i].transform;
            }

            //  sort by name to get consistent ordering
            System.Array.Sort(waypoints, (a, b) => string.Compare(a.name, b.name));
        }

        if (waypointNames == null || waypointNames.Length != waypoints.Length){
            waypointNames = new string[waypoints.Length];
            for (int i = 0; i < waypoints.Length; i++){
                if (waypoints[i] != null){
                    waypointNames[i] = waypoints[i].name;
                }
                else{
                    waypointNames[i] = $"Waypoint_{i}";
                }
            }
        }

        //  build name lookup dictionaries
        for (int i = 0; i < waypoints.Length; i++){     
        if (waypoints[i] != null){
                waypointNameToIndex[waypointNames[i]] = i;
                waypointNameToTransform[waypointNames[i]] = waypoints[i];
                
                waypointNameToIndex[waypoints[i].name] = i;
                waypointNameToTransform[waypoints[i].name] = waypoints[i];

                waypointData[i] = new WaypointData{
                    index = i,
                    transform = waypoints[i],
                    name = waypointNames[i],
                    usageCount = 0
                };
            }
        }
    }

    // get waypoint position by index
    public Vector3 GetWaypointPosition(int index){
        if (IsValidIndex(index)){
            return waypoints[index].position;
        }
        Debug.LogWarning($"Invalid waypoint index: {index}");
        return Vector3.zero;
    }

    // get waypoint position by name
    public Vector3 GetWaypointPosition(string name){
        if (waypointNameToTransform.ContainsKey(name)){
            return waypointNameToTransform[name].position;
        }
        Debug.LogWarning($"Waypoint not found: {name}");
        return Vector3.zero;
    }

    public Transform GetWaypointTransform(int index){
        if (IsValidIndex(index)){
            return waypoints[index];
        }
        return null;
    }

    public Transform GetWaypointTransform(string name){
        if (waypointNameToTransform.ContainsKey(name)){
            return waypointNameToTransform[name];
        }
        return null;
    }

    public int GetWaypointIndex(string name){
        if (waypointNameToIndex.ContainsKey(name)){
            return waypointNameToIndex[name];
        }
        return -1;
    }

    public string GetWaypointName(int index){
        if (IsValidIndex(index)){
            return waypointNames[index];
        }
        Debug.LogWarning($"Invalid waypoint index: {index}");
        return "";
    }

    public Transform[] GetAllWaypoints(){
        return waypoints;
    }

    public int GetWaypointCount(){
        return waypoints != null ? waypoints.Length : 0;
    }

    // create a task from waypoint indices
    public SimulationEvents.Task CreateTask(int originWaypointIndex, int destinationWaypointIndex, string associatedMap = "None", TimeOfDay entryTime = null,
        string taskId = "", string description = ""){
        if (!IsValidIndex(originWaypointIndex) || !IsValidIndex(destinationWaypointIndex))
        {
            Debug.LogWarning("Invalid waypoint indices for task creation");
            return null;
        }
        Vector3 origin = GetWaypointPosition(originWaypointIndex);
        Vector3 destination = GetWaypointPosition(destinationWaypointIndex);

        RecordWaypointUsage(originWaypointIndex);
        RecordWaypointUsage(destinationWaypointIndex);

        return new SimulationEvents.Task(origin, destination, associatedMap, entryTime, taskId, description);
    }

    // create a task from waypoint names
    public SimulationEvents.Task CreateTask(string originWaypointName, string destinationWaypointName,
        string associatedMap = "None", TimeOfDay entryTime = null,  string taskId = "", string description = ""){
        int originIdx = GetWaypointIndex(originWaypointName);
        int destIdx = GetWaypointIndex(destinationWaypointName);

        if (originIdx == -1 || destIdx == -1)
        {
            Debug.LogWarning($"Invalid waypoint names: {originWaypointName} -> {destinationWaypointName}");
            return null;
        }

        return CreateTask(originIdx, destIdx, associatedMap, entryTime, taskId, description);
    }

    // create a task from current AGV position to a waypoint
    public SimulationEvents.Task CreateTaskFromPosition(Vector3 originPosition, int destinationWaypointIndex,
        string associatedMap = "None", TimeOfDay entryTime = null,  string taskId = "", string description = ""){
        if (!IsValidIndex(destinationWaypointIndex)){
            Debug.LogWarning($"Invalid destination waypoint index: {destinationWaypointIndex}");
            return null;
        }

        Vector3 destination = GetWaypointPosition(destinationWaypointIndex);
        RecordWaypointUsage(destinationWaypointIndex);

        return new SimulationEvents.Task(originPosition, destination, associatedMap, entryTime, taskId, description);
    }

    // create a task from current AGV position to a waypoint by name
    public SimulationEvents.Task CreateTaskFromPosition(Vector3 originPosition, string destinationWaypointName,
        string associatedMap = "None", TimeOfDay entryTime = null,  string taskId = "", string description = ""){
        int destIdx = GetWaypointIndex(destinationWaypointName);
        if (destIdx == -1){
            Debug.LogWarning($"Invalid destination waypoint name: {destinationWaypointName}");
            return null;
        }

        return CreateTaskFromPosition(originPosition, destIdx, associatedMap, entryTime, taskId, description);
    }

    // find nearest waypoint to a position
    public int FindNearestWaypoint(Vector3 position){
        int nearestIndex = -1;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < waypoints.Length; i++){
            if (waypoints[i] == null) continue;

            float distance = Vector3.Distance(position, waypoints[i].position);
            if (distance < nearestDistance){
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    // get waypoint usage statistics
    public Dictionary<int, int> GetWaypointUsageStats(){
        Dictionary<int, int> stats = new Dictionary<int, int>();
        foreach (var kvp in waypointData){
            stats[kvp.Key] = kvp.Value.usageCount;
        }
        return stats;
    }

    // check if waypoint index is valid
    private bool IsValidIndex(int index){
        return waypoints != null && index >= 0 && index < waypoints.Length && waypoints[index] != null;
    }

    // record waypoint usage for statistics
    private void RecordWaypointUsage(int index){
        if (waypointData.ContainsKey(index)){
            waypointData[index].usageCount++;
        }
    }

    // waypoint metadata class
    private class WaypointData{
        public int index;
        public Transform transform;
        public string name;
        public int usageCount;
    }
}