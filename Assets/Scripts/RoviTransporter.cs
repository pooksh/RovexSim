using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using SimulationEvents;
using System;

public class RoviTransporter : Transporter {
    [Header("AGV Movement Settings")]
    [SerializeField] private float stoppingDistance = 0.5f;  // distance at which AGV considers destination reached
    [SerializeField] private float rotationSpeed = 90f;      // degrees per second for rotation
    [SerializeField] private bool enableDebugLogs = true;    // enable debug logging for movement
    
    [Header("Advanced Routing")]
    [SerializeField] private bool enableRerouting = true;
    [SerializeField] private bool enableStuckDetection = true;
    [SerializeField] private float rerouteCheckInterval = 1f;
    [SerializeField] private float stuckTimeThreshold = 3f; //  time before considering AGV stuck
    [SerializeField] private float pathValidationInterval = 2f;  
    [SerializeField] private int maxRerouteAttempts = 3;      
    [SerializeField] private float minimumRerouteDistance = 3f;  // min # of units traveled before we can re-check for congestion to reroute
    
    [Header("Collision Avoidance")]
    [SerializeField] private float avoidanceRadius = 2f;
    [SerializeField] private float backingDistance = 1.5f; 
    [SerializeField] private float backingSpeed = 1f;
    [SerializeField] private float stuckTogetherThreshold = 1.5f;  // distance to consider AGVs stuck together
    
    private Vector3 lastPosition;
    private float timeStopped;
    private const float STOP_THRESHOLD = 0.1f;  // minimum movement to consider AGV moving
    private const float STOP_TIME_THRESHOLD = 2f;   // time in seconds to consider AGV stopped
    private float lastRerouteCheck = 0f;
    private float lastPathValidation = 0f;
    private int currentRerouteAttempts = 0;
    private Vector3 lastDestination;
    private bool isRerouting = false;
    private float distanceTraveled = 0f;
    private Vector3 lastRerouteCheckPosition;
    private int avoidancePriority; 
    private bool isBackingUp = false;
    private float backingUpStartTime = 0f;
    private const float MAX_BACKING_TIME = 3f;  // max time to back up before trying diff approach
    private Vector3 backingTarget;
    private static int nextPriority = 0;  // static counter for unique priorities

    public override void InitializeTransporter(float speed) { // speed may be a constant tho?
        SetTag();
        node = null;

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
        
        navAgent.speed = speed;
        navAgent.acceleration = speed * 2f; 
        navAgent.angularSpeed = rotationSpeed;
        navAgent.stoppingDistance = stoppingDistance;
        navAgent.autoBraking = true;
        
        // assign unique avoidance priority to prevent deadlocks; lower numbers = higher priority (0-99 range)
        avoidancePriority = nextPriority % 100;
        nextPriority++;
        navAgent.avoidancePriority = avoidancePriority;
        
        navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        navAgent.radius = 0.5f;  // agent radius for avoidance calculations
        
        //  2D top-down (XY) adjustments: keep agent in XY plane
        navAgent.updateRotation = false; // we don't want 3D yaw rotation for top-down sprites
        navAgent.updateUpAxis = false;
        Vector3 clampedStart = transform.position;
        clampedStart.z = 0f;
        transform.position = clampedStart;
        
        if (RouteManager.Instance != null){
            RouteManager.Instance.RegisterAGV(this);
        }

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
            
            // find alternative route
            if (enableRerouting && RouteManager.Instance != null){
                Vector3? alternative = RouteManager.Instance.FindAlternativeDestination(targetPosition);
                if (alternative.HasValue){
                    if (enableDebugLogs)
                        Debug.Log($"{gameObject.name}: Found alternative destination at {alternative.Value}");
                    targetPosition = alternative.Value;
                }
                else
                {
                    Debug.LogError($"{gameObject.name}: Could not find alternative route");
                    return;
                }
            }
            else{
                return;
            }
        }
        
        //  set destination and start moving (clamp Z for XY plane)
        targetPosition.z = 0f;
        destination = targetPosition;
        lastDestination = destination;
        navAgent.SetDestination(destination);
        isMoving = true;
        SetMovementState(MovementState.Moving);
        
        currentRerouteAttempts = 0;
        distanceTraveled = 0f;
        lastRerouteCheckPosition = transform.position;
        
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
                completedTask.MarkCompleted(); // TODO: will possibly need to sync with tick?
                
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
        
        //  enforce XY plane (Z=0) to work with 2D NavMesh on XY
        if (Mathf.Abs(transform.position.z) > 0f) {
            Vector3 fixedPos = transform.position;
            fixedPos.z = 0f;
            transform.position = fixedPos;
        }
        
        //  check if AGV has reached destination
        if (isMoving && HasReachedDestination()) {
            isMoving = false;
            SetMovementState(MovementState.Arrived);
        }
        
        TrackMovement();
        
        //  handle collision avoidance and stuck situations
        if (currentState == MovementState.Moving){
            HandleCollisionAvoidance();
        }
        
        //  advanced routing: check for rerouting and path validation
        if (currentState == MovementState.Moving && enableRerouting && !isBackingUp){
            CheckForRerouting();
            ValidatePath();
        }
        
        HandleCurrentState();
    }
    
    private void TrackMovement() {
        //  track if AGV is actually moving
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        
        if (distanceMoved < STOP_THRESHOLD) {
            timeStopped += Time.deltaTime;
            if (timeStopped > STOP_TIME_THRESHOLD && isMoving && enableStuckDetection && !isBackingUp) {
                if (enableDebugLogs)
                    Debug.LogWarning($"{gameObject.name} appears to be stuck!");
                
                if (IsStuckTogether()){
                    HandleStuckTogether();
                }
                //  attempt to reroute if stuck
                else if (timeStopped > stuckTimeThreshold && enableRerouting && RouteManager.Instance != null){
                    AttemptReroute();
                }
            }
        }
        else {
            timeStopped = 0f;
        }
        
        distanceTraveled += distanceMoved;
        lastPosition = transform.position;
    }
    
    private void HandleCollisionAvoidance(){
        if (isBackingUp){
            HandleBackingUp();
            return;
        }
        
        List<RoviTransporter> nearbyAGVs = GetNearbyAGVs(avoidanceRadius);
        
        if (nearbyAGVs.Count > 0){
            // check if we're heading towards another AGV
            foreach (RoviTransporter otherAGV in nearbyAGVs){
                if (otherAGV == this || otherAGV == null) continue;
                
                Vector3 toOther = (otherAGV.transform.position - transform.position);
                float distance = toOther.magnitude;
                
                // if very close and both moving, apply avoidance
                if (distance < stuckTogetherThreshold && otherAGV.GetCurrentState() == MovementState.Moving){
                    Vector3 myDirection = navAgent.velocity.normalized;
                    Vector3 otherDirection = otherAGV.navAgent.velocity.normalized;
                    
                    // if heading towards each other, one (lower priority)should yield
                    if (Vector3.Dot(myDirection, toOther.normalized) > 0.5f && 
                        Vector3.Dot(otherDirection, -toOther.normalized) > 0.5f){
                        if (avoidancePriority > otherAGV.avoidancePriority){
                            StartBackingUp();
                            break;
                        }
                    }
                }
            }
        }
    }
    
    private bool IsStuckTogether(){
        List<RoviTransporter> nearbyAGVs = GetNearbyAGVs(stuckTogetherThreshold);
        return nearbyAGVs.Count > 0;
    }
    
    private void HandleStuckTogether(){
        List<RoviTransporter> nearbyAGVs = GetNearbyAGVs(stuckTogetherThreshold);
        
        int lowestPriority = avoidancePriority;
        foreach (RoviTransporter agv in nearbyAGVs){
            if (agv != null && agv.avoidancePriority < lowestPriority){
                lowestPriority = agv.avoidancePriority;
            }
        }
        
        if (avoidancePriority > lowestPriority){
            StartBackingUp();
        }
    }

    private List<RoviTransporter> GetNearbyAGVs(float radius){
        List<RoviTransporter> nearby = new List<RoviTransporter>();
        
        if (RouteManager.Instance == null) return nearby;
        
        List<RoviTransporter> allAGVs = RouteManager.Instance.GetRegisteredAGVs();
        foreach (RoviTransporter agv in allAGVs){
            if (agv == null || agv == this || !agv.gameObject.activeInHierarchy) continue;
            
            float distance = Vector3.Distance(transform.position, agv.GetCurrentPosition());
            if (distance <= radius){
                nearby.Add(agv);
            }
        }
        
        return nearby;
    }
    
    private void StartBackingUp(){
        if (isBackingUp) return;
        
        isBackingUp = true;
        backingUpStartTime = Time.time;
        
        Vector3 backDirection;
        if (navAgent.velocity.magnitude > 0.1f){
            backDirection = -navAgent.velocity.normalized;
        }
        else{
            // if not moving, back away from destination
            backDirection = (transform.position - destination).normalized;
            if (backDirection.magnitude < 0.1f){
                // if at destination, pick a random direction
                backDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0f).normalized;
            }
        }
        
        backingTarget = transform.position + backDirection * backingDistance;
        backingTarget.z = 0f;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(backingTarget, out hit, 2f, NavMesh.AllAreas)){
            backingTarget = hit.position;
        }
        
        navAgent.ResetPath();
        navAgent.speed = backingSpeed;
        
        if (enableDebugLogs)
            Debug.Log($"{gameObject.name} backing up to let others pass");
    }
    
    private void HandleBackingUp(){
        if (Time.time - backingUpStartTime > MAX_BACKING_TIME){
            StopBackingUp();
            return;
        }
        
        Vector3 direction = (backingTarget - transform.position).normalized;
        float distanceToBackingTarget = Vector3.Distance(transform.position, backingTarget);
        
        if (distanceToBackingTarget > 0.2f){
            if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f){
                navAgent.SetDestination(backingTarget);
            }
        }
        else{
            StopBackingUp();
        }
        
        List<RoviTransporter> nearby = GetNearbyAGVs(stuckTogetherThreshold);
        if (nearby.Count == 0 && Time.time - backingUpStartTime > 0.5f){
            StopBackingUp();
        }
    }
    
    private void StopBackingUp(){
        if (!isBackingUp) return;
        
        isBackingUp = false;
        navAgent.speed = speed;
        timeStopped = 0f;
        
        if (isMoving && destination != Vector3.zero){
            navAgent.SetDestination(destination);
        }
        
        if (enableDebugLogs)
            Debug.Log($"{gameObject.name} finished backing up, resuming movement");
    }

    // check if rerouting is needed based on congestion or blocked paths
    private void CheckForRerouting(){
        if (RouteManager.Instance == null || isRerouting) return;

        if (Time.time - lastRerouteCheck < rerouteCheckInterval) return;

        lastRerouteCheck = Time.time;

        //  only reroute if we've moved minimum distance
        float distanceSinceLastCheck = Vector3.Distance(transform.position, lastRerouteCheckPosition);
        if (distanceSinceLastCheck < minimumRerouteDistance) return;

        lastRerouteCheckPosition = transform.position;

        //  check with RouteManager if rerouting is needed
        Vector3? alternativeRoute;
        if (RouteManager.Instance.ShouldReroute(transform.position, destination, out alternativeRoute)){
            if (alternativeRoute.HasValue && currentRerouteAttempts < maxRerouteAttempts){
                AttemptReroute(alternativeRoute.Value);
            }
        }
    }

    // validate that current path is still valid
    private void ValidatePath(){
        if (RouteManager.Instance == null) return;

        if (Time.time - lastPathValidation < pathValidationInterval) return;
        lastPathValidation = Time.time;

        if (!RouteManager.Instance.ValidatePath(transform.position, destination)){
            if (enableDebugLogs)
                Debug.LogWarning($"{gameObject.name}: Current path is no longer valid, attempting reroute");

            AttemptReroute();
        }

        if (navAgent.pathStatus == NavMeshPathStatus.PathInvalid || navAgent.pathStatus == NavMeshPathStatus.PathPartial){
            if (enableDebugLogs)
                Debug.LogWarning($"{gameObject.name}: NavMesh path is invalid (status: {navAgent.pathStatus}), attempting reroute");

            AttemptReroute();
        }
    }

    // attempt to reroute to alternative path
    private void AttemptReroute(Vector3? alternativeDestination = null){
        if (isRerouting || currentRerouteAttempts >= maxRerouteAttempts){
            if (enableDebugLogs)
                Debug.LogWarning($"{gameObject.name}: Cannot reroute--already rerouting or max attempts reached");
            return;
        }

        isRerouting = true;
        currentRerouteAttempts++;

        Vector3 targetDestination = alternativeDestination ?? destination;

        if (RouteManager.Instance != null && !alternativeDestination.HasValue){
            //  try to find alternative route
            Vector3? altRoute = RouteManager.Instance.FindAlternativeRoute(transform.position, destination);
            if (altRoute.HasValue){
                targetDestination = altRoute.Value;
                if (enableDebugLogs)
                    Debug.Log($"{gameObject.name}: Rerouting via alternative waypoint {targetDestination}");
            }
            else{
                //  try alternative destination
                Vector3? altDest = RouteManager.Instance.FindAlternativeDestination(destination);
                if (altDest.HasValue){
                    targetDestination = altDest.Value;
                    if (enableDebugLogs)
                        Debug.Log($"{gameObject.name}: Rerouting to alternative destination {targetDestination}");
                }
            }
        }

        //  check if we have a current task that needs updating
        if (assignedTasks.Count > 0){
            Task currentTask = assignedTasks.Peek();
            
            //  update destination if we're moving to task destination
            float distToOrigin = Vector3.Distance(transform.position, currentTask.origin);
            float distToDest = Vector3.Distance(transform.position, currentTask.destination);

            if (distToDest < distToOrigin){
                //  we're heading to final destination, update it
                currentTask.destination = targetDestination;
                destination = targetDestination;
            }
            else{
                if (Vector3.Distance(targetDestination, currentTask.origin) > 1f){
                    destination = targetDestination;
                }
            }
        }
        else{
            destination = targetDestination;
        }

        //  stop current movement and set new destination
        navAgent.ResetPath();
        navAgent.SetDestination(destination);

        //  record reroute
        if (RouteManager.Instance != null){
            RouteManager.Instance.RecordReroute();
        }

        //  reset stuck detection
        timeStopped = 0f;
        distanceTraveled = 0f;
        lastRerouteCheckPosition = transform.position;

        if (enableDebugLogs)
            Debug.Log($"{gameObject.name}: Rerouted to {destination} (Attempt {currentRerouteAttempts}/{maxRerouteAttempts})");

        //  reset rerouting flag after a brief delay
        StartCoroutine(ResetReroutingFlag());
    }

    // reset rerouting flag after delay
    private IEnumerator ResetReroutingFlag(){
        yield return new WaitForSeconds(0.5f);
        isRerouting = false;
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
    
    
    public void AssignNewTask(Vector3 origin, Vector3 destination, string associatedMap = "None", TimeOfDay entryTime = null, string taskId = "", string description = "") {
        //  create and assign a new task
        Task newTask = new Task(origin, destination, associatedMap, entryTime, taskId, description);
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

    // assign task using waypoint indices
    public void AssignTaskByWaypoints(int originWaypointIndex, int destinationWaypointIndex, string associatedMap = "None", TimeOfDay entryTime = null,
        string taskId = "", string description = ""){
        if (WaypointManager.Instance == null){
            Debug.LogError($"WaypointManager not found! Cannot assign task by waypoints.");
            return;
        }

        Task task = WaypointManager.Instance.CreateTask(originWaypointIndex, destinationWaypointIndex, associatedMap, entryTime, taskId, description);
        if (task != null){
            AssignNewTask(task);
        }
    }

    // assign task using waypoint names

    public void AssignTaskByWaypoints(string originWaypointName, string destinationWaypointName,
        string associatedMap = "None", TimeOfDay entryTime = null, string taskId = "", string description = ""){
        if (WaypointManager.Instance == null){
            Debug.LogError($"WaypointManager not found! Cannot assign task by waypoint names.");
            return;
        }

        Task task = WaypointManager.Instance.CreateTask(originWaypointName, destinationWaypointName, associatedMap, entryTime, taskId, description);
        if (task != null){
            AssignNewTask(task);
        }
    }

    // assign task from current position to waypoint index
    public void AssignTaskToWaypoint(int destinationWaypointIndex, string assignedMap = "None", TimeOfDay entryTime = null, string taskId = "", string description = ""){
        if (WaypointManager.Instance == null){
            Debug.LogError($"WaypointManager not found! Cannot assign task to waypoint.");
            return;
        }

        Task task = WaypointManager.Instance.CreateTaskFromPosition(transform.position, destinationWaypointIndex, assignedMap, entryTime, taskId, description);
        if (task != null){
            AssignNewTask(task);
        }
    }

    // assign task from current position to waypoint name
    public void AssignTaskToWaypoint(string destinationWaypointName, string assignedMap = "None", TimeOfDay entryTime = null, string taskId = "", string description = ""){
        if (WaypointManager.Instance == null){
            Debug.LogError($"WaypointManager not found! Cannot assign task to waypoint.");
            return;
        }

        Task task = WaypointManager.Instance.CreateTaskFromPosition(transform.position, destinationWaypointName, assignedMap, entryTime, taskId, description);
        if (task != null){
            AssignNewTask(task);
        }
    }
    
    public Vector3 GetCurrentPosition() {
        return transform.position;
    }
    
    public MovementState GetCurrentState() {
        return currentState;
    }
    
    public int GetTaskQueueCount() {
        return assignedTasks.Count;
    }
    
    public void EmergencyStop() {
        //  emergency stop--immediately stop all movement and clear task queue
        StopMovement();
        assignedTasks.Clear();
        
        //  reset availability state; CHECK: do we want to stop completely or just reset the queue?
        busy = false;
        available = true;
        SetMovementState(MovementState.Idle);
        
        //  reset rerouting state
        isRerouting = false;
        currentRerouteAttempts = 0;
        timeStopped = 0f;
        
        if (enableDebugLogs)
            Debug.Log($"{gameObject.name} EMERGENCY STOP activated - ready for new tasks");
    }

    // get current rerouting statistics
    public int GetRerouteAttempts(){
        return currentRerouteAttempts;
    }
    
    public int GetAvoidancePriority(){
        return avoidancePriority;
    }

    void OnDestroy(){
        //  unregister from RouteManager
        if (RouteManager.Instance != null){
            RouteManager.Instance.UnregisterAGV(this);
        }
    }

}
