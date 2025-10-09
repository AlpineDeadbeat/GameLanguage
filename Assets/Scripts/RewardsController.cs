using UnityEngine;

public class RewardsController : MonoBehaviour
{
    public static RewardsController Instance { get; private set; }

    [Header("Lookups")]
    public ItemDictionary itemDictionary;

    [Header("Gold Settings")]
    [Tooltip("Item ID of your Gold Coin in ItemDictionary.")]
    public int goldItemID = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    public void GiveQuestReward(Quest quest)
    {
        if (quest == null || quest.questRewards == null || itemDictionary == null)
        {
            Debug.LogWarning("RewardsController: Missing references for quest reward.");
            return;
        }

        foreach (QuestReward reward in quest.questRewards)
        {
            switch (reward.type)
            {
                case RewardType.Item:
                    GiveItemReward(reward.rewardID, reward.amount);
                    break;

                case RewardType.Gold:
                    GiveGoldReward(reward.amount);
                    break;

                case RewardType.Experience:
                    // optional for later
                    break;

                default:
                    Debug.Log($"Reward type {reward.type} not implemented yet.");
                    break;
            }
        }
    }

    public void GiveGoldReward(int amount)
    {
        GiveItemReward(goldItemID, amount);
    }

    // ✅ Uses the same logic as your chest: add to inventory, or drop if full.
    public void GiveItemReward(int itemID, int amount)
    {
        if (itemDictionary == null)
        {
            Debug.LogWarning("RewardsController: No ItemDictionary assigned.");
            return;
        }

        GameObject itemPrefab = itemDictionary.GetItemPrefab(itemID);
        if (itemPrefab == null)
        {
            Debug.LogWarning($"RewardsController: Item with ID {itemID} not found in ItemDictionary.");
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            bool added = InventoryController.Instance != null &&
                         InventoryController.Instance.AddItem(itemPrefab);

            if (!added)
            {
                // 💥 Drop into world near the player, same as chest does
                GameObject player = GameObject.FindWithTag("Player");
                Vector3 dropPos = player != null
                    ? player.transform.position + Vector3.down
                    : transform.position;

                GameObject dropped = Instantiate(itemPrefab, dropPos, Quaternion.identity);

                var bounce = dropped.GetComponent<BounceEffect>();
                if (bounce != null) bounce.StartBounce();
            }
        }
    }
}