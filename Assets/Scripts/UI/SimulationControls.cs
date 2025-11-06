using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI controls for simulation time management.
/// Handles start, stop, reset, and speed adjustments.
/// Attach this to a Canvas panel with UI elements.
/// </summary>
public class SimulationControls : MonoBehaviour
{
    [Header("UI References")]
    public Button StartButton;
    public Button StopButton;
    public Button ResetButton;
    public Slider SpeedSlider;
    public Text SpeedLabel;
    public Text StatusLabel;

    private SimulationTimeManager sim;

    void Start()
    {
        sim = SimulationTimeManager.Instance;

        // Setup button events
        if (StartButton != null) StartButton.onClick.AddListener(OnStartClicked);
        if (StopButton != null) StopButton.onClick.AddListener(OnStopClicked);
        if (ResetButton != null) ResetButton.onClick.AddListener(OnResetClicked);

        // Setup speed slider
        if (SpeedSlider != null)
        {
            SpeedSlider.minValue = 0.1f;
            SpeedSlider.maxValue = 5f;
            SpeedSlider.value = 1f;
            SpeedSlider.onValueChanged.AddListener(OnSpeedChanged);
        }

        UpdateUI();
    }

    void Update()
    {
        if (sim != null && StatusLabel != null)
        {
            string state = sim.IsRunning ? "Running" : "Paused";
            StatusLabel.text = $"Status: {state}\nTime: {sim.CurrentTime:F1}s";
        }

        if (SpeedLabel != null)
            SpeedLabel.text = $"Speed: {sim?.TimeScale:F1}x";
    }

    private void OnStartClicked()
    {
        if (sim == null) return;

        if (!sim.IsRunning)
        {
            sim.Resume();
            Debug.Log("[SimulationControls] Simulation started/resumed.");
        }
        else
        {
            Debug.Log("[SimulationControls] Simulation already running.");
        }

        UpdateUI();
    }

    private void OnStopClicked()
    {
        if (sim == null) return;

        if (sim.IsRunning)
        {
            sim.Pause();
            Debug.Log("[SimulationControls] Simulation paused.");
        }
        else
        {
            Debug.Log("[SimulationControls] Simulation already paused.");
        }

        UpdateUI();
    }

    private void OnResetClicked()
    {
        if (sim == null) return;

        sim.Stop(); // resets time and clears events
        sim.StartSimulation(); // restarts fresh

        Debug.Log("[SimulationControls] Simulation reset.");
        UpdateUI();
    }

    private void OnSpeedChanged(float value)
    {
        if (sim != null)
            sim.TimeScale = value;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (SpeedLabel != null && sim != null)
            SpeedLabel.text = $"Speed: {sim.TimeScale:F1}x";

        if (StatusLabel != null && sim != null)
            StatusLabel.text = sim.IsRunning ? "Status: Running" : "Status: Paused";
    }
}
