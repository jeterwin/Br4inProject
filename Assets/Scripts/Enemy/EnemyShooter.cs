using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _fireInterval = 2f;
    [SerializeField] private float _bulletSpeed = 8f;

    private Transform _playerTransform;
    private float _timer;

    private void Start()
    {
        _playerTransform = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _fireInterval)
        {
            Fire();
            _timer = 0f;
        }
    }

    private void Fire()
    {
        Vector3 direction = (_playerTransform.position - transform.position).normalized;
        Vector3 spawnPos = transform.position + direction * 1.5f;
        GameObject bulletObj = Instantiate(_bulletPrefab, spawnPos, Quaternion.identity);
        bulletObj.GetComponent<Bullet>().Initialize(direction, _bulletSpeed);
    }
}
