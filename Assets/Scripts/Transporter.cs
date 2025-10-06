using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimulationEvents;

public abstract class Transporter : MonoBehaviour
{

    [SerializeField] protected float speed;
    protected bool busy;
    protected bool available;
    protected Queue<SimulationEvents.Task> assignedTasks;
    protected Queue<SimulationEvents.Downtime> assignedDowntime;

    public abstract void InitializeTransporter(float speed); // called in start in inherited classes, basically abstract constructor
    public abstract void HandleDowntime(); // handle downtime at front of queue
    public abstract void HandleTask(); // handle task at front of queue

    public void AddTask(Task t) { // leave marking tasks to task manager.
        assignedTasks.Enqueue(t);
    }

    public void RemoveTask() {
        assignedTasks.Dequeue();
    }

    public void AddDowntime(Downtime d) {
        assignedDowntime.Enqueue(d);
    }

    public void RemoveDowntime() {
        assignedDowntime.Dequeue();
    }

}
