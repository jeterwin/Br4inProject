using System;
using UnityEngine;

public class EnemyDeathHandler : MonoBehaviour
{
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
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }
}
