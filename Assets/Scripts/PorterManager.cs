using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PorterManager : MonoBehaviour, ITransporterManager
{
    [SerializeField] private bool usingOptions = false;
    [SerializeField] private GameObject porterPrefab;
    [SerializeField] private bool enableDebugLogs;
    public int numPorters = 1;
    public float speed = 1.5f;
    public string map = "UFMap2";
    private List<GameObject> porters;
    private InfoTransfer transferData;
    private bool initialized = false;

    void Awake()
    {
        if (porterPrefab == null) {
            Debug.LogError("Please reference a Porter prefab in the inspector.");
            return;
        }

        if (usingOptions) {
            transferData = (InfoTransfer)FindObjectOfType(typeof(InfoTransfer));
            if (transferData == null) {
                Debug.LogError("Object with InfoTransfer component (which transfers information between scenes) cannot be found. Using defaults.");
            }
            else {
                if (transferData.GetNumPorters() > 0) {
                    numPorters = transferData.GetNumPorters(); 
                }
                if (transferData.GetRoviSpeed() > 0) {
                    speed = transferData.GetRoviSpeed();
                }
                if (transferData.GetMap() != null) {
                    map = transferData.GetMap();
                }
            }
        }

        GameObject parent = GameObject.Find("PorterTransporters");
        if (parent == null) {
            if (enableDebugLogs) {
                Debug.Log("Could not find PorterTransporters object to add Porters under, will add to root.");
            }
        }

        porters = new List<GameObject>();
        for (int i = 0; i < numPorters; i++) {
            GameObject obj = Instantiate(porterPrefab);
            if (parent != null)
            {
                obj.transform.SetParent(parent.transform, true);
            }
            obj.name = $"Porter ({i})";
            
            PorterTransporter newPorter = obj.GetComponent<PorterTransporter>();
            if (newPorter == null) {
                Debug.LogError($"Porter prefab {porterPrefab.name} does not have a PorterTransporter component!");
                Destroy(obj);
                continue;
            }
            newPorter.InitializeTransporter(speed);
            porters.Add(obj);
        }
        initialized = true;
        
        if (enableDebugLogs) {
            Debug.Log($"PorterManager initialized with {porters.Count} porters.");
        }
    }

    public List<GameObject> GetTransporters() {
        if (initialized) {
            return porters;
        }
        else {
            Debug.LogError($"Porters failed to initialize.");
            return null;
        }
    }

    public bool IsInitialized() {
        return initialized;
    }

}



