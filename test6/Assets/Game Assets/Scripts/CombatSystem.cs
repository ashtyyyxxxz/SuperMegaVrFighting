using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class CombatSystem : NetworkBehaviour
{
    [Header("Physical Hands")]
    [SerializeField] private Transform leftPhysicalHand;
    [SerializeField] private Transform rightPhysicalHand;

    [Header("Visual Hands")]
    [SerializeField] private Transform leftVisualHand;
    [SerializeField] private Transform rightVisualHand;

    [Header("Logical Hands")]
    [SerializeField] private Transform leftLogicalHand;
    [SerializeField] private Transform rightLogicalHand;
    [SerializeField] private NetworkTransform leftNetworkTransform;
    [SerializeField] private NetworkTransform rightNetworkTransform;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private AudioSource hitSound;

    private NetworkVariable<HandData> leftHandData = new NetworkVariable<HandData>();
    private NetworkVariable<HandData> rightHandData = new NetworkVariable<HandData>();

    private struct HandData : INetworkSerializable
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float Velocity;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Rotation);
            serializer.SerializeValue(ref Velocity);
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            UpdateOwnerHands();
        }
        else
        {
            UpdateRemoteHands();
        }
    }

    private void UpdateOwnerHands()
    {
        // Обновляем данные для левой руки
        var leftData = new HandData
        {
            Position = leftPhysicalHand.position,
            Rotation = leftPhysicalHand.rotation,
            Velocity = 0 // Можно добавить расчет скорости
        };

        // Обновляем данные для правой руки
        var rightData = new HandData
        {
            Position = rightPhysicalHand.position,
            Rotation = rightPhysicalHand.rotation,
            Velocity = 0
        };

        if (IsServer)
        {
            leftHandData.Value = leftData;
            rightHandData.Value = rightData;
        }
        else
        {
            UpdateHandServerRpc(leftData, rightData);
        }

        // Локальная синхронизация визуальных рук
        leftVisualHand.SetPositionAndRotation(leftPhysicalHand.position, leftPhysicalHand.rotation);
        rightVisualHand.SetPositionAndRotation(rightPhysicalHand.position, rightPhysicalHand.rotation);
    }

    private void UpdateRemoteHands()
    {
        // Синхронизация для других игроков
        leftVisualHand.SetPositionAndRotation(leftHandData.Value.Position, leftHandData.Value.Rotation);
        rightVisualHand.SetPositionAndRotation(rightHandData.Value.Position, rightHandData.Value.Rotation);

        leftLogicalHand.SetPositionAndRotation(leftHandData.Value.Position, leftHandData.Value.Rotation);
        rightLogicalHand.SetPositionAndRotation(rightHandData.Value.Position, rightHandData.Value.Rotation);
    }

    [ServerRpc]
    private void UpdateHandServerRpc(HandData leftData, HandData rightData)
    {
        leftHandData.Value = leftData;
        rightHandData.Value = rightData;
    }

    [ServerRpc]
    public void PlayHitEffectServerRpc(Vector3 position)
    {
        // Спавним эффект только на сервере (не синхронизируется)
        GameObject serverEffect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        Destroy(serverEffect, 2f);

        // Запускаем эффект на всех клиентах
        PlayHitEffectClientRpc(position);
    }

    [ClientRpc]
    private void PlayHitEffectClientRpc(Vector3 position)
    {
        // Спавним локальную версию эффекта на каждом клиенте
        GameObject clientEffect = Instantiate(hitEffectPrefab, position, Quaternion.identity);

        // Проигрываем звук
        if (hitSound != null)
        {
            hitSound.Play();
        }
    }
}