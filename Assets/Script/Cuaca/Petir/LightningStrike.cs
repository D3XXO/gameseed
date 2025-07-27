using UnityEngine;
using System.Collections;

public class LightningStrike : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 15;
    public float warningTime = 1.5f; // Time showing warning before strike
    public float activeTime = 0.5f; // Time damage is active
    public float effectDuration = 1f; // Total time before disappearing

    [Header("Visual/Audio")]
    public GameObject warningEffect; // The warning circle
    public GameObject lightningEffect; // The lightning visual
    public AudioClip thunderSound;
    public ParticleSystem waterSplash;

    private CircleCollider2D damageArea;
    private AudioSource audioSource;

    void Awake()
    {
        damageArea = GetComponent<CircleCollider2D>();
        audioSource = GetComponent<AudioSource>();
        damageArea.enabled = false; // Start with damage off
    }

    public void Strike(Vector2 position)
    {
        StartCoroutine(LightningRoutine(position));
    }

    IEnumerator LightningRoutine(Vector2 position)
    {
        // Position the lightning strike
        transform.position = position;

        // 1. Show warning first
        GameObject warning = Instantiate(warningEffect, position, Quaternion.identity);
        yield return new WaitForSeconds(warningTime);
        Destroy(warning);

        // 2. Activate lightning strike
        damageArea.enabled = true;
        GameObject lightning = Instantiate(lightningEffect, position, Quaternion.identity);
        if (waterSplash != null) Instantiate(waterSplash, position, Quaternion.identity);
        audioSource.PlayOneShot(thunderSound);

        // 3. Keep active for damage duration
        yield return new WaitForSeconds(activeTime);
        damageArea.enabled = false;

        // 4. Wait remaining time (so total is effectDuration)
        float remainingTime = effectDuration - activeTime;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        // 5. Clean up everything
        Destroy(lightning);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<BoatController>(out var boat))
        {
            boat.TakeDamage(damage);

            // Optional knockback
            Vector2 pushDirection = (other.transform.position - transform.position).normalized;
            other.GetComponent<Rigidbody2D>().AddForce(pushDirection * 5f, ForceMode2D.Impulse);
        }
    }
}