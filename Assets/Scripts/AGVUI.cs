using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minimal UI hookup: dropdown to choose number of AGVs (1-3), Start and Stop buttons.
/// Add this to a UI GameObject and assign the AGVManager reference and Unity UI components.
/// </summary>
public class AGVUI : MonoBehaviour
{
    public AGVManager Manager;
    public Dropdown CountDropdown; // populate with ["1","2","3"]
    public Button StartButton;
    public Button StopButton;
    public Text StatusText;

    void Start()
    {
        if (CountDropdown != null)
        {
            CountDropdown.options.Clear();
            CountDropdown.options.Add(new Dropdown.OptionData("1"));
            CountDropdown.options.Add(new Dropdown.OptionData("2"));
            CountDropdown.options.Add(new Dropdown.OptionData("3"));
            CountDropdown.value = Mathf.Clamp(Manager != null ? Manager.MaxAgvs - 1 : 0, 0, 2);
            CountDropdown.onValueChanged.AddListener(OnCountChanged);
        }

        if (StartButton != null) StartButton.onClick.AddListener(OnStartClicked);
        if (StopButton != null) StopButton.onClick.AddListener(OnStopClicked);
    }

    void Update()
    {
        if (StatusText != null && Manager != null)
        {
            StatusText.text = Manager.GetStatusReport();
        }
    }

    public void OnCountChanged(int index)
    {
        int count = index + 1;
        if (Manager != null)
        {
            Manager.CreateAgvs(count);
        }
    }

    public void OnStartClicked()
    {
        Manager?.StartSimulation();
    }

    public void OnStopClicked()
    {
        Manager?.StopSimulation();
    }
}
