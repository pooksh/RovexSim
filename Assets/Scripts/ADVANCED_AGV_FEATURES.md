# Advanced AGV Behavior Features

This document describes the advanced AGV behaviors that have been implemented.

## Overview

The advanced AGV system adds intelligent routing, congestion avoidance, dynamic rerouting, and a better interface for task assignment. These features make AGVs more responsive to environmental changes and better at handling complex scenarios.

## New Components

### 1. RouteManager
**Location:** `Assets/Scripts/RouteManager.cs`

The RouteManager is a singleton that manages dynamic routing for all AGVs in the system. It provides:
- **Congestion Detection**: Monitors path segments for congestion by tracking nearby AGVs
- **Dynamic Rerouting**: Provides alternative routes when paths are blocked or congested
- **Blocked Area Tracking**: Tracks temporary obstacles and finds alternative destinations
- **Path Validation**: Validates that current paths are still reachable

**Key Features:**
- Monitors congestion in real-time
- Provides alternative routes via waypoints or offset destinations
- Tracks blocked areas with expiration timestamps
- Records rerouting statistics

**Setup:**
Add a GameObject with the `RouteManager` component to your scene. It will automatically become a singleton.

### 2. WaypointManager
**Location:** `Assets/Scripts/WaypointManager.cs`

The WaypointManager provides a centralized interface for waypoint-based task assignment. It supports:
- **Waypoint Lookup**: Find waypoints by index, name, or GameObject reference
- **Task Creation**: Create tasks using waypoint indices or names
- **Waypoint Metadata**: Track waypoint usage statistics
- **Auto-Discovery**: Automatically finds waypoints tagged with "Waypoint"

**Key Methods:**
- `GetWaypointPosition(int/index)` - Get waypoint position by index or name
- `CreateTask(origin, destination)` - Create tasks using waypoint references
- `FindNearestWaypoint(position)` - Find closest waypoint to a position

**Setup:**
Add a GameObject with the `WaypointManager` component to your scene. It will automatically find waypoints tagged with "Waypoint" or you can manually assign them.

### 3. Enhanced RoviTransporter
**Location:** `Assets/Scripts/RoviTransporter.cs` (updated)

The RoviTransporter now includes advanced routing capabilities:

**New Features:**
- **Automatic Rerouting**: Detects blocked paths and finds alternatives
- **Stuck Detection**: Identifies when AGV is stuck and automatically reroutes
- **Path Validation**: Regularly validates that current path is still valid
- **Dynamic Path Adjustment**: Adjusts routes based on congestion and obstacles
- **Waypoint-Based Task Assignment**: New methods for assigning tasks using waypoints

**New Inspector Settings:**
- `Enable Rerouting`: Toggle automatic rerouting
- `Enable Stuck Detection`: Toggle stuck detection and automatic recovery
- `Reroute Check Interval`: How often to check for rerouting opportunities
- `Stuck Time Threshold`: Time before considering AGV stuck
- `Max Reroute Attempts`: Maximum rerouting attempts per task
- `Minimum Reroute Distance`: Minimum distance before rerouting can occur

**New Public Methods:**
```csharp
// Waypoint-based task assignment
AssignTaskToWaypoint(int waypointIndex, string taskId = "", string description = "")
AssignTaskToWaypoint(string waypointName, string taskId = "", string description = "")
AssignTaskByWaypoints(int originIndex, int destinationIndex, ...)
AssignTaskByWaypoints(string originName, string destinationName, ...)

// Statistics
GetRerouteAttempts() // Get number of reroute attempts for current task
```

## Usage Examples

### Basic Rerouting
Rerouting happens automatically when:
- Path becomes blocked or congested
- Destination becomes unreachable
- AGV gets stuck for more than the stuck time threshold

No code changes needed - just enable rerouting in the RoviTransporter inspector.

### Assigning Tasks with Waypoints

```csharp
// Get reference to AGV
RoviTransporter agv = GetComponent<RoviTransporter>();

// Method 1: Assign task using waypoint indices
agv.AssignTaskToWaypoint(0, "Task001", "Go to waypoint 0");

// Method 2: Assign task using waypoint names
agv.AssignTaskToWaypoint("EmergencyRoom", "Task002", "Go to ER");

// Method 3: Assign task from one waypoint to another
agv.AssignTaskByWaypoints(0, 1, "Task003", "Move from WP0 to WP1");

// Method 4: Using WaypointManager directly
WaypointManager.Instance.CreateTaskFromPosition(
    agv.GetCurrentPosition(), 
    "ICU", 
    "Task004", 
    "Go to ICU"
);
```

### Marking Areas as Blocked

```csharp
// Mark an area as blocked for 10 seconds
RouteManager.Instance.MarkAreaBlocked(position, 10f);

// Unmark an area
RouteManager.Instance.UnmarkAreaBlocked(position);

// Check if area is blocked
bool isBlocked = RouteManager.Instance.IsAreaBlocked(position);
```

### Getting Statistics

```csharp
// Get rerouting statistics from RouteManager
var (reroutes, congestions) = RouteManager.Instance.GetStatistics();

// Get reroute attempts for a specific AGV
int attempts = agv.GetRerouteAttempts();

// Get waypoint usage statistics
Dictionary<int, int> usageStats = WaypointManager.Instance.GetWaypointUsageStats();
```

## How It Works

### Rerouting Flow
1. AGV starts moving to destination
2. At regular intervals, RouteManager checks for congestion/blockages
3. If rerouting is needed:
   - RouteManager finds alternative route (via waypoint or offset destination)
   - AGV stops current path and sets new destination
   - Task destination is updated if applicable
4. Process repeats until destination is reached or max attempts reached

### Congestion Detection
1. RouteManager monitors all registered AGVs
2. For each path segment, counts nearby AGVs
3. If AGV count exceeds threshold, path is marked as congested
4. AGVs are notified to find alternative routes

### Stuck Detection
1. AGV tracks movement distance over time
2. If movement < threshold for > stuck time threshold
3. AGV automatically attempts rerouting
4. Resets stuck timer after successful movement

## Configuration Tips

### For High Traffic Areas
- Lower `congestionThreshold` in RouteManager (e.g., 1 instead of 2)
- Increase `congestionRadius` to detect congestion earlier
- Enable `enableCongestionAvoidance`

### For Complex Environments
- Increase `maxRerouteAttempts` to allow more attempts
- Lower `stuckTimeThreshold` for faster recovery
- Enable both `enableRerouting` and `enableStuckDetection`

### For Performance
- Increase `rerouteCheckInterval` to check less frequently
- Increase `pathValidationInterval` to validate less often
- Reduce `congestionCheckInterval` in RouteManager

## Integration Notes

- **Backward Compatible**: All existing code continues to work
- **Optional**: Features can be enabled/disabled in inspector
- **Singleton Pattern**: RouteManager and WaypointManager use singleton pattern
- **Auto-Registration**: AGVs automatically register with RouteManager on initialization

## Future Enhancements

Potential improvements:
- Machine learning for predicting congestion
- Dynamic speed adjustment based on congestion
- Priority-based rerouting (high-priority tasks get better routes)
- Multi-level waypoint hierarchies
- Integration with traffic lights/signals


