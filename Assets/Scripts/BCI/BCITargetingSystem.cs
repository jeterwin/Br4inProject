using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class BCITargetingSystem : MonoBehaviour
{
    [SerializeField] private int _requiredDetections = 4;
    [SerializeField] private bool _debugMode = true;
    [SerializeField] private float _windowDuration = 5f;
    [SerializeField] private float _bulletSpeed = 20f;
    [SerializeField] private float _fireCooldown = 2.0f;

    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _fireOrigin;
    [SerializeField] private ParticleSystem shotSFX;

    [Header("Reticle Settings")]
    [SerializeField] private RectTransform _reticle;
    [SerializeField] private float _readyScaleAmount = 1.3f;
    [SerializeField] private float _readyPulseDuration = 0.15f;

    private Dictionary<int, List<float>> _detectionTimestamps = new();
    private Dictionary<int, GameObject> _classToEnemy = new();
    private float _nextFireTime;
    private Coroutine _reticleCoroutine;

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
        if (Time.deltaTime < _nextFireTime) return;

        int id = (int)classId;

        if (!_classToEnemy.TryGetValue(id, out GameObject enemy) || enemy == null)
            return;

        List<float> timestamps = _detectionTimestamps[id];
        timestamps.Add(Time.deltaTime);

        float cutoff = Time.deltaTime - _windowDuration;
        timestamps.RemoveAll(t => t < cutoff);

        if (timestamps.Count >= _requiredDetections)
        {
            _nextFireTime = Time.deltaTime + _fireCooldown;
            FireAtEnemy(enemy);
            ResetAllDetections();

            if (_reticleCoroutine != null) StopCoroutine(_reticleCoroutine);
            _reticleCoroutine = StartCoroutine(AnimateReticle());
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

        var gunSway = _fireOrigin.GetComponentInParent<GunMovementController>();
        if (gunSway != null) gunSway.Shoot();

        Vector3 direction = (enemy.transform.position - _fireOrigin.position).normalized;
        Vector3 spawnPos = _fireOrigin.position + direction * 1.5f;
        GameObject bullet = Instantiate(_bulletPrefab, spawnPos, Quaternion.identity);
        bullet.GetComponent<Bullet>().Initialize(direction, _bulletSpeed);
    }

    private IEnumerator AnimateReticle()
    {
        if (_reticle == null) yield break;

        float elapsed = 0f;
        while (elapsed < _fireCooldown)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _fireCooldown;
            float rotation = Mathf.Lerp(0, 180f, t);
            _reticle.localRotation = Quaternion.Euler(0, 0, rotation);
            yield return null;
        }

        _reticle.localRotation = Quaternion.Euler(0, 0, 180f);

        elapsed = 0f;
        while (elapsed < _readyPulseDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _readyPulseDuration;
            float scale = Mathf.Lerp(1f, _readyScaleAmount, t);
            _reticle.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < _readyPulseDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _readyPulseDuration;
            float scale = Mathf.Lerp(_readyScaleAmount, 1f, t);
            _reticle.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        _reticle.localScale = Vector3.one;
        _reticle.localRotation = Quaternion.identity;
    }
}