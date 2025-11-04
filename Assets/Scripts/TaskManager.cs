using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SimulationEvents;
using Utils; // priority queue port
using System.Text.RegularExpressions;

public class TaskManager : MonoBehaviour
{
    [SerializeField] private bool enableDebugLogs;

    private List<Task> unorderedTasksMaster;
    private Queue<Task> orderedTasksMaster;
    private PriorityQueue<Task, float> enteredTasks;
    private List<Task> assignedTasks;
    private List<GameObject> transportersMaster;
    private LinkedList<GameObject> assignableTransporters;
    
    public enum AssignmentAlgorithm { FirstAvailable, EarliestArrivalTime }   
    [SerializeField] private AssignmentAlgorithm assignAlg;
    private delegate void ChosenAlgorithm();
    ChosenAlgorithm currentAssignmentMethod;

    [SerializeField] private TextAsset inputTasks;
    private string bigString;
    private List<string> lines;
    private List<string> variables;

    LinkedListNode<GameObject> tempNode;

    string map;
    TimeOfDay entry;
    Vector3 origin;
    Vector3 destination;
    string taskID;
    string description;
    float priority;
    float estDuration;
    float loadingTime;
    List<string> coordinatesList;
    Task newtask;

    private Regex csvSplitRegex = new Regex(
        @"
            (?:^|,)               # start of line or comma
            (?:                   # non-capturing group
            ""([^""]*)""        # quoted field in group 1
            |                   # or
            ([^,""]*)           # unquoted field in group 2
            )
        ",
        RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled
    );

    void Start()
    {
        if (inputTasks == null) {
            Debug.LogError("No map/tasklist assigned on task manager");
        }

        unorderedTasksMaster = new List<Task>();
        orderedTasksMaster = new Queue<Task>();
        enteredTasks = new PriorityQueue<Task,float>();
        assignedTasks = new List<Task>();
        transportersMaster = new List<GameObject>(GameObject.FindGameObjectsWithTag("Transporter"));
        assignableTransporters = new LinkedList<GameObject>();

        ImportTasks();

        tempNode = null;
        foreach (GameObject obj in transportersMaster) { // every node is not busy and available at the start of the day
            Transporter porter = obj.GetComponent<Transporter>();
            tempNode = assignableTransporters.AddLast(obj);
            porter.Node = tempNode;
        }

        switch (assignAlg) {
            case AssignmentAlgorithm.FirstAvailable: currentAssignmentMethod = FirstAvailableNotBusyMethod; break;
            case AssignmentAlgorithm.EarliestArrivalTime: currentAssignmentMethod = EarliestArrivalTimeMethod; break;
        }

    }

    private void ImportTasks()
    {
        bigString = inputTasks.text;
        lines = new List<string>();
        variables = new List<string>();     
        lines.AddRange(bigString.Split("\n"));

        map = lines[1];
        Debug.Log(map);

        float x, y;
        coordinatesList = new List<string>();

        for (int i = 3; i < lines.Count; i++) { // ignore first three lines
            if (string.IsNullOrWhiteSpace(lines[i])) {
                continue;
            }

            if (enableDebugLogs) {
                Debug.Log($"Current line being imported: {lines[i]}");
            }

            variables.Clear();

            foreach (Match m in csvSplitRegex.Matches(lines[i])) {
                string field;
                if (m.Groups[1].Success) {
                    field = m.Groups[1].Value;
                } else {
                    field = m.Groups[2].Value;
                }
                variables.Add(field);
            }

            entry = new TimeOfDay(variables[0]);

            coordinatesList.Clear();
            coordinatesList.AddRange(variables[1].Split(";"));
            x = float.Parse(coordinatesList[0]);
            y = float.Parse(coordinatesList[1]);
            origin = new Vector3(x, y, 0);

            coordinatesList.Clear();
            coordinatesList.AddRange(variables[2].Split(";"));
            x = float.Parse(coordinatesList[0]);
            y = float.Parse(coordinatesList[1]);
            destination = new Vector3(x, y, 0);

            taskID = variables[3];
            description = variables[4];

            if (string.IsNullOrWhiteSpace(variables[5])) {
                    priority = 0f;
                } else {
                    priority = float.Parse(variables[5]);
                }

                if (string.IsNullOrWhiteSpace(variables[6])) {
                    estDuration = 0f;
                } else {
                    estDuration = float.Parse(variables[6]);
                }

                if (string.IsNullOrWhiteSpace(variables[7])) {
                    loadingTime = 2f;
                } else {
                    loadingTime = float.Parse(variables[7]);
                }

            if (enableDebugLogs) {
                Debug.Log("Variables:" + map + ", " + entry.StringTime() + ", " + origin + ", " + destination + ", " + taskID + ", " + description + ", " + priority + ", " + estDuration + ", " + loadingTime);
            }

            newtask = new Task(map, entry, origin, destination, taskID, description, priority, estDuration, loadingTime);
            if (enableDebugLogs) {
                Debug.Log("Imported task: " + (i - 3));
            }
            unorderedTasksMaster.Add(newtask);
        }

        orderedTasksMaster = new Queue<Task>(unorderedTasksMaster.OrderBy(item => item.EntryTime));


        if (enableDebugLogs) {
            // foreach (Task t in unorderedTasksMaster) {
            //     t.SmallDebugPrintVariables();
            // }

            foreach (Task t in orderedTasksMaster) {
                t.SmallDebugPrintVariables();
            }
        }

    }

    public void UpdateManager(TimeOfDay currentTime)  // called by time sim every tick
    {
        // update the list of available transporters
        foreach (GameObject obj in transportersMaster) {
            Transporter porter = obj.GetComponent<Transporter>();
            if (!porter.IsAvailable() && porter.Node != null && porter.Node.List == assignableTransporters) {
                assignableTransporters.Remove(porter.Node);
                porter.Node = null;
            }
            else if (porter.available && porter.Node == null) {
                tempNode = assignableTransporters.AddLast(obj);
                porter.Node = tempNode;
            }
        
        }

        // TODO: update task priority queue (rebuild)
        // clear the task priority queue if any changes were made to any task priority and 
        // only implement this if task priorities change throughout simulation

        // make tasks entered
        if (orderedTasksMaster.Count() != 0) {
            Task currTask = orderedTasksMaster.Peek();
            while (currTask.EntryTime <= currentTime) {
                currTask.MarkEntered();
                if (enableDebugLogs) {
                    Debug.Log($"{currTask.description}; entered at {currentTime.StringTime()}");
                }
                enteredTasks.Enqueue(currTask, currTask.priority); 
                orderedTasksMaster.Dequeue();
                if (orderedTasksMaster.Count() == 0) {
                    break;
                }
                currTask = orderedTasksMaster.Peek();
            }
        }
        else {
            Debug.Log("there are no tasks to enter");
        }

        // mark tasks assigned
       currentAssignmentMethod();
       
    }

    public void FinishSimulation() {
        int completed = 0;
        int total = 0;
        foreach (Task t in unorderedTasksMaster) {
            total++;
            if (t.IsCompleted()) {
                completed++;
            }
        }
        Debug.Log($"{completed}/{total} Tasks completed!");
    }

    // take tasks that are entered and not assigned and assign them to an available transporter
    // every assignment algorithm should:
    // - go through list of entered tasks
    // - find a transporter to assign the task
    // - assign the task to the transporter, mark assigned, add to assigned tasklist

    private void FirstAvailableNotBusyMethod()
    {
        if (enteredTasks.Count == 0) {
            Debug.Log("there are no currently entered tasks");
            return;
        }

        if (assignableTransporters.Count != 0) {
            foreach (GameObject obj in assignableTransporters) {
                Transporter porter = obj.GetComponent<Transporter>();
                if (!porter.busy && enteredTasks.Count != 0 && !enteredTasks.Peek().IsCompleted()) {
                    AssignTask(enteredTasks.Dequeue(), porter);
                }
            }
        }
        else {
            Debug.Log("there are no currently assignable transporters");
        }
    }
    
    private void EarliestArrivalTimeMethod()
    {
        // TODO
    }

    private void AssignTask(Task task, Transporter porter)
    {
        // - assign the task to the transporter, mark assigned, add to assigned tasklist
        porter.AddTask(task);
        task.MarkAssigned();
        assignedTasks.Add(task); 
    }

}
