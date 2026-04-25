using System;
using System.Collections.Generic;
using UnityEngine;

public class BCITargetingSystem : MonoBehaviour
{
    public static BCITargetingSystem Instance { get; private set; }

    [SerializeField] private int _requiredDetections = 4;
    [SerializeField] private bool _debugMode = true;
    [SerializeField] private float _windowDuration = 5f;
    [SerializeField] private float _bulletSpeed = 20f;
    [SerializeField] private float _fireCooldown = 1.0f;

    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _fireOrigin;
    [SerializeField] private ParticleSystem shotSFX;

    private Dictionary<int, List<float>> _detectionTimestamps = new();
    private Dictionary<int, GameObject> _classToEnemy = new();
    private float _nextFireTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public IReadOnlyDictionary<int, GameObject> ClassToEnemy => _classToEnemy;

    public void RegisterEnemy(int classId, GameObject enemy)
    {
        _classToEnemy[classId] = enemy;
        _detectionTimestamps[classId] = new List<float>();
    }

    public void UnregisterEnemy(int classId)
    {
        _classToEnemy.Remove(classId);
        _detectionTimestamps.Remove(classId);
    }

    public void OnClassSelected(uint classId)
    {
        if (Time.time < _nextFireTime) return;

        int id = (int)classId;

        if (!_classToEnemy.TryGetValue(id, out GameObject enemy) || enemy == null)
            return;

        List<float> timestamps = _detectionTimestamps[id];
        timestamps.Add(Time.unscaledTime);

        float cutoff = Time.unscaledTime - _windowDuration;
        timestamps.RemoveAll(t => t < cutoff);

        if (timestamps.Count >= _requiredDetections)
        {
            _nextFireTime = Time.time + _fireCooldown;
            FireAtEnemy(enemy);
            ResetAllDetections();
        }
    }

    private void ResetAllDetections()
    {
        foreach (var list in _detectionTimestamps.Values)
        {
            list.Clear();
        }
    }

    private void FireAtEnemy(GameObject enemy)
    {
        if (shotSFX != null) shotSFX.Play();

        // Find the gun controller and trigger recoil
        var gunSway = _fireOrigin.GetComponentInParent<GunMovementController>();
        if (gunSway != null) gunSway.Shoot();

        Vector3 direction = (enemy.transform.position - _fireOrigin.position).normalized;
        Vector3 spawnPos = _fireOrigin.position + direction * 1.5f;
        GameObject bullet = Instantiate(_bulletPrefab, spawnPos, Quaternion.identity);
        bullet.GetComponent<Bullet>().Initialize(direction, _bulletSpeed);
    }
}