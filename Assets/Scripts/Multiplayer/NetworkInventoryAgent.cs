using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkInventoryAgent : NetworkBehaviour
{
    // Server-authoritative snapshots
    private List<InventorySaveData> _serverInventory = new List<InventorySaveData>();
    private List<InventorySaveData> _serverHotbar = new List<InventorySaveData>();

    // ====== Server-side setters (call these from server code) ======

    public void Server_SetInventory(List<InventorySaveData> data)
    {
        if (!IsServer) return;
        _serverInventory = data;

        // Convert to primitive arrays for RPC
        ToArrays(data, out var ids, out var slots, out var qtys);
        PushInventoryClientRpc(ids, slots, qtys);
    }

    public void Server_SetHotbar(List<InventorySaveData> data)
    {
        if (!IsServer) return;
        _serverHotbar = data;

        ToArrays(data, out var ids, out var slots, out var qtys);
        PushHotbarClientRpc(ids, slots, qtys);
    }

    public List<InventorySaveData> Server_GetInventory() => _serverInventory;

    // ====== RPCs: arrays only (no List<>) ======

    [ClientRpc]
    private void PushInventoryClientRpc(int[] itemIDs, int[] slotIndices, int[] quantities, ClientRpcParams _ = default)
    {
        if (!IsOwner) return;

        var list = FromArrays(itemIDs, slotIndices, quantities);
        var inv = FindObjectOfType<InventoryController>(includeInactive: true);
        inv?.SetInventoryItems(list);
    }

    [ClientRpc]
    private void PushHotbarClientRpc(int[] itemIDs, int[] slotIndices, int[] quantities, ClientRpcParams _ = default)
    {
        if (!IsOwner) return;

        var list = FromArrays(itemIDs, slotIndices, quantities);
        var hot = FindObjectOfType<HotbarController>(includeInactive: true);
        hot?.SetHotbarItems(list);
    }

    // ====== Helpers: List<InventorySaveData> <-> arrays ======

    private static void ToArrays(List<InventorySaveData> list, out int[] ids, out int[] slots, out int[] qtys)
    {
        int n = list?.Count ?? 0;
        ids = new int[n];
        slots = new int[n];
        qtys = new int[n];
        for (int i = 0; i < n; i++)
        {
            ids[i] = list[i].itemID;
            slots[i] = list[i].slotIndex;
            qtys[i] = list[i].quantity;
        }
    }

    private static List<InventorySaveData> FromArrays(int[] ids, int[] slots, int[] qtys)
    {
        var res = new List<InventorySaveData>();
        if (ids == null || slots == null || qtys == null) return res;

        int n = Mathf.Min(ids.Length, Mathf.Min(slots.Length, qtys.Length));
        for (int i = 0; i < n; i++)
            res.Add(new InventorySaveData { itemID = ids[i], slotIndex = slots[i], quantity = qtys[i] });

        return res;
    }
}