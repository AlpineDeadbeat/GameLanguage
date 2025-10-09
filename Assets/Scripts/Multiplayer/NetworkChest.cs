using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Place this next to your existing Chest component on the same GameObject.
/// When interacted, the server flips the open state once and spawns networked loot items.
/// </summary>
[RequireComponent(typeof(Chest))]
public class NetworkChest : NetworkBehaviour, IInteractable
{
    private Chest chest;

    [Tooltip("World item prefab that has NetworkObject + NetworkItemWorld")]
    public GameObject networkItemPrefab;

    void Awake()
    {
        chest = GetComponent<Chest>();
    }

    public bool CanInteract() => chest.CanInteract();

    public void Interact()
    {
        if (!IsOwner) return; // client requests open
        TryOpenServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryOpenServerRpc()
    {
        if (!chest.IsOpened)
        {
            chest.OpenChest(); // your existing visual logic; make sure it is deterministic
            SpawnLootServer();
        }
    }

    private void SpawnLootServer()
    {
        // Minimal example: spawn one item directly above the chest
        if (networkItemPrefab == null) return;
        var go = Instantiate(networkItemPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        var no = go.GetComponent<NetworkObject>();
        if (no != null) no.Spawn(true);
    }
}
