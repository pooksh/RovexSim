using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using SimulationEvents;

public class RoviTransporter : Transporter {
    [Header("AGV Movement Settings")]
    [SerializeField] private float stoppingDistance = 0.5f;  // distance at which AGV considers destination reached
    [SerializeField] private float rotationSpeed = 90f;      // degrees per second for rotation
    [SerializeField] private bool enableDebugLogs = true;    // enable debug logging for movement
    
    private Vector3 lastPosition;
    private float timeStopped;
    private const float STOP_THRESHOLD = 0.1f;  // minimum movement to consider AGV moving
    private const float STOP_TIME_THRESHOLD = 2f;   // time in seconds to consider AGV stopped

    public override void InitializeTransporter(float speed) { // speed may be a constant tho?
        this.speed = speed;
        this.busy = false;
        this.available = true;
        this.assignedTasks = new Queue<Task>();
        this.assignedDowntime = new Queue<Downtime>();
        
        this.currentPosition = transform.position;
        this.destination = transform.position;
        this.isMoving = false;
        this.currentState = MovementState.Idle;
        this.lastPosition = transform.position;
        this.timeStopped = 0f;
        
        // get or add NavMeshAgent component
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null) {
            navAgent = gameObject.AddComponent<NavMeshAgent>();
            if (enableDebugLogs)
                Debug.Log($"Added NavMeshAgent to Rovi {gameObject.name}");
        }
        
        //  configure NavMeshAgent settings
        navAgent.speed = speed;
        navAgent.acceleration = speed * 2f; 
        navAgent.angularSpeed = rotationSpeed;
        navAgent.stoppingDistance = stoppingDistance;
        navAgent.autoBraking = true;
        navAgent.avoidancePriority = 50;     // medium priority for hospital corridors
        
        if (enableDebugLogs)
            Debug.Log($"Initialized RoviTransporter: {gameObject.name} with speed {speed}");
    } 

    public override void HandleDowntime() {
        //  handle downtime events (charging, maintenance, etc.)
        if (assignedDowntime.Count > 0) {
            Downtime downtime = assignedDowntime.Peek();
            if (!downtime.IsCompleted()) {
                //  set state to charging or appropriate downtime state
                SetMovementState(MovementState.Charging);
                busy = true;
                available = false;
                
                if (enableDebugLogs)
                    Debug.Log($"{gameObject.name} is now in downtime");
            }
        }
    }
    
    public override void HandleTask() {
        if (assignedTasks.Count > 0) {
            Task currentTask = assignedTasks.Peek();
            if (!currentTask.IsCompleted()) {
                //  start moving to task origin first (to pick up stretcher)
                MoveToDestination(currentTask.origin);
                busy = true;
                available = false;
                
                if (enableDebugLogs)
                    Debug.Log($"{gameObject.name} starting task {currentTask.taskId}: {currentTask.description}");
            }
        }
    }
    
    public override void MoveToDestination(Vector3 targetPosition) {
        // check if destination is reachable on NavMesh
        if (!IsDestinationReachable(targetPosition)) {
            Debug.LogWarning($"{gameObject.name}: Destination {targetPosition} is not reachable on NavMesh");
            return;
        }
        
        //  set destination and start moving
        destination = targetPosition;
        navAgent.SetDestination(destination);
        isMoving = true;
        SetMovementState(MovementState.Moving);
        
        if (enableDebugLogs)
            Debug.Log($"{gameObject.name} moving to {targetPosition}");
    }
    
    public override void StopMovement() {
        if (navAgent != null && navAgent.isActiveAndEnabled) {
            navAgent.ResetPath();
            navAgent.velocity = Vector3.zero;
        }
        
        isMoving = false;
        SetMovementState(MovementState.Idle);
        
        if (enableDebugLogs)
            Debug.Log($"{gameObject.name} stopped");
    }
    
    public override bool HasReachedDestination() { 
        if (!navAgent.pathPending && navAgent.remainingDistance < stoppingDistance) {
            return true;
        }
        return false;
    }
    
    public override void SetMovementState(MovementState newState) {
        if (currentState != newState) {
            MovementState previousState = currentState;
            currentState = newState;
            
            if (enableDebugLogs)
                Debug.Log($"{gameObject.name} state changed: {previousState} -> {newState}");
            
            //  handle state-specific logic
            OnStateChanged(previousState, newState);
        }
    }
    
    private void OnStateChanged(MovementState previousState, MovementState newState) {
        //  handle state transition logic
        switch (newState) {
            case MovementState.Idle:
                busy = false;
                available = true;
                break;
            case MovementState.Moving:
                busy = true;
                available = false;
                break;
            case MovementState.Arrived:
                // AGV has reached destination, prepare for loading/unloading
                SetMovementState(MovementState.Loading);
                break;
            case MovementState.Loading:
                //  start loading/unloading process
                StartCoroutine(LoadingSequence());
                break;
            case MovementState.Charging:
                busy = true;
                available = false;
                break;
        }
    }
    
    private IEnumerator LoadingSequence() {
        if (assignedTasks.Count > 0) {
            Task currentTask = assignedTasks.Peek();
            float loadingTime = currentTask.requiresLoading ? currentTask.loadingTime : 0.5f;
            
            //  check if we're at origin (picking up) or destination (dropping off)
            float distanceToOrigin = Vector3.Distance(transform.position, currentTask.origin);
            float distanceToDestination = Vector3.Distance(transform.position, currentTask.destination);
            
            if (distanceToOrigin < distanceToDestination) {
                //  we're at origin--picking up stretcher
                if (enableDebugLogs)
                    Debug.Log($"{gameObject.name} picking up stretcher at origin for {loadingTime} seconds");
                
                yield return new WaitForSeconds(loadingTime);
                
                //  now move to destination
                MoveToDestination(currentTask.destination);
                
                if (enableDebugLogs)
                    Debug.Log($"{gameObject.name} picked up stretcher, moving to destination");
            }
            else {
                //  we're at destination--dropping off stretcher
                if (enableDebugLogs)
                    Debug.Log($"{gameObject.name} dropping off stretcher at destination for {loadingTime} seconds");
                
                yield return new WaitForSeconds(loadingTime);
                
                //  task completed
                Task completedTask = assignedTasks.Dequeue();
                completedTask.MarkCompleted();
                
                SetMovementState(MovementState.Idle);
                
                if (enableDebugLogs)
                    Debug.Log($"{gameObject.name} completed task {completedTask.taskId}");
            }
        }
    }

    void Start() {
        //  initialize with default speed if not set
        if (speed <= 0)
            speed = 2f;
            
        InitializeTransporter(speed);
    }

    void Update() {
        //  update position tracking
        UpdatePosition();
        
        //  check if AGV has reached destination
        if (isMoving && HasReachedDestination()) {
            isMoving = false;
            SetMovementState(MovementState.Arrived);
        }
        
        TrackMovement();
        
        HandleCurrentState();
    }
    
    private void TrackMovement() {
        //  track if AGV is actually moving
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        
        if (distanceMoved < STOP_THRESHOLD) {
            timeStopped += Time.deltaTime;
            if (timeStopped > STOP_TIME_THRESHOLD && isMoving) {
                if (enableDebugLogs)
                    Debug.LogWarning($"{gameObject.name} appears to be stuck");
            }
        }
        else {
            timeStopped = 0f;
        }
        
        lastPosition = transform.position;
    }
    
    private void HandleCurrentState() {
        //  handle state-specific update logic
        switch (currentState) {
            case MovementState.Idle:
                //  check for new tasks
                if (assignedTasks.Count > 0) {
                    HandleTask();
                }
                break;
            case MovementState.Charging:
                //  handle charging logic
                HandleDowntime();
                break;
        }
    }
    
    public void AssignNewTask(Vector3 origin, Vector3 destination, string taskId = "", string description = "") {
        //  create and assign a new task
        Task newTask = new Task(origin: origin, destination: destination, taskId: taskId, description: description);
        AddTask(newTask);
        
        if (enableDebugLogs)
            Debug.Log($"{gameObject.name} assigned new task: {taskId}");
    }
    
    public void AssignNewTask(Task task) {
        //  assign an existing task
        AddTask(task);
        
        if (enableDebugLogs)
            Debug.Log($"{gameObject.name} assigned task: {task.taskId}");
    }
    
    public Vector3 GetCurrentPosition() {
        return transform.position;
    }
    
    public MovementState GetCurrentState() {
        return currentState;
    }
    
    public bool IsAvailable() {
        return available && !busy && currentState == MovementState.Idle;
    }
    
    public int GetTaskQueueCount() {
        return assignedTasks.Count;
    }
    
    public void EmergencyStop() {
        //  emergency stop--immediately stop all movement and clear task queue
        StopMovement();
        assignedTasks.Clear();
        SetMovementState(MovementState.Idle);
        
        if (enableDebugLogs)
            Debug.Log($"{gameObject.name} EMERGENCY STOP activated");
    }

}
