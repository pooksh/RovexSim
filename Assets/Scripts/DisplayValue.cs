using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI; 


public class DisplayValue : MonoBehaviour
{
    [SerializeField] private bool isAMRCount;
    private TMP_Text display;
    private string value;
    private Slider slider;
    private InfoTransfer transferData;
    
    void Start()
    {
        transferData = (InfoTransfer)FindObjectOfType(typeof(InfoTransfer));
        if (transferData == null) {
            Debug.LogError("Object which transfers information between scenes cannot be found");
        }
        display = GetComponentInChildren<TMP_Text>();
        slider = GetComponent<Slider>();
        if (isAMRCount) {
            UpdateDisplay(slider.minValue.ToString());
            transferData.UpdateNumRovi((int)slider.minValue);
        }
        else {
            UpdateDisplay(slider.minValue.ToString("0.00"));
            transferData.UpdateSpeed(slider.minValue);
        }

    }

    public void OnValueChanged(float newValue) {
        if (isAMRCount) {
            value = newValue.ToString();
            transferData.UpdateNumRovi((int)newValue);
        }
        else {
            value = newValue.ToString("0.00");
            transferData.UpdateSpeed(newValue);
        }
        UpdateDisplay(value);
    }

    private void UpdateDisplay(string str) {
        if (str != null) {
            display.text = str;
        }
    }

}
