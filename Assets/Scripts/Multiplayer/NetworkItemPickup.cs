using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkInventoryAgent))]
public class NetworkItemPickup : NetworkBehaviour
{
    private NetworkInventoryAgent agent;

    void Awake()
    {
        agent = GetComponent<NetworkInventoryAgent>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsOwner) return;

        var niw = other.GetComponent<NetworkItemWorld>();
        if (niw != null && niw.NetworkObject != null && niw.NetworkObject.IsSpawned)
        {
            TryPickupServerRpc(niw.NetworkObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryPickupServerRpc(NetworkObjectReference itemRef, ServerRpcParams _ = default)
    {
        if (!itemRef.TryGet(out var itemNO)) return;
        var itemWorld = itemNO.GetComponent<NetworkItemWorld>();
        if (itemWorld == null) return;

        var playerAgent = GetComponent<NetworkInventoryAgent>();
        var list = new List<InventorySaveData>(playerAgent.Server_GetInventory());

        int slotIndex = FindFirstEmptySlot(list);
        if (slotIndex < 0) return;

        list.Add(new InventorySaveData { itemID = itemWorld.itemID, slotIndex = slotIndex, quantity = itemWorld.quantity });
        playerAgent.Server_SetInventory(list);

        itemNO.Despawn(true);

        // Show popup on the picking client with a friendly name if we can find it
        string itemName = itemWorld.itemID.ToString();
        var dictionary = FindObjectOfType<ItemDictionary>(true);
        if (dictionary != null)
        {
            var prefab = dictionary.GetItemPrefab(itemWorld.itemID);
            if (prefab != null)
            {
                var item = prefab.GetComponent<Item>();
                if (item != null) itemName = item.Name;
            }
        }
        ShowPickupClientRpc(OwnerClientId, itemName);
    }

    private int FindFirstEmptySlot(List<InventorySaveData> list)
    {
        var invCtrl = FindObjectOfType<InventoryController>(true);
        int maxSlots = invCtrl != null ? invCtrl.slotCount : 20;

        var used = new HashSet<int>();
        foreach (var d in list) used.Add(d.slotIndex);

        for (int i = 0; i < maxSlots; i++)
            if (!used.Contains(i)) return i;

        return -1;
    }

    [ClientRpc]
    private void ShowPickupClientRpc(ulong targetClient, string itemName)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClient) return;
        ItemPickupUIController.Instance?.ShowItemPickup(itemName, null);
    }
}