# AGV Fleet Management System

This Unity project implements a basic Automated Guided Vehicle (AGV) system for hospital stretcher transportation. The system includes pathfinding, obstacle avoidance, and task management capabilities.

## Core Components

### 1. Transporter (Abstract Base Class)
- **Location**: `Assets/Scripts/Transporter.cs`
- **Purpose**: Base class defining common AGV functionality
- **Key Features**:
  - Movement state management (Idle, Moving, Arrived, Loading, Charging)
  - Task and downtime queue management
  - Abstract methods for movement control

### 2. RoviTransporter (Concrete Implementation)
- **Location**: `Assets/Scripts/RoviTransporter.cs`
- **Purpose**: Concrete implementation of AGV with Unity NavMeshAgent
- **Key Features**:
  - NavMesh-based pathfinding and obstacle avoidance
  - Automatic task execution (pickup → transport → dropoff)
  - Movement state tracking and debugging
  - Configurable speed, rotation, and stopping distance

### 3. SimulationEvents (Event System)
- **Location**: `Assets/Scripts/SimulationEvents.cs`
- **Purpose**: Defines task and downtime event structures
- **Key Features**:
  - Enhanced Task class with origin/destination coordinates
  - Task priority, timing, and loading requirements
  - Downtime management for maintenance and charging

### 4. TaskManager
- **Location**: `Assets/Scripts/TaskManager.cs`
- **Purpose**: Manages task assignment and fleet coordination
- **Status**: Framework ready for implementation

## Setup Instructions

### 1. Scene Setup
1. Create a new Unity scene with a NavMesh
2. Add NavMeshSurface component to your floor/ground object
3. Bake the NavMesh (Window -> AI -> Navigation)

### 2. AGV Setup
1. Create a GameObject for your AGV
2. Add the `RoviTransporter` component
3. The component will automatically add a NavMeshAgent
4. Configure the AGV settings in the inspector:
    - Speed: Movement speed in units per second
    - Stopping Distance: Distance considered "arrived" at destination
    - Rotation Speed: Degrees per second for turning
    - Enable Debug Logs: Toggle debug output

### 3. Demo Controller Setup
1. Add the `AGVDemoController` component to a GameObject in your scene
2. Assign your AGVs to the `agvs` array
3. Create waypoints (empty GameObjects) and assign them to the `waypoints` array
    -  Waypoints represent strategic navigation points, ex:
        Waypoint 0: Emergency Room Entrance
        Waypoint 1: Main Corridor Intersection  
        Waypoint 2: ICU Entrance
        Waypoint 3: Operating Theater
        Waypoint 4: Patient Ward A
        Waypoint 5: Charging Station
4. Configure demo settings:
    - Demo Task Interval: Time between automatic tasks
    - Auto Start Demo: Begin automatic task assignment
    - Show Debug Info: Display AGV status on screen

## Usage Examples

### Basic Task Assignment
```csharp
//  get reference to AGV
RoviTransporter agv = GetComponent<RoviTransporter>();

//  create a simple task
Vector3 origin = new Vector3(0, 0, 0);
Vector3 destination = new Vector3(10, 0, 10);
agv.AssignNewTask(origin, destination, "Task001", "Transport patient");

//  or create a detailed task
Task detailedTask = new Task(origin, destination, "Task002", "Emergency transport", 
                           priority: 2.0f, estimatedDuration: 120f, loadingTime: 3f);
agv.AssignNewTask(detailedTask);
```

### Monitoring AGV Status
```csharp
//  check if AGV is available
if (agv.IsAvailable())
{
    //  assign new task
}

//  get current state
Transporter.MovementState state = agv.GetCurrentState();

//  get position
Vector3 position = agv.GetCurrentPosition();

//  get task queue count
int pendingTasks = agv.GetTaskQueueCount();
```

### Emergency Control
```csharp
//  emergency stop
agv.EmergencyStop();

//  stop movement without clearing tasks
agv.StopMovement();
```

## Movement States

- **Idle**: AGV is stationary and ready for tasks
- **Moving**: AGV is traveling to destination
- **Arrived**: AGV has reached its destination
- **Loading**: AGV is loading/unloading cargo (stretcher)
- **Charging**: AGV is charging

## Task Workflow

1. **Task Assignment**: Task is added to AGV's queue
2. **Origin Movement**: AGV moves to task origin (pickup location)
3. **Loading**: AGV loads the stretcher (configurable time)
4. **Destination Movement**: AGV moves to task destination
5. **Unloading**: AGV unloads the stretcher (configurable time)
6. **Completion**: Task is marked complete and removed from queue

## Demo Controls

- **1, 2, 3 Keys**: Assign manual tasks between waypoints. This is just for testing and should be changed
- **E Key**: Emergency stop all AGVs
- **Automatic Mode**: Tasks are assigned automatically based on demo interval

## Customization

### Speed and Movement
- Modify `speed` parameter in `InitializeTransporter()`
- Adjust NavMeshAgent settings for different movement characteristics
- Customize acceleration, angular speed, and stopping distance

### Loading Times
- Set `loadingTime` in Task constructor
- Modify `LoadingSequence()` coroutine for complex loading logic
- Add visual/audio feedback during loading operations

### Task Priority
- Use `priority` parameter in Task constructor
- Implement priority-based task sorting in TaskManager
- Add deadline-based task management

## Troubleshooting

### AGV Not Moving
- Check if NavMesh is properly baked
- Verify AGV is on the NavMesh surface
- Ensure destination is reachable (use `IsDestinationReachable()`)

### Tasks Not Executing
- Verify AGV is in Idle state
- Check if task queue has tasks
- Enable debug logs to trace execution

### Performance Issues
- Limit number of AGVs in scene
- Optimize NavMesh complexity
- Reduce debug log frequency

## TODO

- Make an interface to create tasks rather than the current simple keyboard input
- Make task assignments intelligent; right now we are just assigning tasks to the first available AGV. Consider distance, priority, task queue length, battery level, etc.
- Make UI fit the Figma designs
- Make non-AGV movement
- And more.... Refer to rest of backlog

## Future Enhancements

- Battery management and charging stations
- Multi-AGV collision avoidance
- Dynamic pathfinding around obstacles
- Real-time task optimization
- Integration with hospital management systems
- Visual feedback and animations
- Performance metrics and reporting