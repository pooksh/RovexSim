using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimulationEvents;

public class RoviTransporter : Transporter
{

    public override void InitializeTransporter(float speed) { // called in start to initialize this transporter
        
    } 

    public override void HandleDowntime() {
        
    }
    
    public override void HandleTask() {

    }

    void Start() {
        InitializeTransporter(speed);
    }

    void Update() {

    }

}
