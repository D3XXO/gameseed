using UnityEngine;
using UnityEngine.SceneManagement;

public class BigWaveController : MonoBehaviour
{
    private Transform target;
    public float speed;
    public string playerTag = "Player";
    public int waveDamage;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void Update()
    {
        if (target == null) return;

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            BoatController boatController = other.GetComponent<BoatController>();
            if (boatController != null)
            {
                boatController.TakeDamage(waveDamage, true);
            }
            
            Destroy(gameObject);
        }
    }
}