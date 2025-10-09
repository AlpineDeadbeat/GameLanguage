using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    private string saveLocation;
    private InventoryController inventoryController;
    private HotbarController hotbarController;
    private Chest[] chests;

    // Start is called before the first frame update
    void Start()
    {
        InitializeComponents();
        LoadGame();
    }

    private void InitializeComponents()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindObjectOfType<InventoryController>();
        hotbarController = FindObjectOfType<HotbarController>();
        chests = FindObjectsOfType<Chest>();
    }

    public void SaveGame()
    {
        SaveData saveData = new SaveData
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            mapBoundary = FindObjectOfType<CinemachineConfiner>().m_BoundingShape2D.gameObject.name,
            inventorySaveData = inventoryController.GetInventoryItems(),
            hotbarSaveData = hotbarController.GetHotbarItems(),
            chestSaveData = GetChestsState(),
            questProgressData = QuestController.Instance.activateQuests,
            handinQuestIDs = QuestController.Instance.handinQuestIDs
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
    }

    private List<ChestSaveData> GetChestsState()
    {
        List<ChestSaveData> chestStates = new List<ChestSaveData>();

        foreach(Chest chest in chests)
        {
            ChestSaveData chestSaveData = new ChestSaveData
            {
                chestID = chest.ChestID,
                isOpened = chest.IsOpened
            };
            chestStates.Add(chestSaveData);
        }

        return chestStates;
    }

    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

            // Move player
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player) player.transform.position = saveData.playerPosition;

            // Find boundary collider by name
            PolygonCollider2D savedMapBoundary = null;
            var boundaryGO = GameObject.Find(saveData.mapBoundary);
            if (boundaryGO) savedMapBoundary = boundaryGO.GetComponent<PolygonCollider2D>();

            // Set Cinemachine confiner safely (supports Confiner and Confiner2D)
            var confiner = FindObjectOfType<Cinemachine.CinemachineConfiner>();
            if (confiner && savedMapBoundary)
            {
                confiner.m_BoundingShape2D = savedMapBoundary;
            }
            else
            {
                var confiner2D = FindObjectOfType<Cinemachine.CinemachineConfiner2D>();
                if (confiner2D && savedMapBoundary)
                    confiner2D.m_BoundingShape2D = savedMapBoundary;
            }

            // Optional: highlight/generate map if boundary found
            if (savedMapBoundary)
            {
                MapController_Manual.Instance?.HighlightArea(saveData.mapBoundary);
                MapController_Dynamic.Instance?.GenerateMap(savedMapBoundary);
            }

            inventoryController.SetInventoryItems(saveData.inventorySaveData);
            hotbarController.SetHotbarItems(saveData.hotbarSaveData);

            LoadChestStates(saveData.chestSaveData);

            if (QuestController.Instance != null)
            {
                QuestController.Instance.LoadQuestProgress(saveData.questProgressData);
                QuestController.Instance.handinQuestIDs = saveData.handinQuestIDs;
            }
        }
        else
        {
            SaveGame();
            inventoryController.SetInventoryItems(new List<InventorySaveData>());
            hotbarController.SetHotbarItems(new List<InventorySaveData>());
            MapController_Dynamic.Instance?.GenerateMap();
        }
    }

    private void LoadChestStates(List<ChestSaveData> chestStates)
    {
        foreach(Chest chest in chests)
        {
            ChestSaveData chestSaveData = chestStates.FirstOrDefault(c => c.chestID == chest.ChestID);

            if (chestSaveData != null)
            {
                chest.SetOpened(chestSaveData.isOpened);
            }
        }
    }
}
