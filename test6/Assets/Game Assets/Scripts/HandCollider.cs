using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class HandCollider : NetworkBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float _minDamage = 5f;
    [SerializeField] private float _maxDamage = 25f;
    [SerializeField] private float _minSpeedThreshold = 1f;
    [SerializeField] private float _maxSpeedThreshold = 5f;

    [Header("References")]
    [SerializeField] private CombatSystem _combatSystem;
    [SerializeField] private StatsHolder _statsHolder;

    private HandVelocityTracker _velocityTracker;

    private void Awake()
    {
        _velocityTracker = GetComponent<HandVelocityTracker>() ?? gameObject.AddComponent<HandVelocityTracker>();
        _statsHolder = _statsHolder ?? GetComponentInParent<StatsHolder>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        float currentSpeed = _velocityTracker.CurrentSpeed;
        float damage = CalculateDamageBasedOnSpeed(currentSpeed);

        if (damage <= 0) return;

        if (other.TryGetComponent<HandCollider>(out _))
        {
            _combatSystem.PlayHitEffectServerRpc(transform.position);
            return;
        }

        if (other.TryGetComponent<StatsHolder>(out var targetStats) && targetStats != _statsHolder)
        {
            DealDamageServerRpc(targetStats.NetworkObjectId, damage);
        }
    }

    [ServerRpc]
    private void DealDamageServerRpc(ulong targetId, float damage)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var target))
        {
            if (target.TryGetComponent<StatsHolder>(out var targetStats))
            {
                targetStats.TakeDamageServerRpc(damage);
            }
        }
    }

    private float CalculateDamageBasedOnSpeed(float speed)
    {
        if (speed < _minSpeedThreshold) return 0;
        float normalizedSpeed = Mathf.InverseLerp(_minSpeedThreshold, _maxSpeedThreshold, speed);
        return Mathf.Lerp(_minDamage, _maxDamage, normalizedSpeed);
    }

    public float CompareAndGetDamage(float magnitude1, float magnitude2)
    {
        return CalculateDamageBasedOnSpeed(Mathf.Max(magnitude1, magnitude2));
    }
}