using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitOnClick : MonoBehaviour
{

    [SerializeField] private bool enableDebugLogs = true;

    public void Quit()
    {
        Application.Quit();
        if (enableDebugLogs) {
            Debug.Log("Quitting...");
        }
    }
}
