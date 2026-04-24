using System;
using UnityEngine;

public class EnemyDeathHandler : MonoBehaviour
{
    public event Action<EnemyDeathHandler> OnDeath;

    public void Die()
    {
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }
}
