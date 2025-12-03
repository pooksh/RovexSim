using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI; 

public class InfoUpdater : MonoBehaviour
{
    private InfoTransfer transferData;
    [SerializeField] public enum OptionVariables { NumRovi, NumPorters, RoviSpeed }   
    [SerializeField] private OptionVariables optVar;
    private delegate void ChosenMethod(float var);
    ChosenMethod currentMethod;
    private DisplayValue display;
    private Slider slider;

    void Start() {
        slider = GetComponent<Slider>();
        display = GetComponent<DisplayValue>();
        if (display == null) {
            Debug.LogError("Add a display value component to this slider object.");
        }

        transferData = (InfoTransfer)FindObjectOfType(typeof(InfoTransfer));
        if (transferData == null) {
            Debug.LogError("Object which transfers information between scenes cannot be found");
        }

        switch (optVar) {
            case OptionVariables.NumRovi: currentMethod = UpdateNumRovi; break;
            case OptionVariables.NumPorters: currentMethod = UpdateNumPorters; break;
            case OptionVariables.RoviSpeed: currentMethod = UpdateRoviSpeed; break;
        }

        OnValueChanged(slider.minValue);
    }    

    public void OnValueChanged(float newValue) {
        if (display.isFloat) {
            currentMethod((float)newValue);
        }
        else {
            currentMethod((int)newValue);
        }
    }

    public void UpdateNumRovi(int n) {
        if (SceneManager.GetActiveScene().name == "OptionsScene") {
            transferData.PassNumRovi(n);
        }
    }

    public void UpdateNumRovi(float n) {
        UpdateNumRovi((int)n);
    }

    public void UpdateNumPorters(int n) {
        if (SceneManager.GetActiveScene().name == "OptionsScene") {
            transferData.PassNumPorters(n);
        }
    }

    public void UpdateNumPorters(float n) {
        UpdateNumPorters((int)n);
    }

    public void UpdateRoviSpeed(float s) {
        if (SceneManager.GetActiveScene().name == "OptionsScene") {
            transferData.PassRoviSpeed(s);
        }
    }

    public void UpdateMap(string m) {
        if (SceneManager.GetActiveScene().name == "OptionsScene") {
            transferData.PassMap(m);
        }
    }
}
