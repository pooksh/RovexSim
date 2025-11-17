using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InfoTransfer : MonoBehaviour
{

    public int numRovis = 0;
    public float speed = 0;
    public string map;
    [SerializeField] private bool enableDebugLogs = false;

    void Awake() {
        DontDestroyOnLoad(this.gameObject);
    }

    public void UpdateNumRovi(int n) {
        if (SceneManager.GetActiveScene().name == "OptionsScene") {
            numRovis = n;
            if (enableDebugLogs) {
                Debug.Log($"Number of Rovis currently is: {numRovis}");
            }
        }
    }

    public void UpdateSpeed(float s) {
        if (SceneManager.GetActiveScene().name == "OptionsScene") {
            speed = s;
            if (enableDebugLogs) {
                Debug.Log($"Speed currently is: {speed}");
            }
        }
    }

    public void UpdateMap(string m) {
        if (SceneManager.GetActiveScene().name == "OptionsScene") {
            map = m;
            if (enableDebugLogs) {
                Debug.Log($"Map currently is: {map}");
            }
        }
    }
}
