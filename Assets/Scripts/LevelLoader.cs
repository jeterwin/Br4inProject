using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private CanvasGroup _loadingPanel;
    [SerializeField] private TMP_Text _loadingText;
    [SerializeField] private Button _playButton;

    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private float _wobbleSpeed = 8f;
    [SerializeField] private float _wobbleAmount = 10f;

    private AsyncOperation _asyncLoad;
    private Vector3 _originalTextPos;

    private void Start()
    {
        _loadingPanel.alpha = 0f;
        _loadingPanel.gameObject.SetActive(false);
        _playButton.gameObject.SetActive(false);
        _playButton.onClick.AddListener(CommitSceneActivation);
        _originalTextPos = _loadingText.transform.localPosition;
    }

    public void TriggerLoading(string sceneName)
    {
        StartCoroutine(LoadingRoutine(sceneName));
    }

    private IEnumerator LoadingRoutine(string sceneName)
    {
        _loadingPanel.gameObject.SetActive(true);

        float timer = 0f;
        while (timer < _fadeDuration)
        {
            timer += Time.deltaTime;
            _loadingPanel.alpha = timer / _fadeDuration;
            yield return null;
        }

        _asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        _asyncLoad.allowSceneActivation = false;

        while (!_asyncLoad.isDone)
        {
            _loadingText.transform.localPosition = _originalTextPos + new Vector3(0, Mathf.Sin(Time.time * _wobbleSpeed) * _wobbleAmount, 0);

            if (_asyncLoad.progress >= 0.9f)
            {
                _loadingText.text = "Ready!";
                _playButton.gameObject.SetActive(true);
            }

            yield return null;
        }
    }

    private void CommitSceneActivation()
    {
        _asyncLoad.allowSceneActivation = true;
    }
}