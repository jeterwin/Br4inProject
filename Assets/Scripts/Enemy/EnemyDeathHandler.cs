using System;
using UnityEngine;

public class EnemyDeathHandler : MonoBehaviour
{
    [SerializeField] private GameObject _deathParticlePrefab;
    [SerializeField] private int _scoreValue = 100;

    public int ScoreValue => _scoreValue;

    public static event Action<EnemyDeathHandler> OnDeath;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))
        {
            Destroy(other.gameObject);
            Die();
        }
    }

    public void Die()
    {
        if (_deathParticlePrefab != null)
        {
            Instantiate(_deathParticlePrefab, transform.position, Quaternion.identity);
        }

        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }
}