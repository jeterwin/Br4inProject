using UnityEngine;
using TMPro;
using System;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private ScoreManager _scoreManager;

    [Header("Display Texts")]
    [SerializeField] private TextMeshProUGUI _finalScoreText;
    [SerializeField] private TextMeshProUGUI _totalKillsText;
    [SerializeField] private TextMeshProUGUI _timeAliveText;

    private void OnEnable()
    {
        PlayerDamage.OnPlayerDeath += ShowGameOver;
    }

    private void OnDisable()
    {
        PlayerDamage.OnPlayerDeath -= ShowGameOver;
    }

    private void Start()
    {
        _gameOverPanel.SetActive(false);
    }

    private void ShowGameOver()
    {
        _gameOverPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _finalScoreText.text = $"Final Score: {_scoreManager.CurrentScore}";
        _totalKillsText.text = $"Enemies Defeated: {_scoreManager.EnemyKills}";

        TimeSpan t = TimeSpan.FromSeconds(_scoreManager.TimeAlive);
        _timeAliveText.text = string.Format("Survival Time: {0:D2}:{1:D2}", t.Minutes, t.Seconds);
    }
}