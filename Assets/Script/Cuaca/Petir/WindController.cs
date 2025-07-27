using System.Collections;
using UnityEngine;

public class WindController : MonoBehaviour
{
    public ParticleSystem rainParticles;
    public float maxWindForce = 5f;
    public float windChangeInterval = 10f;

    void Start()
    {
        StartCoroutine(ChangeWindDirection());
    }

    IEnumerator ChangeWindDirection()
    {
        while (true)
        {
            float windForce = Random.Range(-maxWindForce, maxWindForce);
            var forceOverLifetime = rainParticles.forceOverLifetime;
            forceOverLifetime.x = windForce;

            yield return new WaitForSeconds(windChangeInterval);
        }
    }
}