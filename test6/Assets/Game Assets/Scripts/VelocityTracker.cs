using UnityEngine;

public class HandVelocityTracker : MonoBehaviour
{
    private Vector3 _previousPosition;
    private Vector3 _currentVelocity;
    private float _updateInterval = 0.1f;
    private float _timeSinceLastUpdate;

    public Vector3 CurrentVelocity => _currentVelocity;
    public float CurrentSpeed => _currentVelocity.magnitude;

    private void Start()
    {
        _previousPosition = transform.position;
    }

    private void Update()
    {
        _timeSinceLastUpdate += Time.deltaTime;

        if (_timeSinceLastUpdate >= _updateInterval)
        {
            CalculateVelocity();
            _timeSinceLastUpdate = 0f;
        }
    }

    private void CalculateVelocity()
    {
        Vector3 currentPosition = transform.position;
        _currentVelocity = (currentPosition - _previousPosition) / _timeSinceLastUpdate;
        _previousPosition = currentPosition;
    }
}