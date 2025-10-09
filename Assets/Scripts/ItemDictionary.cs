using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDictionary", menuName = "Game/Item Dictionary")]
public class ItemDictionary : ScriptableObject
{
    [Tooltip("List of all item prefabs. Index = item ID - 1")]
    public List<Item> itemPrefabs = new();

    private Dictionary<int, GameObject> itemDictionary;

    // Rebuilds dictionary if it's null or empty
    private void EnsureDictionary()
    {
        if (itemDictionary != null && itemDictionary.Count > 0) return;

        itemDictionary = new Dictionary<int, GameObject>();

        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            if (itemPrefabs[i] != null)
            {
                // IDs start at 1
                itemPrefabs[i].ID = i + 1;
                itemDictionary[itemPrefabs[i].ID] = itemPrefabs[i].gameObject;
            }
        }
    }

    public GameObject GetItemPrefab(int itemID)
    {
        EnsureDictionary();

        itemDictionary.TryGetValue(itemID, out GameObject prefab);
        if (prefab == null)
            Debug.LogWarning($"ItemDictionary: Item with ID {itemID} not found!");
        return prefab;
    }
}