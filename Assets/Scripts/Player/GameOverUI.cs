using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering.PostProcessing;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private ScoreManager _scoreManager;
    [SerializeField] private CanvasGroup _reticleCanvasGroup;
    [SerializeField] private Transform _gunTransform;

    [Header("Display Texts")]
    [SerializeField] private TextMeshProUGUI _finalScoreText;
    [SerializeField] private TextMeshProUGUI _totalKillsText;
    [SerializeField] private TextMeshProUGUI _timeAliveText;

    [SerializeField] private float gunDownwardDistance = 1f;

    [Header("Post Process")]
    [SerializeField] private PostProcessVolume _postProcessVolume;
    private ChromaticAberration _chromatic;

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

        if (_postProcessVolume != null && _postProcessVolume.profile != null)
        {
            _postProcessVolume.profile.TryGetSettings(out _chromatic);
            if (_chromatic != null)
            {
                _chromatic.intensity.value = 0f;
            }
        }
    }

    private void ShowGameOver()
    {
        // Disable sway so it doesn't override the death movement
        if (_gunTransform != null && _gunTransform.TryGetComponent<GunMovementController>(out var sway))
        {
            sway.enabled = false;
        }

        StartCoroutine(ProcessDeathSequence());
    }

    private IEnumerator ProcessDeathSequence()
    {
        float duration = 1f;
        float elapsed = 0f;

        Vector3 gunStartPos = _gunTransform != null ? _gunTransform.localPosition : Vector3.zero;
        Vector3 gunEndPos = gunStartPos + (Vector3.down * gunDownwardDistance);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Post-process effect
            if (_chromatic != null)
            {
                _chromatic.intensity.overrideState = true;
                _chromatic.intensity.value = t;
            }

            // Reticle fade
            if (_reticleCanvasGroup != null)
            {
                _reticleCanvasGroup.alpha = 1f - t;
            }

            // Gun lowering
            if (_gunTransform != null)
            {
                _gunTransform.localPosition = Vector3.Lerp(gunStartPos, gunEndPos, t);
            }

            yield return null;
        }

        yield return new WaitForSecondsRealtime(1f);

        _gameOverPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _finalScoreText.text = $"Final Score: {_scoreManager.CurrentScore}";
        _totalKillsText.text = $"Enemies Defeated: {_scoreManager.EnemyKills}";

        TimeSpan tSpan = TimeSpan.FromSeconds(_scoreManager.TimeAlive);
        _timeAliveText.text = string.Format("Survival Time: {0:D2}:{1:D2}", tSpan.Minutes, tSpan.Seconds);
    }
}