using UnityEngine;

public class GunMovementController : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float _swayAmount = 0.01f;
    [SerializeField] private float _maxSwayAmount = 0.05f;
    [SerializeField] private float _smoothAmount = 6f;

    [Header("Rotation Sway")]
    [SerializeField] private float _rotationAmount = 4f;
    [SerializeField] private float _maxRotationAmount = 8f;
    [SerializeField] private float _rotationSmoothness = 12f;

    [Header("Recoil Settings")]
    [SerializeField] private Vector3 _recoilRotation = new Vector3(-10, 2, 0);
    [SerializeField] private Vector3 _recoilKickBack = new Vector3(0, 0, -0.1f);
    [SerializeField] private float _snappiness = 10f;
    [SerializeField] private float _returnSpeed = 5f;

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    private Vector3 _currentRecoilRotation;
    private Vector3 _targetRecoilRotation;
    private Vector3 _currentRecoilPos;
    private Vector3 _targetRecoilPos;

    private void Start()
    {
        _initialPosition = transform.localPosition;
        _initialRotation = transform.localRotation;
    }

    private void Update()
    {
        _targetRecoilRotation = Vector3.Lerp(_targetRecoilRotation, Vector3.zero, _returnSpeed * Time.unscaledDeltaTime);
        _currentRecoilRotation = Vector3.Slerp(_currentRecoilRotation, _targetRecoilRotation, _snappiness * Time.unscaledDeltaTime);

        _targetRecoilPos = Vector3.Lerp(_targetRecoilPos, Vector3.zero, _returnSpeed * Time.unscaledDeltaTime);
        _currentRecoilPos = Vector3.Lerp(_currentRecoilPos, _targetRecoilPos, _snappiness * Time.unscaledDeltaTime);

        float mouseX = -Input.GetAxis("Mouse X") * _swayAmount;
        float mouseY = -Input.GetAxis("Mouse Y") * _swayAmount;

        mouseX = Mathf.Clamp(mouseX, -_maxSwayAmount, _maxSwayAmount);
        mouseY = Mathf.Clamp(mouseY, -_maxSwayAmount, _maxSwayAmount);

        Vector3 swayPos = new Vector3(mouseX, mouseY, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, _initialPosition + swayPos + _currentRecoilPos, 1f - Mathf.Exp(-_smoothAmount * Time.unscaledDeltaTime));

        float tiltX = Input.GetAxis("Mouse Y") * _rotationAmount;
        float tiltY = -Input.GetAxis("Mouse X") * _rotationAmount;

        tiltX = Mathf.Clamp(tiltX, -_maxRotationAmount, _maxRotationAmount);
        tiltY = Mathf.Clamp(tiltY, -_maxRotationAmount, _maxRotationAmount);

        Quaternion swayRot = Quaternion.Euler(tiltX + _currentRecoilRotation.x, tiltY + _currentRecoilRotation.y, _currentRecoilRotation.z);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, _initialRotation * swayRot, 1f - Mathf.Exp(-_rotationSmoothness * Time.unscaledDeltaTime));
    }

    public void Shoot()
    {
        _targetRecoilRotation += new Vector3(_recoilRotation.x, Random.Range(-_recoilRotation.y, _recoilRotation.y), _recoilRotation.z);
        _targetRecoilPos += new Vector3(Random.Range(-_recoilKickBack.x, _recoilKickBack.x), _recoilKickBack.y, _recoilKickBack.z);
    }
}