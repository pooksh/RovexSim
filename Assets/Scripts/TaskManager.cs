using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SimulationEvents;
using Utils; // priority queue port

public class TaskManager : MonoBehaviour
{
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

    void Start()
    {
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
            porter.node = tempNode;
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

        string map = lines[1];
        TimeOfDay entry;
        Vector3 origin;
        Vector3 destination;
        string taskID;
        string description;
        float priority;
        float estDuration;
        float loadingTime;

        float x, z; // TODO : change to x,y when axis updated
        List<string> coordinatesList = new List<string>();

        for (int i = 3; i < lines.Count; i++) { // ignore first three lines

            // TODO: add checks for parsing errors ughh
            // right now dont add ',' in descriptions
            // use ';' to separate coordinates

            Debug.Log(lines[i]);
            variables.AddRange(lines[i].Split(","));
            entry = new TimeOfDay(variables[0]);
            coordinatesList.Clear();
            coordinatesList.AddRange(variables[1].Split(";"));
            x = float.Parse(coordinatesList[0]);
            z = float.Parse(coordinatesList[1]);
            // TODO : change to x,y when axis updated
            origin = new Vector3(x,0,z);
            coordinatesList.Clear();
            coordinatesList.AddRange(variables[2].Split(";"));
            x = float.Parse(coordinatesList[0]);
            z = float.Parse(coordinatesList[1]);
            // TODO : change to x,y when axis updated
            destination = new Vector3(x,0,z);
            taskID = variables[3];
            description = variables[4];
            priority = float.Parse(variables[5]);
            estDuration = float.Parse(variables[6]);
            loadingTime = float.Parse(variables[7]);
            
            // Debug.Log(map + ", " + entry + ", " + origin + ", " + destination + ", " + taskID + ", " + description + ", " + priority + ", " + estDuration + ", " + loadingTime);

            unorderedTasksMaster.Add(new Task(map, entry, origin, destination, taskID, description, priority, estDuration, loadingTime));
        }

        orderedTasksMaster = new Queue<Task>(unorderedTasksMaster.OrderBy(item => item.EntryTime));
        
    }

    public void UpdateManager(TimeOfDay currentTime)  // called by time sim every tick
    {
        // update the list of available transporters
        foreach (GameObject obj in transportersMaster) {
            Transporter porter = obj.GetComponent<Transporter>();
            if (!porter.available && porter.node != null && porter.node.List == assignableTransporters) {
                assignableTransporters.Remove(porter.node);
                porter.node = null;
            }
            else if (porter.available && porter.node == null) {
                tempNode = assignableTransporters.AddLast(obj);
                porter.node = tempNode;
            }
        
        }

        // TODO: update task priority queue (rebuild)
        // clear the task priority queue if any changes were made to any task priority and 
        // only implement this if task priorities change throughout simulation

        // make tasks entered
        Task currTask = orderedTasksMaster.Peek();
        while (currTask.EntryTime == currentTime) {
            currTask.MarkEntered();
            enteredTasks.Enqueue(currTask, currTask.priority); 
            orderedTasksMaster.Dequeue();
            currTask = orderedTasksMaster.Peek();
        }

        // mark tasks assigned
       currentAssignmentMethod();
       
       //  tasks are marked completed by transporter. we may need to sync to tick later - not sure

    }

    // take tasks that are entered and not assigned and assign them to an available transporter
    // every assignment algorithm should:
    // - go through list of entered tasks
    // - find a transporter to assign the task
    // - assign the task to the transporter, mark assigned, add to assigned tasklist

    private void FirstAvailableNotBusyMethod()
    {
        foreach (GameObject obj in assignableTransporters) {
            Transporter porter = obj.GetComponent<Transporter>();
            if (!porter.busy) {
                AssignTask(enteredTasks.Dequeue(), porter);
            }
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
