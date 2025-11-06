using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI control for simulation time manager.
/// Attach to a Canvas panel with Text and Slider controls.
/// </summary>
public class SimulationUI : MonoBehaviour
{
    public Text TimeText;
    public Slider SpeedSlider;
    public Button PauseButton;
    public Button ResumeButton;
    public Button ResetButton;

    void Start()
    {
        if (SpeedSlider != null)
        {
            SpeedSlider.minValue = 0f;
            SpeedSlider.maxValue = 10f;
            SpeedSlider.value = 1f;
            SpeedSlider.onValueChanged.AddListener(OnSpeedChanged);
        }

        if (PauseButton != null) PauseButton.onClick.AddListener(() => SimulationTimeManager.Instance?.Pause());
        if (ResumeButton != null) ResumeButton.onClick.AddListener(() => SimulationTimeManager.Instance?.Resume());
        if (ResetButton != null) ResetButton.onClick.AddListener(() => SimulationTimeManager.Instance?.Stop());
    }

    void Update()
    {
        var mgr = SimulationTimeManager.Instance;
        if (mgr != null && TimeText != null)
        {
            TimeText.text = $"Sim Time: {mgr.CurrentTime:F1}s | Rate: {mgr.TimeScale:F1}x";
        }
    }

    void OnSpeedChanged(float val)
    {
        var mgr = SimulationTimeManager.Instance;
        if (mgr != null)
            mgr.TimeScale = val;
    }
}
