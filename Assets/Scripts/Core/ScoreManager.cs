using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreText;

    private int _currentScore = 0;
    private int _enemyKills = 0;
    private float _startTime;

    public int CurrentScore => _currentScore;
    public int EnemyKills => _enemyKills;
    public float TimeAlive => Time.time - _startTime;

    private void OnEnable()
    {
        EnemyDeathHandler.OnDeath += AddScore;
    }

    private void OnDisable()
    {
        EnemyDeathHandler.OnDeath -= AddScore;
    }

    private void Start()
    {
        _startTime = Time.time;
        UpdateUI();
    }

    private void AddScore(EnemyDeathHandler enemy)
    {
        _currentScore += enemy.ScoreValue;
        _enemyKills++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_scoreText != null)
        {
            _scoreText.text = $"Score: {_currentScore}";
        }
    }
}