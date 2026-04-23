using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 3.5f;
    [SerializeField] private float _acceleration = 8f;
    [SerializeField] private float _stoppingDistance = 8f;

    public int ClassId { get; set; }

    private NavMeshAgent _agent;
    private Transform _playerTransform;

    private void Start()
    {
        _playerTransform = GameObject.FindWithTag("Player").transform;
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = _moveSpeed;
        _agent.acceleration = _acceleration;
        _agent.stoppingDistance = _stoppingDistance;
    }

    private void Update()
    {
        _agent.destination = _playerTransform.position;

        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
