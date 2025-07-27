using UnityEngine;
using System.Collections;

public class ThunderAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float activeDuration = 4f;
    public float knockbackForce = 5f;
    public int damage = 3;

    private void Start()
    {
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(activeDuration);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            BoatController boat = other.GetComponent<BoatController>();
            if (boat != null)
            {
                Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                boat.ApplyKnockback(knockbackDirection, knockbackForce);
                boat.TakeDamage(damage);
            }
        }
    }
}