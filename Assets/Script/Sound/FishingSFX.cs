using UnityEngine;
using UnityEngine.Audio;

public class FishingSFX : MonoBehaviour
{
    [Header("Casting Sounds")]
    [SerializeField] private AudioClip[] castingSounds;
    [SerializeField] private float castingVolume = 0.8f;

    [Header("Reeling Sounds")]
    [SerializeField] private AudioClip reelStartSound;
    [SerializeField] private AudioClip reelLoopSound;
    [SerializeField] private AudioClip reelEndSound;
    [SerializeField] private float reelingVolume = 0.7f;

    private AudioSource castingSource;
    [SerializeField] public AudioSource reelingSource;
    [SerializeField] private AudioMixerGroup fishingSFXMixerGroup;
    private void Awake()
    {
        // Setup Audio Sources
        castingSource = gameObject.AddComponent<AudioSource>();
        reelingSource = gameObject.AddComponent<AudioSource>();
        castingSource.outputAudioMixerGroup = fishingSFXMixerGroup;
        reelingSource.outputAudioMixerGroup = fishingSFXMixerGroup;
        // Konfigurasi dasar
        castingSource.playOnAwake = false;
        reelingSource.playOnAwake = false;
        reelingSource.loop = true; // Untuk suara reel yang terus menerus
    }

    public void PlayCastingSound()
    {
        if (castingSounds.Length == 0) return;

        // Memilih suara acak dari array
        AudioClip randomCast = castingSounds[Random.Range(0, castingSounds.Length)];
        castingSource.clip = randomCast;
        castingSource.volume = castingVolume;
        castingSource.Play();
    }

    public void StartReelingSound()
    {
        if (reelStartSound != null)
        {
            reelingSource.PlayOneShot(reelStartSound, reelingVolume);
        }

        if (reelLoopSound != null)
        {
            reelingSource.clip = reelLoopSound;
            reelingSource.volume = reelingVolume;
            reelingSource.PlayDelayed(reelStartSound.length * 0.8f); // Mulai sebelum start sound selesai
        }
    }

    public void StopReelingSound()
    {
        if (reelLoopSound != null)
        {
            reelingSource.Stop();
        }

        if (reelEndSound != null)
        {
            reelingSource.PlayOneShot(reelEndSound, reelingVolume);
        }
    }

    public class FishingController : MonoBehaviour
    {
        [SerializeField] private FishingSFX fishingSFX;

        public void CastFishingRod()
        {
            // Logika melempar pancingan
            fishingSFX.PlayCastingSound();
        }

        public void StartReeling()
        {
            // Logika mulai menarik pancingan
            fishingSFX.StartReelingSound();
        }

        public void StopReeling(bool caughtFish)
        {
            // Logika berhenti menarik
            fishingSFX.StopReelingSound();

            if (caughtFish)
            {
                // Tambahkan SFX tangkapan sukses jika perlu
            }
        }
    }

    public void SetReelingPitch(float pitch)
    {
        if (reelingSource != null)
        {
            reelingSource.pitch = Mathf.Clamp(pitch, 0.5f, 2.0f); // Batasi range pitch
        }
    }

    public void StopAllSounds()
    {
        if (reelingSource != null && reelingSource.isPlaying)
        {
            reelingSource.Stop();
        }

        if (castingSource != null && castingSource.isPlaying)
        {
            castingSource.Stop();
        }
    }
}