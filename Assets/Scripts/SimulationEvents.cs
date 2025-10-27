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
        protected TimeOfDay entryTime;
        protected TimeOfDay startTime;
        protected TimeOfDay endTime { get; private set; } // arrival time in the case of task
        protected string associatedMap;

        protected Event () {
            isEntered = false;
            isAssigned = false;
            isCompleted = false;
        }

        public void MarkEntered() { // to be called by manager
            isEntered = true;
        }

        public void MarkAssigned() { // to be called by manager
            isAssigned = true;
        }

        public void MarkCompleted() { // to be called by transporter
            isCompleted = true;
        }

        public bool IsCompleted() {
            return isCompleted;
        }

        public TimeOfDay EntryTime => entryTime; // public getter

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
        
        public Task(string associatedMap, TimeOfDay entryTime, Vector3 origin, Vector3 destination, string taskId = "", string description = "") {
            this.associatedMap = associatedMap;
            this.entryTime = entryTime;
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
        
        public Task(string associatedMap, TimeOfDay entryTime, Vector3 origin, Vector3 destination, string taskId, string description, 
                   float priority, float estimatedDuration, float loadingTime = 2f) {
            this.associatedMap = associatedMap;
            this.entryTime = entryTime;
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
        
        public string GetSmallStringVariables() {
            return "Map: " + associatedMap + ", Entry Time: " + entryTime.StringTime() + ", Origin: " + origin + ", Destination: " + destination;
        }

        public string GetStringVariables() {
            return "Map: " + associatedMap + ", Entry Time: " + entryTime.StringTime() + ", Origin: " + origin + ", Destination: " + destination + ", TaskID: " + taskId + ", Description: " + description + ",\n Priority: " + priority + ", Estimated Duration: " + estimatedDuration + ", Requires Loading: " + requiresLoading + ", Loading Time: " + loadingTime + ", Request Time: " + requestTime + ", Deadline: " + deadline;
        }

        public void SmallDebugPrintVariables() {
            Debug.Log(GetSmallStringVariables());
        }

        public void DebugPrintVariables() {
            Debug.Log(GetStringVariables());
        }


    }

    public class Downtime : Event {
        // transportation downtimes; when downtime is active, the transporter is disabled

        public Downtime() {

        }
        
    }

}