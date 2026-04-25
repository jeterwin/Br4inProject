using System;
using UnityEngine;

public class EnemyDeathHandler : MonoBehaviour
{
    [SerializeField] private GameObject _deathParticlePrefab;

    public event Action<EnemyDeathHandler> OnDeath;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bullet"))
        {
            Destroy(other.gameObject);
            Die();
        }
    }

    public void Die()
    {
        if (_deathParticlePrefab != null)
            Instantiate(_deathParticlePrefab, transform.position, Quaternion.identity);

        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }
}
