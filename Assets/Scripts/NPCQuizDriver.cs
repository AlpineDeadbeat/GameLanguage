using UnityEngine;

public class NPCQuizDriver : MonoBehaviour
{
    [Header("Links")]
    public NPC npc;                               // same object as your NPC.cs
    public LanguageQuestionBank bank;             // your Afrikaans bank
    public Sprite overridePortrait;               // optional; else NPCDialogue portrait is used

    [Header("Difficulty Range (dynamic)")]
    public LqDifficulty startMin = LqDifficulty.A1;
    public LqDifficulty startMax = LqDifficulty.A2;
    public LqDifficulty hardCap = LqDifficulty.C2;

    [Header("Progression")]
    public int correctToLevelUp = 3;              // after this many correct answers → push difficulty up
    public bool demoteOnWrong = false;            // optional: drop a tier on wrong
    public LqType? typeFilter = null;             // leave null for any type

    // runtime state
    private LqDifficulty curMin, curMax;
    private int correctAtThisTier = 0;

    void Awake()
    {
        if (npc == null) npc = GetComponent<NPC>();
        curMin = startMin;
        curMax = startMax;
        PrepareNextQuestion();
    }

    void OnEnable()
    {
        // listen for correct/incorrect from NPC
        if (npc != null) npc.onAnswered += OnAnswered;
    }
    void OnDisable()
    {
        if (npc != null) npc.onAnswered -= OnAnswered;
    }

    // Called from a UI button or another script if you want to force a refresh
    public void PrepareNextQuestion()
    {
        if (LanguageQuizManager.Instance == null || bank == null || npc == null) return;

        // get a random question in the current band
        var q = bank.GetRandom(curMin, curMax, typeFilter);
        if (q == null) return;

        // portrait/name
        string npcName = npc.dialogueData != null ? npc.dialogueData.npcName : "Tutor";
        Sprite portrait = overridePortrait != null ? overridePortrait
                         : (npc.dialogueData != null ? npc.dialogueData.npcPortrait : null);

        // build a one-off dialogue asset in memory and assign it
        var dlg = LanguageQuizManager.Instance.BuildDialogueFor(q, npcName, portrait);
        // Important: ensure flags so “correct” ends, “wrong” shows retry (your NPC.cs already handles retry)
        dlg.endDialogueLines = new[] { false, true, false };
        dlg.autoProgressLines = new[] { false, false, false };

        npc.dialogueData = dlg;    // NPC will use this the next time you interact
    }

    private void OnAnswered(bool wasCorrect)
    {
        if (wasCorrect)
        {
            correctAtThisTier++;
            if (correctAtThisTier >= correctToLevelUp)
            {
                correctAtThisTier = 0;
                // nudge difficulty upward: widen max, then lift min
                if (curMax < hardCap) curMax++;
                else if (curMin < hardCap) curMin++;
            }
        }
        else if (demoteOnWrong)
        {
            correctAtThisTier = 0;
            if (curMin > startMin) curMin--;     // soften band a bit
            if (curMax > curMin) curMax = curMin;
        }

        // Prepare a fresh question for the NEXT interaction
        PrepareNextQuestion();
    }
}