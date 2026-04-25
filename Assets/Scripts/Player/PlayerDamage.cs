using UnityEngine;

public class PlayerDamage : MonoBehaviour
{
    public static event System.Action OnPlayerDeath;

    private bool _isDead;

    private void OnTriggerEnter(Collider other)
    {
        if (_isDead) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Bullet"))
        {
            _isDead = true;
            OnPlayerDeath?.Invoke();
            GetComponent<PlayerMovementController>().enabled = false;
            Debug.Log("Player killed!");
        }
    }
}
