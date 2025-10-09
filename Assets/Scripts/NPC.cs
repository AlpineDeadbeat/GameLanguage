using System.Collections;
using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    [Header("Dialogue Data")]
    public NPCDialogue dialogueData;

    public System.Action<bool> onAnswered;
    private DialogueController dialogueUI;
    private int dialogueIndex;
    private bool isTyping;
    private bool isDialogueActive;

    // NEW: remember which line shows the question, and whether the last pick was wrong
    private int quizQuestionIndex = -1;
    private bool lastAnswerWasWrong = false;

    private enum QuestState { NotStarted, InProgress, Completed }
    private QuestState questState = QuestState.NotStarted;

    private void Start()
    {
        dialogueUI = DialogueController.Instance;
        if (dialogueUI != null)
        {
            dialogueUI.onClosed += OnDialogueClosed;
        }
    }

    private void OnDialogueClosed()
    {
        if (isDialogueActive)
        {
            EndDialogue();
        }
    }

    public bool CanInteract() => !isDialogueActive;

    public void Interact()
    {
        if (isDialogueActive || dialogueUI == null || dialogueData == null) return;

        // Determine quest state if there is a quest
        if (dialogueData.quest != null && QuestController.Instance != null)
        {
            var qid = dialogueData.quest.questID;
            if (QuestController.Instance.IsQuestCompleted(qid)) questState = QuestState.Completed;
            else if (QuestController.Instance.IsQuestActive(qid)) questState = QuestState.InProgress;
            else questState = QuestState.NotStarted;
        }

        // Pick starting line based on quest state
        if (questState == QuestState.InProgress && dialogueData.questInProgressIndex >= 0)
            dialogueIndex = dialogueData.questInProgressIndex;
        else if (questState == QuestState.Completed && dialogueData.questCompletedIndex >= 0)
            dialogueIndex = dialogueData.questCompletedIndex;
        else
            dialogueIndex = 0;

        // fresh quiz state each time
        quizQuestionIndex = -1;
        lastAnswerWasWrong = false;

        isDialogueActive = true;

        dialogueUI.ShowDialogueUI(true);
        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPortrait);
        dialogueUI.ClearChoices();
        DisplayCurrentLine();
    }

    private void DisplayCurrentLine()
    {
        if (dialogueData.dialogueLines == null ||
            dialogueIndex < 0 || dialogueIndex >= dialogueData.dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        StopAllCoroutines();
        dialogueUI.ClearChoices();
        StartCoroutine(TypeLine(dialogueData.dialogueLines[dialogueIndex]));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueUI.SetDialogueText("");

        float step = Mathf.Max(0f, dialogueData.typingSpeed);
        if (step <= 0f)
        {
            dialogueUI.SetDialogueText(line);
        }
        else
        {
            foreach (char c in line)
            {
                dialogueUI.SetDialogueText(dialogueUI.dialogueText.text + c);
                yield return new WaitForSeconds(step);
            }
        }

        isTyping = false;

        // End takes priority
        bool shouldEnd = dialogueData.endDialogueLines != null
                         && dialogueIndex < dialogueData.endDialogueLines.Length
                         && dialogueData.endDialogueLines[dialogueIndex];
        if (shouldEnd)
        {
            EndDialogue();
            yield break;
        }

        // If the previous pick was wrong and we're on the feedback line,
        // show a single "Try again" button that jumps back to the question.
        if (dialogueData.enableQuizMode && lastAnswerWasWrong && dialogueIndex != quizQuestionIndex && quizQuestionIndex >= 0)
        {
            dialogueUI.ClearChoices();
            dialogueUI.CreateChoiceButton("Try again", () =>
            {
                lastAnswerWasWrong = false;
                dialogueIndex = quizQuestionIndex; // go back to the question line
                dialogueUI.ClearChoices();
                DisplayCurrentLine();
            });
            yield break;
        }

        // Auto-advance (only if not ending and not showing the retry)
        bool shouldAuto = dialogueData.autoProgressLines != null
                          && dialogueIndex < dialogueData.autoProgressLines.Length
                          && dialogueData.autoProgressLines[dialogueIndex];
        if (shouldAuto)
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            GoNext();
            yield break;
        }

        // Otherwise show choices (if any)
        TryShowChoicesForCurrentLine();
    }

    private void TryShowChoicesForCurrentLine()
    {
        if (dialogueData.choices == null) return;

        foreach (var ch in dialogueData.choices)
        {
            if (ch != null && ch.dialogueIndex == dialogueIndex && ch.choices != null && ch.choices.Length > 0)
            {
                DisplayChoices(ch);
                return;
            }
        }
        // No choices on this line: wait for retry button (if wrong), end, or close.
    }

    private void DisplayChoices(DialogueChoice choice)
    {
        dialogueUI.ClearChoices();

        // Remember which line is the quiz question so we can jump back on wrong answers
        quizQuestionIndex = choice.dialogueIndex;

        for (int i = 0; i < choice.choices.Length; i++)
        {
            int captured = i;
            dialogueUI.CreateChoiceButton(choice.choices[i], () => ChooseOption(choice, captured));
        }
    }

    private void ChooseOption(DialogueChoice choice, int choiceIndex)
    {
        // Quest start on this pick?
        bool shouldGiveQuest = choice.givesQuest != null &&
                               choiceIndex < choice.givesQuest.Length &&
                               choice.givesQuest[choiceIndex];

        if (shouldGiveQuest && dialogueData.quest != null && QuestController.Instance != null)
        {
            QuestController.Instance.AcceptQuest(dialogueData.quest);
            questState = QuestState.InProgress;
        }

        // Quiz reward + wrong/ right tracking
        bool isCorrect = dialogueData.enableQuizMode &&
                         choice.isCorrect != null &&
                         choiceIndex < choice.isCorrect.Length &&
                         choice.isCorrect[choiceIndex];

        if (dialogueData.enableQuizMode)
        {
            lastAnswerWasWrong = !isCorrect;
            onAnswered?.Invoke(isCorrect);
        }

        if (isCorrect && RewardsController.Instance != null)
        {
            RewardsController.Instance.GiveGoldReward(Mathf.Max(1, dialogueData.quizRewardGold));
        }

        // Advance to next line for this option
        int next = (choice.nextDialogueIndexes != null && choiceIndex < choice.nextDialogueIndexes.Length)
            ? choice.nextDialogueIndexes[choiceIndex]
            : dialogueIndex + 1;

        dialogueIndex = next;
        dialogueUI.ClearChoices();
        DisplayCurrentLine();
    }

    private void GoNext()
    {
        dialogueIndex++;
        DisplayCurrentLine();
    }

    private void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;

        if (dialogueUI != null)
        {
            dialogueUI.SetDialogueText("");
            dialogueUI.ClearChoices();
            dialogueUI.ShowDialogueUI(false);
        }

        if (dialogueData != null && QuestController.Instance != null)
        {
            // Use the same identifier you put in the quest objectiveID for TalkNPC,
            // simplest is the NPC's name:
            QuestController.Instance.ProgressTalkObjective(dialogueData.npcName);
        }

        // quest completion payout (existing)
        if (dialogueData != null && dialogueData.quest != null && QuestController.Instance != null)
        {
            var qid = dialogueData.quest.questID;
            if (QuestController.Instance.IsQuestCompleted(qid))
            {
                if (RewardsController.Instance != null) RewardsController.Instance.GiveQuestReward(dialogueData.quest);
                QuestController.Instance.HandInQuest(qid);
                questState = QuestState.Completed;
            }
        }

        // reset for next time
        dialogueIndex = 0;
        quizQuestionIndex = -1;
        lastAnswerWasWrong = false;
    }
}