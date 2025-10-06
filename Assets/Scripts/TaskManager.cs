using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimulationEvents;

public class TaskManager : MonoBehaviour
{
    // what does the task manager do?
    // takes in a file of tasks (sorted by chronological time) into a queue
    // makes tasks "entered" to transporters at particular times of the day // isEntered;

    // need a list of all tasks
    public Queue<SimulationEvents.Task> taskMasterQueue;

    delegate void assignmentAlgorithm(); // task assignment algorithm
        // take tasks that are entered and not assigned and assign them
    assignmentAlgorithm algorithm;

    public TextAsset inputTasks;
    private string bigString;
    private List<string> lines;
    private List<string> variables;

    // Start is called before the first frame update
    void Start()
    {
        // import tasks from file
        ImportTasks();
        // get list of all gameobjects with transporter tag
        // set algorithm
        algorithm = FirstAvailableTransporter;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ImportTasks() {
        taskMasterQueue = new Queue<SimulationEvents.Task>();
        bigString = inputTasks.text;
        lines = new List<string>();
        variables = new List<string>();
        lines.AddRange(bigString.Split("\n"));
        for (int i = 1; i < lines.Count; i++) { // ignore first line
            Debug.Log(lines[i]);
            variables.AddRange(lines[i].Split(","));
            taskMasterQueue.Enqueue(new SimulationEvents.Task(variables[0], variables[1], variables[2], variables[3]));
        }
        for (int i = 0; i < taskMasterQueue.Count; i++) {
            Debug.Log(taskMasterQueue.Dequeue().DebugPrintVariables());
        }
    }

    private void MakeTaskEntered(Task t) {
        // a good thing to note is that C# passes by reference,
        // so we don't need to worry about data consistency across
        // each individual transporter's list of taks and
        // the main tasklist

    }

    void FirstAvailableTransporter() {

    }
    
    void EarliestArrivalTime() {

    }

}
