using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RoviManager : MonoBehaviour, ITransporterManager
{
    [SerializeField] private bool usingOptions = true;
    [SerializeField] private GameObject roviPrefab;
    [SerializeField] private bool enableDebugLogs;
    public int numRovi = 1;
    public float speed = 2.5f;
    public string map = "UFMap2";
    private List<GameObject> rovis;
    private InfoTransfer transferData;
    private bool initialized = false;

    void Awake()
    {
        if (roviPrefab == null) {
            Debug.LogError("Please reference a Rovi prefab in the inspector.");
        }

        if (usingOptions) {
            transferData = (InfoTransfer)FindObjectOfType(typeof(InfoTransfer));
            if (transferData == null) {
                Debug.LogError("Object with InfoTransfer component (which transfers information between scenes) cannot be found. Using defaults.");
            }
            else {
                if (transferData.GetNumRovi() > 0) {
                    numRovi = transferData.GetNumRovi(); 
                }
                if (transferData.GetRoviSpeed() > 0) {
                    speed = transferData.GetRoviSpeed();
                }
                if (transferData.GetMap() != null) {
                    map = transferData.GetMap();
                }
            }
        }
        
        GameObject parent = GameObject.Find("RoviTransporters");
        if (parent == null) {
            Debug.Log("Could not find RoviTransporters object to add Rovi under, will add to root.");
        }

        rovis = new List<GameObject>();
        for (int i = 0; i < numRovi; i++) {
            GameObject obj = Instantiate(roviPrefab);
            if (parent != null)
            {
                obj.transform.SetParent(parent.transform, true);
            }
            obj.name = $"Rovi ({i})";
            
            RoviTransporter newRovi = obj.GetComponent<RoviTransporter>();
            newRovi.InitializeTransporter(speed);
            rovis.Add(obj);
        }
        initialized = true;
    }

    public List<GameObject> GetTransporters() {
        if (initialized) {
            return rovis;
        }
        else {
            Debug.LogError($"Rovis failed to initialize.");
            return null;
        }
    }

    public bool IsInitialized() {
        return initialized;
    }

}
