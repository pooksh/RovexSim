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
