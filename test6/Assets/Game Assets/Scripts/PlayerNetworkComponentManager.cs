using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkComponentManager : NetworkBehaviour
{
    [SerializeField] private GameObject[] gameObjectsToOff;
    [SerializeField] private MonoBehaviour[] componentsToOff;

    public override void OnNetworkSpawn()
    {
        if (IsOwner) return;

        GetComponentInChildren<CharacterController>().enabled = false;

        foreach(GameObject obj in gameObjectsToOff)
        {
            obj.SetActive(false);
        }

        foreach(MonoBehaviour obj in componentsToOff)
        {
            obj.enabled = false;
        }
    }
}
