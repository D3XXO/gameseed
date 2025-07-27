using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightningEffect : MonoBehaviour
{
    [Header("Light Settings")]
    public Light2D globalLight;
    public float flashIntensity = 0.8f;
    public float flashDuration = 0.2f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip[] thunderClips;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    public void TriggerLightningEffect()
    {
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        // Light Flash
        float originalIntensity = globalLight.intensity;
        globalLight.intensity = flashIntensity;

        // Play randomized thunder sound
        if (audioSource != null && thunderClips.Length > 0)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(thunderClips[Random.Range(0, thunderClips.Length)]);
        }

        yield return new WaitForSeconds(flashDuration);
        globalLight.intensity = originalIntensity;
    }
}