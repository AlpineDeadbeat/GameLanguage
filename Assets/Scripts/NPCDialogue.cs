using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]
public class NPCDialogue : ScriptableObject
{
    [Header("Identity & Portrait")]
    public string npcName;
    public Sprite npcPortrait;

    [Header("Dialogue Lines")]
    [TextArea] public string[] dialogueLines;

    [Tooltip("If true at index i, that line will auto-advance after Auto Progress Delay.")]
    public bool[] autoProgressLines;

    [Tooltip("If true at index i, conversation ends after showing that line.")]
    public bool[] endDialogueLines;

    [Header("Typing & Voice")]
    public float autoProgressDelay = 1.5f;
    public float typingSpeed = 0.05f;
    public AudioClip voiceSound;
    public float voicePitch = 1f;

    [Header("Choices (branching)")]
    public DialogueChoice[] choices;

    [Header("Quest Integration")]
    [Tooltip("What this NPC says while the quest it gave is in progress.")]
    public int questInProgressIndex;
    [Tooltip("What this NPC says once the quest it gave is completed.")]
    public int questCompletedIndex;
    [Tooltip("Quest this NPC can give (optional).")]
    public Quest quest;

    [Header("Quiz Mode (Language Q&A)")]
    [Tooltip("If enabled, choices on the marked line(s) can award coins when picked correctly.")]
    public bool enableQuizMode = false;

    [Tooltip("Coins (as inventory items) awarded for each correct choice when Quiz Mode is on.")]
    public int quizRewardGold = 1;
}

[System.Serializable]
public class DialogueChoice
{
    [Tooltip("Which dialogue line these choices appear under (index into dialogueLines).")]
    public int dialogueIndex;

    [Tooltip("Text shown on each button, in order.")]
    public string[] choices;

    [Tooltip("For each choice, the next dialogue line index to go to.")]
    public int[] nextDialogueIndexes;

    [Tooltip("For each choice, whether selecting it should give the NPC's quest.")]
    public bool[] givesQuest;

    [Tooltip("For each choice, whether it's the correct answer (used if Quiz Mode is enabled).")]
    public bool[] isCorrect;
}