using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public static class NetworkRewardsServer
{
    // itemID is int in your project
    public static void GrantItemToPlayer(ulong clientId, int itemID, int quantity = 1)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        var playerObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        var agent = playerObj.GetComponent<NetworkInventoryAgent>();
        if (agent == null) return;

        var list = new List<InventorySaveData>(agent.Server_GetInventory());

        int slot = FindFirstEmptySlot(list);
        if (slot < 0) return;

        list.Add(new InventorySaveData { itemID = itemID, slotIndex = slot, quantity = quantity });
        agent.Server_SetInventory(list);
    }

    private static int FindFirstEmptySlot(List<InventorySaveData> list)
    {
        int maxSlots = 20;
        var invCtrl = Object.FindObjectOfType<InventoryController>(true);
        if (invCtrl != null) maxSlots = invCtrl.slotCount;

        var used = new HashSet<int>();
        foreach (var d in list) used.Add(d.slotIndex);

        for (int i = 0; i < maxSlots; i++)
            if (!used.Contains(i)) return i;

        return -1;
    }
}
