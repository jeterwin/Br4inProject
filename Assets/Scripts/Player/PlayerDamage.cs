using UnityEngine;

public class PlayerDamage : MonoBehaviour
{
    public static event System.Action OnPlayerDeath;

    [SerializeField] private PlayerMovementController playerMovementController;
    [SerializeField] private CameraRotationController cameraRotController;
    [SerializeField] private GunMovementController gunMovementController;

    private bool _isDead;

    private void OnTriggerEnter(Collider other)
    {
        if (_isDead) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Bullet"))
        {
            _isDead = true;
            OnPlayerDeath?.Invoke();
            playerMovementController.enabled = false;
            cameraRotController.enabled = false;
            gunMovementController.enabled = false;
            Debug.Log("Player killed!");
        }
    }
}
