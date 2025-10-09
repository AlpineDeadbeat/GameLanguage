using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; } //Singleton Instance

    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;
    public Transform choiceContainer;
    public GameObject choiceButtonPrefab;

    // NEW: notify listeners when the panel is closed
    public System.Action onClosed;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); //Make sure only one instance
    }

    public void ShowDialogueUI(bool show)
    {
        dialoguePanel.SetActive(show); //Toggle UI visability
    }

    public void EnsureVisible()
    {
        if (dialoguePanel == null) return;
        dialoguePanel.SetActive(true);

        // Optional: handle CanvasGroup transparency
        var cg = dialoguePanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        // Prevent accidental scale hiding
        dialoguePanel.transform.localScale = Vector3.one;
    }

    // NEW: call this from your Close/X button instead of just hiding the panel
    public void Close()
    {
        // standard cleanup
        ShowDialogueUI(false);
        SetDialogueText(string.Empty);
        ClearChoices();
        // notify NPC (and anyone else) that the dialogue ended
        onClosed?.Invoke();
    }

    public void SetNPCInfo(string npcName, Sprite portrait)
    {
        nameText.text = npcName;
        portraitImage.sprite = portrait;
    }

    public void SetDialogueText(string text)
    {
        dialogueText.text = text;
    }

    public void ClearChoices()
    {
        foreach (Transform child in choiceContainer) Destroy(child.gameObject);
    }

    public GameObject CreateChoiceButton(string choiceText, UnityEngine.Events.UnityAction onClick)
    {
        GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);
        choiceButton.GetComponentInChildren<TMP_Text>().text = choiceText;
        choiceButton.GetComponent<Button>().onClick.AddListener(onClick);
        return choiceButton;
    }
}
