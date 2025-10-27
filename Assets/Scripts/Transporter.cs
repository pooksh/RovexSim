using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using SimulationEvents;

public abstract class Transporter : MonoBehaviour {

    [SerializeField] protected float speed;
    public bool busy;
    public bool available;
    protected Queue<SimulationEvents.Task> assignedTasks;
    protected Queue<SimulationEvents.Downtime> assignedDowntime;
    
    protected Vector3 currentPosition;  // current position of the AGV (Vector3 -> x,y,z coordinates)
    protected Vector3 destination;  
    protected bool isMoving;  
    protected NavMeshAgent navAgent;    // unity NavMeshAgent for pathfinding
    
    protected LinkedListNode<GameObject> node; // a pointer to my spot in the linked list of available / not busy transporters for O(1) access/deletion
    public LinkedListNode<GameObject> Node { // getters n setters
        get { return node; }
        set { node = value; } 
    } 

    // navmesh logic is blackbox so we may see limitations in using this. we can start w/ it and see where/if it breaks
    
    public enum MovementState {
        Idle,   // AGV is stationary and ready for tasks
        Moving,
        Arrived,
        Loading,    // AGV is loading/unloading cargo (stretcher)
        Charging
    }
    protected MovementState currentState;

    public abstract void InitializeTransporter(float speed); // called in start in inherited classes, basically abstract constructor
    public abstract void HandleDowntime(); // handle downtime at front of queue
    public abstract void HandleTask(); // handle task at front of queue
    
    public abstract void MoveToDestination(Vector3 targetPosition);
    public abstract void StopMovement();
    public abstract bool HasReachedDestination();
    public abstract void SetMovementState(MovementState newState); 

    protected void SetTag() { // call alongside initialization
        gameObject.tag = "Transporter";
    }

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
    
    protected void UpdatePosition() {
        currentPosition = transform.position;
    }
    
    protected float GetDistanceToDestination() {
        return Vector3.Distance(currentPosition, destination);
    }

    public bool IsAvailable() {
        return available && !busy && currentState == MovementState.Idle;
    }

    protected bool IsDestinationReachable(Vector3 targetPosition) {
        //  check if destination is on the NavMesh and reachable
        NavMeshHit hit; //  holds properties of the resulting location
        return NavMesh.SamplePosition(targetPosition, out hit, 1.0f, NavMesh.AllAreas);
        //  change from NavMesh.AllAreas when we figure out what areas we are restricting (https://docs.unity3d.com/550/Documentation/Manual/nav-AreasAndCosts.html)
    }

}
