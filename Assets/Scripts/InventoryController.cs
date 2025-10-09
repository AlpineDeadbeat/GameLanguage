using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // <-- needed for Image

public class InventoryController : MonoBehaviour
{
    [Header("UI")]
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount = 24;

    [Header("World/Debug")]
    public GameObject[] itemPrefabs;

    [Header("Lookups")]
    // Assign your ScriptableObject asset here (Assets → Create → Game → Item Dictionary)
    public ItemDictionary itemDictionary;

    public static InventoryController Instance { get; private set; }

    // Cache of itemID -> total quantity in inventory
    private readonly Dictionary<int, int> itemsCountCache = new();
    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Ensure slots exist so we always have a grid to populate
        EnsureSlotsExist();

        // Try to auto-load ItemDictionary if not assigned (optional)
        if (itemDictionary == null)
        {
            // Put your ItemDictionary.asset under Assets/Resources/ to use this fallback name
            itemDictionary = Resources.Load<ItemDictionary>("ItemDictionary");
            if (itemDictionary == null)
                Debug.LogWarning("InventoryController: ItemDictionary not assigned and not found in Resources. Save/Load that needs it may fail.");
        }

        RebuildItemCounts();
    }

    private void EnsureSlotsExist()
    {
        if (inventoryPanel == null || slotPrefab == null)
        {
            Debug.LogError("InventoryController: inventoryPanel or slotPrefab not assigned.");
            return;
        }

        if (inventoryPanel.transform.childCount == 0)
        {
            for (int i = 0; i < slotCount; i++)
                Instantiate(slotPrefab, inventoryPanel.transform);
        }
    }

    public void RebuildItemCounts()
    {
        itemsCountCache.Clear();

        foreach (Transform slotTranform in inventoryPanel.transform)
        {
            Slot slot = slotTranform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                {
                    itemsCountCache[item.ID] = itemsCountCache.GetValueOrDefault(item.ID, 0) + Mathf.Max(1, item.quantity);
                }
            }
        }

        OnInventoryChanged?.Invoke();
    }

    public Dictionary<int, int> GetItemCounts() => itemsCountCache;

    /// <summary>
    /// Adds the picked object to inventory. You can pass a WORLD prefab (from the scene)
    /// and we will convert it to a UI item automatically (SpriteRenderer → Image, remove physics).
    /// </summary>
    public bool AddItem(GameObject itemGO)
    {
        if (itemGO == null) return false;

        // Read ID/quantity from the thing we picked up (world instance)
        Item itemToAdd = itemGO.GetComponent<Item>();
        if (itemToAdd == null) return false;

        int id = itemToAdd.ID;
        int amount = Mathf.Max(1, itemToAdd.quantity);

        // 1) Try to stack into an existing slot with same ID
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Item slotItem = slot.currentItem.GetComponent<Item>();
                if (slotItem != null && slotItem.ID == id)
                {
                    slotItem.quantity += amount;
                    slotItem.UpdateQuantityDisplay();
                    RebuildItemCounts();
                    return true;
                }
            }
        }

        // 2) No stack found -> put it into an empty slot (convert WORLD prefab to UI)
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemGO, slotTransform);
                var rt = newItem.GetComponent<RectTransform>();
                if (rt) rt.anchoredPosition = Vector2.zero;

                // --- Convert world components to UI-friendly ones ---
                var sr = newItem.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Sprite sprite = sr.sprite;
                    Destroy(sr);

                    var img = newItem.GetComponent<Image>();
                    if (img == null) img = newItem.AddComponent<Image>();
                    img.sprite = sprite;
                    img.preserveAspect = true;
                }

                var rb = newItem.GetComponent<Rigidbody2D>();
                if (rb != null) Destroy(rb);

                var col = newItem.GetComponent<Collider2D>();
                if (col != null) Destroy(col);

                var bounce = newItem.GetComponent<BounceEffect>();
                if (bounce != null) Destroy(bounce);
                // -----------------------------------------------

                Item newItemComp = newItem.GetComponent<Item>();
                if (newItemComp != null)
                {
                    newItemComp.quantity = amount;
                    newItemComp.UpdateQuantityDisplay();
                }

                slot.currentItem = newItem;
                RebuildItemCounts();
                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
    }

    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> invData = new();
        foreach (Transform slotTranform in inventoryPanel.transform)
        {
            Slot slot = slotTranform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                {
                    invData.Add(new InventorySaveData
                    {
                        itemID = item.ID,
                        slotIndex = slotTranform.GetSiblingIndex(),
                        quantity = Mathf.Max(1, item.quantity)
                    });
                }
            }
        }
        return invData;
    }

    public void SetInventoryItems(List<InventorySaveData> inventorySaveData)
    {
        // Clear existing
        foreach (Transform child in inventoryPanel.transform)
            Destroy(child.gameObject);

        // Recreate slots
        for (int i = 0; i < slotCount; i++)
            Instantiate(slotPrefab, inventoryPanel.transform);

        if (inventorySaveData == null || inventorySaveData.Count == 0)
        {
            RebuildItemCounts();
            return;
        }

        foreach (InventorySaveData data in inventorySaveData)
        {
            if (data.slotIndex < 0 || data.slotIndex >= slotCount) continue;

            Slot slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();

            // Prefer the ItemDictionary prefab if available; otherwise create a dummy UI item
            GameObject prefab = itemDictionary != null ? itemDictionary.GetItemPrefab(data.itemID) : null;

            GameObject itemGO;
            if (prefab != null)
            {
                itemGO = Instantiate(prefab, slot.transform);
            }
            else
            {
                // Fallback path if dictionary not set: just create an empty UI item
                itemGO = new GameObject("ItemUI_Fallback", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Item));
                itemGO.transform.SetParent(slot.transform, false);
                Debug.LogWarning($"InventoryController: No UI prefab found for item ID {data.itemID}. Check ItemDictionary.");
            }

            var rt = itemGO.GetComponent<RectTransform>();
            if (rt) rt.anchoredPosition = Vector2.zero;

            Item itemComp = itemGO.GetComponent<Item>();
            if (itemComp != null)
            {
                itemComp.ID = data.itemID;
                itemComp.quantity = Mathf.Max(1, data.quantity);
                itemComp.UpdateQuantityDisplay();
            }

            slot.currentItem = itemGO;
        }

        RebuildItemCounts();
    }

    public void RemoveItemsFromInventory(int itemID, int amountToRemove)
    {
        foreach (Transform slotTranform in inventoryPanel.transform)
        {
            if (amountToRemove <= 0) break;

            Slot slot = slotTranform.GetComponent<Slot>();
            if (slot?.currentItem?.GetComponent<Item>() is Item item && item.ID == itemID)
            {
                int removed = Mathf.Min(amountToRemove, item.quantity);
                item.RemoveFromStack(removed);
                amountToRemove -= removed;

                if (item.quantity <= 0)
                {
                    Destroy(slot.currentItem);
                    slot.currentItem = null;
                }
            }
        }

        RebuildItemCounts();
    }
}