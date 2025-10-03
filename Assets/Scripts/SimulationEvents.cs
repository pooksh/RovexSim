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
        // private Time entryTime;
        // private Time startTime;
        // private Time endTime; // arrival time in the case of task

        // this Time structure is temp beacuse we dont have the time simulation figured out yet. 

        protected Event () {
            isEntered = false;
            isAssigned = false;
            isCompleted = false;
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
        
        // private origin
        // private destination;

        public Task () {
            // implicit call to Event()
        }

    }

    public class Downtime : Event
    {
        // transportation downtimes; when downtime is active, the transporter is disabled

        public Downtime() {
            // implicit call to Event()
        }
        
    }

}