using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimulationEvents {

    // events and the inherited classes do not inherit from monobehaviour
    // because they are purely data stores. at least for now.

    public abstract class Event {

        // generalized event
        
        protected bool isEntered; // can it be seen by transporters?
        protected bool isAssigned; // is it assigned to one particular transporter?
        protected bool isCompleted; // has the event completed?
        private string entryTime;
        private string startTime;
        private string endTime; // arrival time in the case of task
        // TEMPORARILY STRINGS ideally these would be a custome Time structure

        // this Time structure is temp beacuse we dont have the time simulation figured out yet. 

        protected Event (string entry) {
            isEntered = false;
            isAssigned = false;
            isCompleted = false;
            entryTime = entry;
        }

        public void MarkEntered() { // to be called by manager
            isEntered = true;
        }

        public void MarkAssigned() { // to be called by transporter
            isAssigned = true;
        }

        public void MarkCompleted() {
            isCompleted = true;
        }

<<<<<<< HEAD
    }

    public class Task : Event
    {

        // transportation task

        private string associatedMap; // Map
        private string origin; // Coordinate
        private string destination; // Coordinate
        // TEMPORARILY STRINGS 

        public Task (string entry, string map, string org, string des) : base(entry) {
            associatedMap = map;
            origin = org;
            destination = des;
        }

        public string DebugPrintVariables() {
            return "Map: " + associatedMap + ", Origin: " + origin + ", Destination: " + destination;
=======
        public bool IsCompleted() {
            return isCompleted;
>>>>>>> AGVmovement
        }

    }

    public class Task : Event {
        // Transportation task properties
        public Vector3 origin;                // Starting position for the task
        public Vector3 destination;           // Target destination for the task
        public string taskId;                 // Unique identifier for the task
        public string description;            // Description of what needs to be transported
        public float priority;                // Task priority (higher = more urgent)
        public float estimatedDuration;       // Estimated time to complete task
        public bool requiresLoading;          // Does this task require loading/unloading time?
        public float loadingTime;             // Time required for loading/unloading
        
        // Task timing
        public float requestTime;             // When the task was requested
        public float deadline;                // When the task must be completed
        
        public Task(Vector3 origin, Vector3 destination, string taskId = "", string description = "") {
            this.origin = origin;
            this.destination = destination;
            this.taskId = taskId;
            this.description = description;
            this.priority = 1f;               // Default priority
            this.estimatedDuration = 60f;     // Default 1 minute duration
            this.requiresLoading = true;      // Default requires loading
            this.loadingTime = 2f;            // Default 2 seconds loading time
            this.requestTime = Time.time;     // Set request time to current time
            this.deadline = requestTime + estimatedDuration;
        }
        
        public Task(Vector3 origin, Vector3 destination, string taskId, string description, 
                   float priority, float estimatedDuration, float loadingTime = 2f) {
            this.origin = origin;
            this.destination = destination;
            this.taskId = taskId;
            this.description = description;
            this.priority = priority;
            this.estimatedDuration = estimatedDuration;
            this.requiresLoading = true;
            this.loadingTime = loadingTime;
            this.requestTime = Time.time;
            this.deadline = requestTime + estimatedDuration;
        }
        
        // Utility methods
        public float GetDistance() {
            return Vector3.Distance(origin, destination);
        }
        
        public bool IsOverdue() {
            return Time.time > deadline;
        }
        
        public float GetRemainingTime() {
            return deadline - Time.time;
        }
    }

    public class Downtime : Event {
        // transportation downtimes; when downtime is active, the transporter is disabled

        public Downtime(string entry) : base(entry) {

        }
        
    }

}