using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimulationEvents {

    // events and the inherited classes do not inherit from monobehaviour
    // because they are purely data stores. at least for now.

    public abstract class Event
    {

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
        }

    }

    public class Downtime : Event
    {
        // transportation downtimes; when downtime is active, the transporter is disabled

        public Downtime(string entry) : base(entry) {

        }
        
    }

}