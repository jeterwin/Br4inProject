using UnityEngine;

public class DriftingObject : MonoBehaviour
{
    [SerializeField] private Vector3 _direction = Vector3.right;
    [SerializeField] private float _speed = 2f;

    private void Update()
    {
        transform.Translate(_direction.normalized * _speed * Time.deltaTime, Space.World);
    }
}
