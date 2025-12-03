# RovexSim Documentation

Welcome to RovexSim! This is a Unity-based hospital logistics simulation that compares automated stretcher transportation (using Rovi AMRs) against manual porter-based transportation.

For further information, check out the project report.

---

## Table of Contents

1. [Deployment Instructions (Windows)](#deployment-instructions-for-windows)
2. [Project Overview](#project-overview)
3. [Architecture](#architecture)
4. [Key Systems](#key-systems)
5. [Detailed Guides](#detailed-guides)

---

## Deployment Instructions For Windows

1. Clone the github repository: https://github.com/pooksh/RovexSim.git
2. Install Docker Desktop and open it.
3. Add your installation’s Docker Desktop \bin location to the PATH variable. The normal installation location is:
            C:\Program Files\Docker\Docker\resources\bin
    You can access the PATH variables by opening the System Properties control panel → Advanced → Environment Variables → Select Path → Edit → Click new and paste the full file location of the \bin folder → OK and Apply.
4. Open terminal and navigate in the project directory to:
            ..\Backend\src\docker-compose\
5. Build the backend API and SQL containers using the command: 
            docker compose up
    **Command will not work if PATH variable is not set up correctly.**
6. Open another terminal and navigate to
            ..\Backend\src\docker-compose\init-db\
7. Note: For the initial script and overall database management, it can be run using your DBMS of choice (it is currently a MSSQL database running in a Linux container), or you can run using Microsoft’s command line sqlcmd tool. This next step will use sqlcmd to accomplish this. 

    Replace the <SA_PASSWORD> with the value you find with the same name in .\Backend\src\docker-compose\docker-compose.yml (this can also be changed for production usage, it is currently just a placeholder value in both the yml file and the backend appsettings.json).
            winget install sqlcmd
            sqlcmd -S localhost,1433 -U sa -P <SA_PASSWORD> -i 01-init.sql
8. Once running, the backend API is accessible through
	        http://localhost:5000/swagger/index.html
    Register a user through the associated endpoint. An example of an appropriate register user request body:
    {
    "username": "test",
    "password": "test",
    "firstName": "john",
    "lastName": "egbert",
    "permission": "admin"
    }

---

## Project Overview

### What is RovexSim?

RovexSim is a marketing tool that simulates hospital stretcher transportation using two different methods:

1. **Rovi System**: Autonomous Mobile Robots (AMRs) that autonomously transport stretchers using pathfinding
2. **Porter System**: Human porters who manually transport stretchers

Both systems can receive tasks (ex. "Transport patient from Room/Waypoint 1 to Room/Waypoint 9") for comparison.

### Why This Matters

Hospital logistics is expensive and time-consuming. RovexSim lets hospital administrators see:
- How much faster AMRs could be than manual porters
- How long the AMRs sit idle or charging
- Overall cost savings potential
- Whether the investment in automation is worth it

---

## Architecture

### High-Level Structure

```
RovexSim/
├── Assets/
│   ├── Scenes/              # Unity scenes for different simulation scenarios
│   ├── Scripts/             # All C# game logic
│   ├── Prefabs/             # Reusable game objects (AMRs, porters, UI)
│   ├── Sprites/             # 2D graphics and icons
│   └── Settings/            # Unity configuration files
├── Backend/                 
│   └── src/
│       ├── Controllers/     # API endpoints
│       ├── Models/          # Data models
│       └── docker-compose/  # Docker configuration
└── ProjectSettings/         # Unity engine configuration
```

---

## Key Systems

### 1. **Transporter System** 

Everything that moves in the simulation inherits from the `Transporter` base class. Two implementations exist:

#### RoviTransporter (Automated)
- Uses NavMeshAgent for pathfinding
- Can navigate around obstacles automatically
- Tracks battery level and charging

**Key Methods:**
```csharp
AssignNewTask(origin, destination, taskId, description);
IsAvailable();  
GetCurrentState();  // Idle? Moving? Charging?
EmergencyStop(); 
```

#### PorterTransporter (Manual)
- Simulates human porter behavior
- Takes breaks
- Useful for comparison

**Key Methods:**
```csharp
AssignNewTask(origin, destination, taskId, description);
TakeBreak(duration);  // Porters need rest!
```

### 2. **Task Management System**

**How it works:**

1. **TaskManager**: Holds all tasks waiting to be done (like a hospital queue)
2. **Individual Transporters**: Execute the tasks and track how long they take

### 3. **Advanced Routing System**

When AMRs are moving, they need to be smart about their routes:

- **RouteManager**: Detects congestion (too many AMRs on same path)
- **WaypointManager**: Provides predefined waypoints (ER entrance, hospital beds, etc.)
- **Dynamic Rerouting**: If a path is blocked or congested, AMRs find alternative routes

---

### Configuring AMR Behavior

In the `RoviTransporter` inspector:

```
Speed                          5.0
Rotation Speed                 120.0
Stopping Distance              0.5
Enable Rerouting               ON     ← Smart routing
Enable Stuck Detection         ON     ← Detects stuck AMRs
Reroute Check Interval         2.0    ← Check every 2 seconds
Stuck Time Threshold           3.0    ← Mark stuck after 3 secs
Max Reroute Attempts           3      ← Try 3 alternative routes
Enable Charging                ON     ← Realistic battery
Charging Interval              300    ← Charge every 5 min
Charging Duration              60     ← Charging takes 1 min
```

### Configuring Porter Behavior

Similar settings in `PorterTransporter`:
```
Speed                          3.5    ← Humans walk slower
Enable Breaks                  ON
Break Interval                 3600   ← Break every 1 hour
Break Duration                 600    ← 10 minute break
```

---

## Detailed Guides

### Guide 1: Running the Application

**Steps:**

    You can run the application two ways: either through Unity Editor (development and configuration purposes) or create a standalone build and run.

    Unity Editor
    1. Open the Unity project.
    2. Confirm that your server is running and you are logged in as server admin (see deployment instructions).
    3. Go to File → Open Scene. Choose the OptionsScene scene from the Assets\Scenes folder to run through the entire application, or for other purposes, open the relevant scene.
    4. Click the play button in the middle of the banner at the top.

    Build
    1. Open the Unity project.
    2. Click File → Build and Run. Select a build location and the application will start.

### Guide 2: Using the Application

**Steps:**

    1. You will be greeted with the Login page. Confirm that you have a user registered (see deployment instructions). Type in the credentials you created and click Login. To quit the application, press the button in the top left corner.
    2. If you have logged in successfully, you will be greeted with the options screen, which allows  you to change the speed of Rovi, the number of Rovi, and the number of human transporters. When you want to start the simulation, press the start button in the lower right corner. Again, to quit the application, press the button in the top left corner.
    3. On the Simulation screen, you will be greeted by a hospital map (Anaheim Memorial Medical Centre) given by Rovex and translated into a tilemap floorplan with obstacles (red tiles), rooms (blue tiles), and hallways (grey tiles). The simulation will start right away with the options that were input: the correct amount of each type of transporter will spawn and the speed of the Rovi transporters will change based on your input (the speed of human transporters will stay fixed). You will see a clock in the upper right hand corner of the simulation screen ticking up every 5 minutes. This is a 24 clock which represents the current time in the hospital. Every tick, tasks are entered into the system at random times according to the task list configured in the Unity Editor in Task Generator and Task Manager. As they enter the system, you will see the spawned transporters being assigned tasks via the current assignment method (Task Manager), and each transporter handling tasks appropriately by visiting the waypoints associated with the task, simulating unloading and loading times, avoiding each other and obstacles. The time simulation will end when the clock hits midnight. (If you are running in the editor, a console log message will tell you how many tasks were completed out of the total tasklist). 
    4. Changing Application Settings and from the Unity Editor (Development and Configuration)
    Some simulation attributes cannot be changed in-program, but need explicit changes from the Unity Editor. Make sure you are in the simulation scene in the Unity Editor if you wish to change the following items.


### Guide 3: Adding Custom Waypoints

**Steps:**

    1. Decide on hospital locations: ER, OR, ICU, Pharmacy, etc.
    2. For each location:
        - Create an empty GameObject
        - Name it: `Waypoint_ER`, `Waypoint_OR`, etc.
        - Position it at that location in your scene
        - Add tag: `Waypoint` (right-click object → Tags → Create new tag "Waypoint")

### Guide 4: Creating tasks

**Options:**

1. **Add Tasks:**
   - Drag origin waypoint into "Origin Waypoints" section under the TaskGenerator GameObject.
   - Drag destination waypoint into "Destination Waypoints" section under the TaskGenerator GameObject.
   - ex. If going from Waypoint 1 to Waypoint 3, drag Waypoint 1 into the Origin Waypoints section and Waypoint 3 into the Destination Waypoints section, ensuring they are both in the same nth position of either list.
   - Drag an input map file into the "File" section under "Task Generator (Script)". The file should contain "mapname
    [INSERT MAP NAME HERE]
    entryTime,origin,destination,id,description,priority,estimatedDuration,loadingTime". The name can be anything. Everything else in the file will be overwritten, so what it contains does not matter yet.
   - Write map name from second line of input file under "Target Map Name" in the "Destination Waypoints" section.
   - Press "Generate Preset Task"
   - Drag same file from before, now edited, into the TaskManager GameObject Input Tasks. This should be under Managers -> Task Manager (Script) -> "Input Tasks".
   - Press play.

2. **Use Waypoints in Code:**

   **Option A: Assign by Index**
   ```csharp
   AMR.AssignTaskToWaypoint(0);
   // Goes to first waypoint
   ```

   **Option B: Between Two Waypoints**
   ```csharp
   AMR.AssignTaskByWaypoints(0, 2);
   // Route from first to third waypoint
   ```
   
   **Option C: Between Two Waypoints Assigned By System**
   ```csharp
   AMR.AssignTaskByWaypoints(0, 2, "Porter");
   // Route from first to third waypoint assigned to a human porter
   AMR.AssignTaskByWaypoints(0, 2, "AGV");
   // Route from first to third waypoint assigned to an AMR
   ```

---

## File Organization Cheat Sheet

**Quick Reference: Where to find things**

| What? | Where? |
|-------|--------|
| 3D Models & Objects | `Assets/Prefabs/` |
| Main Scripts | `Assets/Scripts/` |
| Hospital Scenes | `Assets/Scenes/` |
| Graphics & Icons | `Assets/Sprites/` |
| Game Configuration | `Assets/Settings/` |
| Backend API | `Backend/src/Controllers/` |
| Database Models | `Backend/src/Models/` |
| Unit Tests | `Backend/src/Tests/` (if exists) |
| Docker Setup | `Backend/src/docker-compose/` |

---

## Next Steps

- Safely refactor all instances of 'AGV' to 'AMR'.
- Allow for the comparison of human porters and AMRs; assign the same task to both and compare metrics.

---

## Glossary

- **AMR**: Autonomous Mobile Robot
- **NavMesh**: Navigation Mesh (invisible walkway map for pathfinding)
- **Transporter**: Base class for anything that moves (AMR or Porter)
- **Task**: A transportation request (pick up patient from location A, drop off at location B)
- **Waypoint**: Named location in the hospital (ER, OR, ICU)
- **RoviTransporter**: AMR implementation
- **PorterTransporter**: Human porter implementation (manual labor, takes breaks)
- **RouteManager**: System that detects congestion and provides alternative routes

---