using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskGenerator : MonoBehaviour
{
    [SerializeField] public TextAsset file;
    [SerializeField] public WaypointManager wpmgr;
    [SerializeField] public Transform[] originWaypoints;
    [SerializeField] public Transform[] destinationWaypoints;
    // TaskGenerator handles the rest
}
