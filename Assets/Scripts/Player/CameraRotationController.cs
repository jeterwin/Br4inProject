using Cinemachine;
using System.Collections;
using UnityEngine;

public class CameraRotationController : MonoBehaviour
{
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private CinemachineVirtualCamera _vcam;

    [Header("Shake Settings")]
    [SerializeField] private float _shakeAmplitude = 1.2f;
    [SerializeField] private float _shakeFrequency = 2.0f;
    [SerializeField] private float _shakeDuration = 0.2f;

    private float _cameraPitch;
    private CinemachineBasicMultiChannelPerlin _noise;
    private Coroutine _shakeCoroutine;

    private void OnEnable()
    {
        EnemyDeathHandler.OnDeath += TriggerShake;
    }

    private void OnDisable()
    {
        EnemyDeathHandler.OnDeath -= TriggerShake;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (_vcam != null)
        {
            _noise = _vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            _noise.m_AmplitudeGain = 0f;
        }
    }

    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * _mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * _mouseSensitivity;

        _playerTransform.Rotate(Vector3.up * mouseX);

        _cameraPitch -= mouseY;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -90f, 90f);
        _vcam.transform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
    }

    private void TriggerShake(EnemyDeathHandler enemy)
    {
        if (_noise == null) return;

        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
        }

        _shakeCoroutine = StartCoroutine(ProcessShake());
    }

    private IEnumerator ProcessShake()
    {
        _noise.m_AmplitudeGain = _shakeAmplitude;
        _noise.m_FrequencyGain = _shakeFrequency;

        yield return new WaitForSeconds(_shakeDuration);

        _noise.m_AmplitudeGain = 0f;
        _noise.m_FrequencyGain = 0f;
        _shakeCoroutine = null;
    }
}