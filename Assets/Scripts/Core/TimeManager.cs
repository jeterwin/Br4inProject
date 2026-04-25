using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }
    public bool IsTimeFrozen => Time.timeScale == frozenTimeScale;

    [SerializeField] private float frozenTimeScale = 0.1f;

    private bool _wasMoving;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetMoving(bool isMoving)
    {
        if (isMoving == _wasMoving) return;

        _wasMoving = isMoving;
        Time.timeScale = isMoving ? 1f : frozenTimeScale;
        Debug.Log(isMoving ? "Time resumed" : "Time frozen");
    }
}
