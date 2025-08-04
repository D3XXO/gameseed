using UnityEngine;
using UnityEngine.Audio;

public class AmbienceController : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioMixerGroup ambienceMixerGroup;
    [SerializeField] private AudioClip[] ambienceClips;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;

    private AudioSource audioSource;

    private void Start()
    {
        // Setup AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = ambienceMixerGroup;
        audioSource.loop = loop;

        if (playOnStart && ambienceClips.Length > 0)
        {
            PlayRandomAmbience();
        }
    }

    public void PlayRandomAmbience()
    {
        if (ambienceClips.Length == 0) return;

        AudioClip clip = ambienceClips[Random.Range(0, ambienceClips.Length)];
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void SetVolume(float volume)
    {
        // Convert linear volume (0-1) to dB (-80 to 0)
        float dB = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
        ambienceMixerGroup.audioMixer.SetFloat("AmbienceVolume", dB);
    }
}