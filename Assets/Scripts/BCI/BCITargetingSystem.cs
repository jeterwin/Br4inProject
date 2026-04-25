using System;
using System.Collections.Generic;
using UnityEngine;

public class BCITargetingSystem : MonoBehaviour
{
    [SerializeField] private int _requiredDetections = 4;
    [SerializeField] private float _windowDuration = 5f;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _bulletSpeed = 20f;
    [SerializeField] private Transform _fireOrigin;
    [SerializeField] private bool _debugMode = true;

    private Dictionary<int, List<float>> _detectionTimestamps = new();
    private Dictionary<int, GameObject> _classToEnemy = new();

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
        int id = (int)classId;

        if (!_classToEnemy.TryGetValue(id, out GameObject enemy) || enemy == null)
            return;

        List<float> timestamps = _detectionTimestamps[id];
        timestamps.Add(Time.unscaledTime);

        float cutoff = Time.unscaledTime - _windowDuration;
        timestamps.RemoveAll(t => t < cutoff);

        if (timestamps.Count >= _requiredDetections)
        {
            Debug.Log($"[BCI] Firing at enemy classId={id}");
            FireAtEnemy(enemy);
            timestamps.Clear();
        }
    }

    private void FireAtEnemy(GameObject enemy)
    {
        Vector3 direction = (enemy.transform.position - _fireOrigin.position).normalized;
        Vector3 spawnPos = _fireOrigin.position + direction * 1.5f;
        GameObject bullet = Instantiate(_bulletPrefab, spawnPos, Quaternion.identity);
        bullet.GetComponent<Bullet>().Initialize(direction, _bulletSpeed);
    }
}
