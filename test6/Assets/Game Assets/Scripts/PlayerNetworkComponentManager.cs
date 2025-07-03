using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkComponentManager : NetworkBehaviour
{
    [SerializeField] private GameObject[] gameObjectsToOff;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        foreach(GameObject obj in gameObjectsToOff)
        {
            obj.SetActive(false);
        }
    }
}
