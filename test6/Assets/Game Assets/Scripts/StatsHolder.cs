using Unity.Netcode;
using UnityEngine;

public class StatsHolder : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool destroyOnDeath = true;

    public NetworkVariable<float> CurrentHealth = new NetworkVariable<float>(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server);

    public event System.Action<float> OnHealthChanged; // float - новое значение здоровья
    public event System.Action OnDeath;

    private void Awake()
    {
        CurrentHealth.OnValueChanged += HandleHealthChanged;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            CurrentHealth.Value = maxHealth;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        if (damage <= 0 || CurrentHealth.Value <= 0) return;

        float newHealth = Mathf.Max(0, CurrentHealth.Value - damage);
        CurrentHealth.Value = newHealth;

        if (newHealth <= 0)
        {
            DieServerRpc();
        }
    }

    [ServerRpc]
    private void DieServerRpc()
    {
        Debug.Log($"{name} has died!");
        OnDeath?.Invoke();

        if (destroyOnDeath)
        {
            //NetworkObject.Despawn(true);
        }
        else
        {
            // Можно добавить респавн или другие действия
        }
    }

    private void HandleHealthChanged(float oldValue, float newValue)
    {
        OnHealthChanged?.Invoke(newValue);

        // Локальные эффекты при изменении здоровья
        if (IsOwner)
        {
            Debug.Log($"Health changed: {newValue}/{maxHealth}");
        }
    }
}