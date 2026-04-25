using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BCIDebugSimulator : MonoBehaviour
{
    [SerializeField] private float _cycleInterval = 1.5f;
    [SerializeField] private BCITargetingSystem _targetingSystem;

    private float _timer;
    private int _currentIndex;

    private void Update()
    {
        _timer += Time.unscaledDeltaTime;

        if (_timer < _cycleInterval)
            return;

        _timer = 0f;

        List<int> activeClassIds = _targetingSystem.ClassToEnemy.Keys.ToList();
        if (activeClassIds.Count == 0)
        {
            Debug.Log("[BCIDebug] No active enemies registered");
            return;
        }

        _currentIndex %= activeClassIds.Count;
        int classId = activeClassIds[_currentIndex];
        Debug.Log($"[BCIDebug] Detection → classId={classId}");
        _targetingSystem.OnClassSelected((uint)classId);
        _currentIndex = (_currentIndex + 1) % activeClassIds.Count;
    }
}
