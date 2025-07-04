using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class CombatSystem : NetworkBehaviour
{
    [Header("Network Settings")]
    [SerializeField] private float networkUpdateRate = 0.1f; // Частота сетевых обновлений

    [Header("VR Hands")]
    [SerializeField] private Transform leftHandController;
    [SerializeField] private Transform rightHandController;
    [SerializeField] private Transform leftHandVisual;
    [SerializeField] private Transform rightHandVisual;

    [Header("Logical Hands (Colliders)")]
    [SerializeField] private NetworkTransform logicalLeftHand;
    [SerializeField] private NetworkTransform logicalRightHand;

    [Header("Effects")]
    [SerializeField] private NetworkObject hitEffectPrefab;
    [SerializeField] private AudioSource hitSound;

    private float _lastNetworkUpdateTime;
    private HandState _lastLeftHandState;
    private HandState _lastRightHandState;

    private struct HandState : INetworkSerializable
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float VelocityMagnitude;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Rotation);
            serializer.SerializeValue(ref VelocityMagnitude);
        }
    }

    private void Awake()
    {
        // Инициализация логических рук
        if (logicalLeftHand != null) logicalLeftHand.Interpolate = true;
        if (logicalRightHand != null) logicalRightHand.Interpolate = true;
    }

    private void Update()
    {
        if (!IsSpawned) return;

        UpdateHandVisuals();

        // Сетевые обновления с заданной частотой
        if (Time.time - _lastNetworkUpdateTime >= networkUpdateRate)
        {
            _lastNetworkUpdateTime = Time.time;
            if (IsServer)
            {
                UpdateHandStates();
            }
            else if (IsOwner)
            {
                SendHandStatesServerRpc(
                    GetHandState(leftHandController),
                    GetHandState(rightHandController));
            }
        }
    }

    private void UpdateHandVisuals()
    {
        // Локальное обновление визуальных рук
        if (leftHandVisual && leftHandController)
        {
            leftHandVisual.SetPositionAndRotation(
                leftHandController.position,
                leftHandController.rotation);
        }

        if (rightHandVisual && rightHandController)
        {
            rightHandVisual.SetPositionAndRotation(
                rightHandController.position,
                rightHandController.rotation);
        }
    }

    private void UpdateHandStates()
    {
        if (IsOwner)
        {
            // Владелец обновляет состояния напрямую
            UpdateLogicalHand(leftHandController, logicalLeftHand);
            UpdateLogicalHand(rightHandController, logicalRightHand);
        }
        else
        {
            // Для других игроков используем сетевые данные
            UpdateLogicalHand(_lastLeftHandState, logicalLeftHand);
            UpdateLogicalHand(_lastRightHandState, logicalRightHand);
        }
    }

    private void UpdateLogicalHand(Transform source, NetworkTransform target)
    {
        if (source && target)
        {
            target.transform.SetPositionAndRotation(
                source.position,
                source.rotation);
        }
    }

    private void UpdateLogicalHand(HandState state, NetworkTransform target)
    {
        if (target)
        {
            target.transform.SetPositionAndRotation(
                state.Position,
                state.Rotation);
        }
    }

    private HandState GetHandState(Transform hand)
    {
        return new HandState
        {
            Position = hand.position,
            Rotation = hand.rotation,
            VelocityMagnitude = 0 // Можно добавить расчет скорости
        };
    }

    [ServerRpc]
    private void SendHandStatesServerRpc(HandState leftState, HandState rightState)
    {
        _lastLeftHandState = leftState;
        _lastRightHandState = rightState;

        // Репликация состояния другим клиентам
        UpdateHandStatesClientRpc(leftState, rightState);
    }

    [ClientRpc]
    private void UpdateHandStatesClientRpc(HandState leftState, HandState rightState)
    {
        if (!IsOwner)
        {
            _lastLeftHandState = leftState;
            _lastRightHandState = rightState;
        }
    }

    [ServerRpc]
    public void PlayHitEffectServerRpc(Vector3 position)
    {
        //Вариант 1: Использовать простой префаб без вложенных NetworkObjects
        GameObject effect = Instantiate(hitEffectPrefab.gameObject, position, Quaternion.identity);
        effect.GetComponent<NetworkObject>().Spawn();
        //Destroy(effect, 2f);

        // Вариант 2: Спавнить локально на всех клиентах
        //PlayHitEffectClientRpc(position);
    }

    [ClientRpc]
    private void PlayHitEffectClientRpc(Vector3 position)
    {
        // Простое решение - спавнить эффект локально
        Instantiate(hitEffectPrefab, position, Quaternion.identity);
    }
}