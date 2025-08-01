using System.Collections;
using UnityEngine;

public class ThunderAttack : MonoBehaviour
{
    public float knockbackForce = 5f;
    public int damage = 3;

    private void Start()
    {
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(4f); // Duration now controlled by DisasterManager
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