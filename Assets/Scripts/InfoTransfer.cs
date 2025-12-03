using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoTransfer : MonoBehaviour
{

    private int numRovi = 0;
    private int numPorters = 0;
    private float speed = 0;
    private string map;
    [SerializeField] private bool enableDebugLogs = false;

    void Awake() {
        DontDestroyOnLoad(this.gameObject);
    }

    public void PassNumRovi(int n) {
        numRovi = n;
        if (enableDebugLogs) {
            Debug.Log($"Number of Rovis currently is: {numRovi}");
        }
    }

    public void PassNumPorters(int n) {
        numPorters = n;
        if (enableDebugLogs) {
            Debug.Log($"Number of Human Transporters currently is: {numPorters}");
        }
    }
    
    public void PassRoviSpeed(float s) {
        speed = s;
        if (enableDebugLogs) {
            Debug.Log($"Speed currently is: {speed}");
        }
    }

    public void PassMap(string m) {
        map = m;
        if (enableDebugLogs) {
            Debug.Log($"Map currently is: {map}");
        }
    }

    public string GetMap() {
        return map;
    }

    public float GetRoviSpeed() {
        return speed;
    }

     public int GetNumRovi() {
        return numRovi;
     }

     public int GetNumPorters() {
        return numPorters;
     }

}
