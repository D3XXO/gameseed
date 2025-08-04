using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    [Header("Audio Mixer References")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Ambience Controller")]
    [SerializeField] private AmbienceController ambienceController;

    [Header("UI References")]
    [SerializeField] private Slider ambienceSlider;
    [SerializeField] private Slider fishingSFXSlider;
    [SerializeField] private Button applyButton;

    [Header("Volume Parameters")]
    [SerializeField] private string ambienceVolumeParam = "AmbienceVolume";
    [SerializeField] private string fishingSFXVolumeParam = "FishingSFXVolume";
    [SerializeField] private float defaultAmbienceVolume = 0.8f;
    [SerializeField] private float defaultFishingSFXVolume = 1f;

    private void Start()
    {
        // Hubungkan slider dengan ambience controller
        ambienceSlider.onValueChanged.AddListener(SetAmbienceVolume);
    }
    private void Awake()
    {
        // Load saved settings
        ambienceSlider.value = PlayerPrefs.GetFloat(ambienceVolumeParam, defaultAmbienceVolume);
        fishingSFXSlider.value = PlayerPrefs.GetFloat(fishingSFXVolumeParam, defaultFishingSFXVolume);

        // Apply initial settings
        SetAmbienceVolume(ambienceSlider.value);
        SetFishingSFXVolume(fishingSFXSlider.value);

        // Setup button listener
        applyButton.onClick.AddListener(SaveSettings);
    }

    public void SetAmbienceVolume(float volume)
    {
        // Update Audio Mixer
        float dB = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
        audioMixer.SetFloat("AmbienceVolume", dB);

        // Update langsung ke AmbienceController (opsional)
        if (ambienceController != null)
        {
            ambienceController.SetVolume(volume);
        }
    }

    public void SetFishingSFXVolume(float volume)
    {
        float dB = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
        audioMixer.SetFloat(fishingSFXVolumeParam, dB);
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat(ambienceVolumeParam, ambienceSlider.value);
        PlayerPrefs.SetFloat(fishingSFXVolumeParam, fishingSFXSlider.value);
        PlayerPrefs.Save();

        Debug.Log("Audio settings saved!");
    }
}