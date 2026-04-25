using System;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private int _enemyCount = 6;
    [SerializeField] private float _spawnYOffset = 1f;
    [SerializeField] private BCITargetingSystem _targetingSystem;

    public event Action OnAllEnemiesDefeated;
    public int AliveCount => _aliveCount;
    public int TotalCount => _totalCount;

    private int _aliveCount;
    private int _totalCount;

    [SerializeField] private bool _spawnOnStart = true;

    private const float EdgeExtent = 13f;

    private void Start()
    {
        if (_spawnOnStart)
            SpawnWave();
    }

    public void SpawnWave()
    {
        _totalCount = _enemyCount;
        _aliveCount = _enemyCount;

        for (int i = 0; i < _enemyCount; i++)
        {
            Vector3 position = GetRandomEdgePosition();
            GameObject enemy = Instantiate(_enemyPrefab, position, Quaternion.identity);
            int classId = i + 1;
            enemy.GetComponent<EnemyController>().ClassId = classId;

            if (_targetingSystem != null)
                _targetingSystem.RegisterEnemy(classId, enemy);

            if (BCIPipelineOrchestrator.Instance != null)
                BCIPipelineOrchestrator.Instance.RegisterFlashTarget(classId, enemy);
        }
    }

    private void OnEnemyDied(EnemyDeathHandler enemy)
    {
        int classId = enemy.GetComponent<EnemyController>().ClassId;
        _aliveCount--;

        if (_targetingSystem != null)
            _targetingSystem.UnregisterEnemy(classId);

        if (BCIPipelineOrchestrator.Instance != null)
            BCIPipelineOrchestrator.Instance.UnregisterFlashTarget(classId);

        if (_aliveCount <= 0)
            OnAllEnemiesDefeated?.Invoke();
    }

    private Vector3 GetRandomEdgePosition()
    {
        int edge = UnityEngine.Random.Range(0, 4);
        float t = UnityEngine.Random.Range(-EdgeExtent, EdgeExtent);

        return edge switch
        {
            0 => new Vector3(t, _spawnYOffset, EdgeExtent),
            1 => new Vector3(t, _spawnYOffset, -EdgeExtent),
            2 => new Vector3(EdgeExtent, _spawnYOffset, t),
            _ => new Vector3(-EdgeExtent, _spawnYOffset, t)
        };
    }

    private void OnEnable()
    {
        EnemyDeathHandler.OnDeath += OnEnemyDied;
    }

    private void OnDisable()
    {
        EnemyDeathHandler.OnDeath -= OnEnemyDied;
    }
}
