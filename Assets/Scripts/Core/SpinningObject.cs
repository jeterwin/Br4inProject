using UnityEngine;

public class SpinningObject : MonoBehaviour
{
    [SerializeField] private Vector3 _rotationSpeed = new Vector3(0f, 90f, 0f);

    private void Update()
    {
        transform.Rotate(_rotationSpeed * Time.deltaTime);
    }
}
