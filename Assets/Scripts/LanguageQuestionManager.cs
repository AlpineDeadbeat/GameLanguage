using UnityEngine;

public class LanguageQuizManager : MonoBehaviour
{
    public static LanguageQuizManager Instance { get; private set; }

    [Header("Bank")]
    public LanguageQuestionBank bank;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (bank != null && !string.IsNullOrEmpty(bank.streamingAssetsJsonFile))
        {
            // try loading external JSON (optional)
            bank.TryLoadFromStreamingAssets(out _);
        }
    }

    // Build a temporary NPCDialogue for one question so you can assign it to an NPC at runtime if desired.
    public NPCDialogue BuildDialogueFor(LanguageQuestion q, string npcName, Sprite npcPortrait)
    {
        if (q == null) return null;

        // Lines: 0 = Question, 1 = Correct line, 2 = Wrong line
        var dlg = ScriptableObject.CreateInstance<NPCDialogue>();
        dlg.npcName = npcName;
        dlg.npcPortrait = npcPortrait;
        dlg.typingSpeed = 0.03f;
        dlg.dialogueLines = new[] {
            $"Q: {q.prompt}",
            "✅ Correct!",
            "❌ Not quite. Try again."
        };
        dlg.autoProgressLines = new[] { false, true, false };
        dlg.endDialogueLines = new[] { false, true, false };
        dlg.enableQuizMode = true;
        dlg.quizRewardGold = Mathf.Max(1, q.rewardGold);

        // Build choices under dialogueIndex 0
        var choice = new DialogueChoice
        {
            dialogueIndex = 0,
            choices = (string[])q.options.Clone(),
            nextDialogueIndexes = new int[q.options.Length],
            isCorrect = new bool[q.options.Length],
            givesQuest = new bool[q.options.Length]
        };

        for (int i = 0; i < q.options.Length; i++)
        {
            bool correct = (i == q.correctIndex);
            choice.isCorrect[i] = correct;
            choice.nextDialogueIndexes[i] = correct ? 1 : 2; // correct -> line 1, wrong -> line 2
        }

        dlg.choices = new[] { choice };
        dlg.quest = null; // quiz-only by default
        dlg.questInProgressIndex = -1;
        dlg.questCompletedIndex = -1;
        return dlg;
    }
}
