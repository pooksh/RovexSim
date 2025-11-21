using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI; 


public class DisplayValue : MonoBehaviour
{
    public bool isFloat;
    private TMP_Text display;
    private string value;
    private Slider slider;
    private InfoTransfer transferData;
    
    void Start()
    {
        display = GetComponentInChildren<TMP_Text>();
        slider = GetComponent<Slider>();
        if (isFloat) {
            UpdateDisplay(slider.minValue.ToString("0.00"));
        }
        else {
            UpdateDisplay(slider.minValue.ToString());
        }
    }

    public void OnValueChanged(float newValue) {
        if (isFloat) {
            value = newValue.ToString("0.00");
        }
        else {
            value = newValue.ToString();
        }
        UpdateDisplay(value);
    }

    private void UpdateDisplay(string str) {
        if (str != null) {
            display.text = str;
        }
    }

}
