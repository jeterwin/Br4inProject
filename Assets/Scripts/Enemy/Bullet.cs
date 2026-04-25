using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float _speed;
    private float _age;

    [SerializeField] private float _lifetime = 10f;

    public void Initialize(Vector3 direction, float speed)
    {
        transform.rotation = Quaternion.LookRotation(direction);
        _speed = speed;
    }

    private void Update()
    {
        transform.Translate(_speed * Time.deltaTime * Vector3.forward);

        _age += Time.deltaTime;
        if (_age >= _lifetime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Destroy(gameObject);
    }
}
