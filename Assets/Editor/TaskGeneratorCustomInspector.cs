using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(TaskGenerator))]
public class TaskGeneratorCustomInspector : Editor
{

    public string mapName;
    [SerializeField] private TextAsset file;
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        EditorGUILayout.LabelField("Taskset Generator");
        if (GUILayout.Button("Generate Tasklist")) {
            string assetPath = AssetDatabase.GetAssetPath(file);
            string fullPath = Path.GetFullPath(assetPath);

            mapName = EditorGUILayout.TextField("Target Map Name", mapName);
            if (file == null) {
                Debug.LogError("Enter a valid task file (.txt)");
            }
            List<string> lines = new List<string>();
            lines.AddRange(file.text.Split("\n"));

            string fileMapName = lines[1];
            if (fileMapName != mapName) {
                Debug.LogError("The map name found in this file does not exist.");
            }
            string newContent = "";

            // get all waypoint transforms
            // random times
            // random task priority

            File.WriteAllText(fullPath, newContent);
            AssetDatabase.Refresh();

        }
        if (GUILayout.Button("stupid button")) {
            Debug.Log("stupid");
        }
    }
}
