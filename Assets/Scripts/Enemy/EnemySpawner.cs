using System;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private int _enemyCount = 6;
    [SerializeField] private float _spawnYOffset = 1f;

    public event Action OnAllEnemiesDefeated;
    public int AliveCount => _aliveCount;
    public int TotalCount => _totalCount;

    private int _aliveCount;
    private int _totalCount;

    private const float EdgeExtent = 13f;

    public void SpawnWave()
    {
        _totalCount = _enemyCount;
        _aliveCount = _enemyCount;

        for (int i = 0; i < _enemyCount; i++)
        {
            Vector3 position = GetRandomEdgePosition();
            GameObject enemy = Instantiate(_enemyPrefab, position, Quaternion.identity);
            enemy.GetComponent<EnemyController>().ClassId = i + 1;

            var deathHandler = enemy.GetComponent<EnemyDeathHandler>();
            if (deathHandler != null)
                deathHandler.OnDeath += OnEnemyDied;
        }
    }

    private void OnEnemyDied(EnemyDeathHandler enemy)
    {
        enemy.OnDeath -= OnEnemyDied;
        _aliveCount--;

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
}
