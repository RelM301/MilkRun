using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerCollision : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private AudioSource coinAudio;
    [SerializeField] private AudioSource deathAudio;

    private GameManager gm;

    void Start()
    {
        gm = Object.FindFirstObjectByType<GameManager>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // 1. HIT ENEMY
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (deathParticles)
            {
                deathParticles.transform.SetParent(null);
                deathParticles.Play();
            }

            // Play death sound and pass its length to GameOver
            float deathClipLength = 0f;
            if (deathAudio && deathAudio.clip != null)
            {
                deathAudio.PlayOneShot(deathAudio.clip);
                deathClipLength = deathAudio.clip.length;
            }

            gm.GameOver(deathClipLength); // Pass the length to GameManager
            Destroy(gameObject);
        }
        // 2. HIT MILK
        else if (collision.gameObject.CompareTag("Coin"))
        {
            ParticleSystem p = collision.gameObject.GetComponentInChildren<ParticleSystem>();
            if (p)
            {
                p.transform.SetParent(null);
                p.Play();
                Destroy(p.gameObject, 1.5f);
            }
            if (coinAudio) coinAudio.PlayOneShot(coinAudio.clip);
            gm.AddScore();
            Destroy(collision.gameObject);
        }
    }
}
