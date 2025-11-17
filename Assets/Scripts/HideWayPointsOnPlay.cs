using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideWayPointsOnPlay : MonoBehaviour
{

    [SerializeField] private bool showSpheresOnPlay = false;
    [SerializeField] private bool enableDebugLogs = true;
    private MeshRenderer mesh;

    void Start()
    {

        mesh = GetComponentInChildren<MeshRenderer>();

        if (mesh == null) {
            if (enableDebugLogs) {
                Debug.Log("Could not find child object or MeshRenderer on child to hide");
            }
            return;
        }

        if (!showSpheresOnPlay) {
            mesh.enabled = false;
        }
        else {
            mesh.enabled = true;
        }
    }
}
