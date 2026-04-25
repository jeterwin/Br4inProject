using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;

    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool hasInput = h * h + v * v > 0.01f;

        TimeManager.Instance.SetMoving(hasInput);

        if (hasInput)
        {
            Vector3 moveDir = (transform.right * h + transform.forward * v).normalized;
            _rb.velocity = new Vector3(moveDir.x * _moveSpeed, _rb.velocity.y, moveDir.z * _moveSpeed);
        }
        else
        {
            _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
        }
    }
}
