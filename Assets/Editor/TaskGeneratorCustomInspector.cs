using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TaskGenerator))]
public class TaskGeneratorCustomInspector : Editor
{

    public string mapName;
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        EditorGUILayout.LabelField("Taskset Generator");
        if (GUILayout.Button("Generate Tasklist")) {
            // generate shit into txt files that importTasks can understand
            mapName = EditorGUILayout.TextField("Target Map Name", mapName);

            // if the peeked does not match target map name then 

            // get all waypoint transforms
            // random times
            // random task priority
            // estimatedDuration (literaly idk)
            // loadingTime 5 idk

        }
        if (GUILayout.Button("stupid button")) {
            Debug.Log("stupid");
        }
    }
}
