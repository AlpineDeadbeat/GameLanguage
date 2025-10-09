using UnityEngine;
using Unity.Netcode;

public class NetworkItemWorld : NetworkBehaviour
{
    // int (not string) to match your Item/ItemDictionary/InventorySaveData
    [SerializeField] public int itemID;
    [SerializeField] public int quantity = 1;
}
