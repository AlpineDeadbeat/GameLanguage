using System;
using UnityEngine;

[Serializable]
public enum LqDifficulty { A1, A2, B1, B2, C1, C2 }
[Serializable]
public enum LqType { MCQ, Synonym, Antonym, Picture, Audio, Cloze }

[Serializable]
public class LanguageQuestion
{
    public string id;                  // unique id (e.g., "fruits_001")
    [TextArea] public string prompt;   // the question/prompt text
    public string[] options;           // 2..6 options
    public int correctIndex;           // index into options
    public LqDifficulty difficulty;    // CEFR-ish bucket
    public LqType type;                // kind of question
    public int rewardGold = 1;         // coins for correct
}
