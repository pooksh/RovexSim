using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System;

[CustomEditor(typeof(TaskGenerator))]
public class TaskGeneratorCustomInspector : Editor
{

    private string mapName;
    private TaskGenerator tg;
    private WaypointManager wpManager;
    private TextAsset taskFile;
    private Transform[] originWaypoints;
    private Transform[] destinationWaypoints;
    public override void OnInspectorGUI() {
        tg = (TaskGenerator)FindObjectOfType(typeof(TaskGenerator));
        taskFile = tg.file;
        wpManager = tg.wpmgr;
        originWaypoints = tg.originWaypoints;
        destinationWaypoints = tg.destinationWaypoints;
        base.OnInspectorGUI();
        EditorGUILayout.LabelField("Tasklist Generator");
        mapName = EditorGUILayout.TextField("Target Map Name", mapName);
        if (taskFile == null) {
            Debug.LogError("Enter a valid task file (.txt)");
            return;
        }
        if (GUILayout.Button("Generate Tasklist")) {
            string assetPath = AssetDatabase.GetAssetPath(taskFile);
            string fullPath = Path.GetFullPath(assetPath);

            List<string> lines = new List<string>();
            lines.AddRange(taskFile.text.Split("\n"));

            string fileMapName = lines[1];
            fileMapName = Regex.Replace(fileMapName, @"\s+", "");
            mapName = Regex.Replace(mapName, @"\s+", "");

            Debug.Log(fileMapName + ", " + mapName);

            if (fileMapName != mapName) {
                Debug.LogError($"The map name {fileMapName} found in this file does not match input map {mapName}.");
                return;
            }


            // get all waypoint transforms in the scene (BAD IF MULTIPLE MAPS IN SCENE...MUST CHANGE LATER)
            
            Transform[] waypoints = wpManager.GetAllWaypoints();
            StringBuilder newContent = new StringBuilder($"mapname\n{mapName}\nentryTime,origin,destination,id,description,priority,estimatedDuration,loadingTime\n");

            int numTasks = 30;
            for (int i = 0; i < numTasks; i++) {
                // generate a random entry time
                int hour = UnityEngine.Random.Range(0,24);
                int min = UnityEngine.Random.Range(0,60);
                string hourStr = $"{hour}";
                string minStr = $"{min}";
                if (hour < 10) {
                    hourStr = $"0{hour}";
                }
                if (min < 10) {
                    minStr = $"0{min}";
                }
                string entryTime = $"{hourStr}:{minStr}";
                // generate two random waypoints that are different
                int originIndex = UnityEngine.Random.Range(0,waypoints.Length);
                int destinationIndex = UnityEngine.Random.Range(0,waypoints.Length);
                Transform origin = waypoints[originIndex];
                Transform destination = waypoints[destinationIndex];
                while (origin.position.x == destination.position.x && origin.position.y == destination.position.y) {
                    destinationIndex = UnityEngine.Random.Range(0,waypoints.Length);
                    destination = waypoints[destinationIndex];
                }
                string originString = $"{origin.position.x};{origin.position.y}";
                string destinationString = $"{destination.position.x};{destination.position.y}";

                // generate task id {numTask}
                string taskID = $"task-{i}";
                // generate random priority between 1-3
                string priority = $"{UnityEngine.Random.Range(1,4)}";
                // generate task description "Waypoint {origin} to Waypoint {destination} on map {mapName} with priority {priority}"
                string description = $"From origin {wpManager.GetWaypointName(originIndex)} at {origin.position} to destination {wpManager.GetWaypointName(destinationIndex)} at {destination.position} on map {mapName}";
                string dummy = "";

                string line = $"{entryTime},{originString},{destinationString},{taskID},\"{description}\",{priority},{dummy},{dummy}\n"; 
                newContent.Append(line);
            }

            // random times
            // random task priority

            File.WriteAllText(fullPath, newContent.ToString());
            AssetDatabase.RenameAsset(assetPath, $"{mapName}_{DateTime.Now.ToString("ddMMyyyy_hhmm")}");
            AssetDatabase.Refresh();

        }
        EditorGUILayout.LabelField("Tasklist Preset Generator");
        if (GUILayout.Button("Generate Preset Task")) {
            string assetPath = AssetDatabase.GetAssetPath(taskFile);
            string fullPath = Path.GetFullPath(assetPath);

            List<string> lines = new List<string>();
            lines.AddRange(taskFile.text.Split("\n"));

            string fileMapName = lines[1];
            fileMapName = Regex.Replace(fileMapName, @"\s+", "");
            mapName = Regex.Replace(mapName, @"\s+", "");

            if (fileMapName != mapName) {
                Debug.LogError($"The map name {fileMapName} found in this file does not match input map {mapName}.");
                return;
            }

                        
            Transform[] waypoints = wpManager.GetAllWaypoints();
            StringBuilder newContent = new StringBuilder($"mapname\n{mapName}\nentryTime,origin,destination,id,description,priority,estimatedDuration,loadingTime\n");

            int numTasks = originWaypoints.Length;
            if (originWaypoints.Length != destinationWaypoints.Length) {
                Debug.LogError("Waypoint lists must be of same length");
            }

            for (int i = 0; i < numTasks; i++) {
                string entryTime = "";
                // generate two random waypoints that are different
                if (originWaypoints[i] == null) {
                    Debug.LogError("Waypoint is null!");
                }
                if (destinationWaypoints[i] == null) {
                    Debug.LogError("Waypoint is null!");
                }
                Transform origin = originWaypoints[i];
                Transform destination = destinationWaypoints[i]; 

                string originString = $"{origin.position.x};{origin.position.y}";
                string destinationString = $"{destination.position.x};{destination.position.y}";

                // generate task id {numTask}
                string taskID = $"task-{i}";
                // generate random priority between 1-3
                string priority = "";
                // generate task description "Waypoint {origin} to Waypoint {destination} on map {mapName} with priority {priority}"
                string description = $"From origin {origin.gameObject.name} at {origin.position} to destination {destination.gameObject.name} at {destination.position} on map {mapName}";
                string dummy = "";

                string line = $"{entryTime},{originString},{destinationString},{taskID},\"{description}\",{priority},{dummy},{dummy}\n"; 
                newContent.Append(line);
            }

            // random times
            // random task priority

            File.WriteAllText(fullPath, newContent.ToString());
            AssetDatabase.RenameAsset(assetPath, $"{mapName}_{DateTime.Now.ToString("ddMMyyyy_hhmm")}");
            AssetDatabase.Refresh();
        }
    }
}
