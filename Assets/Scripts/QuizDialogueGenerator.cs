#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class QuizDialogueGenerator : EditorWindow
{
    private LanguageQuestionBank bank;
    private Sprite defaultPortrait;
    private string npcName = "Tutor";
    private LqDifficulty minDiff = LqDifficulty.A1, maxDiff = LqDifficulty.C2;
    private LqType? typeFilter = null;
    private int count = 10;
    private string targetFolder = "Assets/Dialogue/Generated";

    [MenuItem("Tools/Language Quiz/Generate NPCDialogue Assets")]
    public static void Open() => GetWindow<QuizDialogueGenerator>("Generate Dialogue");

    private void OnGUI()
    {
        bank = (LanguageQuestionBank)EditorGUILayout.ObjectField("Question Bank", bank, typeof(LanguageQuestionBank), false);
        defaultPortrait = (Sprite)EditorGUILayout.ObjectField("NPC Portrait", defaultPortrait, typeof(Sprite), false);
        npcName = EditorGUILayout.TextField("NPC Name", npcName);
        minDiff = (LqDifficulty)EditorGUILayout.EnumPopup("Min Difficulty", minDiff);
        maxDiff = (LqDifficulty)EditorGUILayout.EnumPopup("Max Difficulty", maxDiff);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Type Filter");
        if (GUILayout.Button(typeFilter.HasValue ? typeFilter.Value.ToString() : "(Any)"))
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("(Any)"), !typeFilter.HasValue, () => typeFilter = null);
            foreach (LqType t in System.Enum.GetValues(typeof(LqType)))
            {
                LqType tt = t;
                menu.AddItem(new GUIContent(tt.ToString()), typeFilter == tt, () => typeFilter = tt);
            }
            menu.ShowAsContext();
        }
        EditorGUILayout.EndHorizontal();

        count = EditorGUILayout.IntField("How Many", count);
        targetFolder = EditorGUILayout.TextField("Target Folder", targetFolder);

        GUI.enabled = bank != null;
        if (GUILayout.Button("Generate"))
        {
            GenerateAssets();
        }
        GUI.enabled = true;
    }

    private void GenerateAssets()
    {
        if (!AssetDatabase.IsValidFolder(targetFolder))
        {
            string parent = "Assets";
            foreach (var part in targetFolder.Replace("\\", "/").Split('/'))
            {
                if (string.IsNullOrEmpty(part)) continue;
                if (!AssetDatabase.IsValidFolder($"{parent}/{part}"))
                    AssetDatabase.CreateFolder(parent, part);
                parent = $"{parent}/{part}";
            }
        }

        int made = 0;
        for (int i = 0; i < count; i++)
        {
            var q = bank.GetRandom(minDiff, maxDiff, typeFilter);
            if (q == null) break;

            // Shuffle options so playthrough is less predictable
            var options = (string[])q.options.Clone();
            for (int s = 0; s < options.Length; s++)
            {
                int r = Random.Range(0, options.Length);
                (options[s], options[r]) = (options[r], options[s]);
            }
            int newCorrect = System.Array.IndexOf(options, q.options[q.correctIndex]);

            var dlg = ScriptableObject.CreateInstance<NPCDialogue>();
            dlg.npcName = npcName;
            dlg.npcPortrait = defaultPortrait;
            dlg.typingSpeed = 0.03f;
            dlg.enableQuizMode = true;
            dlg.quizRewardGold = Mathf.Max(1, q.rewardGold);
            dlg.dialogueLines = new[] { $"Q: {q.prompt}", "✅ Correct!", "❌ Not quite. Try again." };
            dlg.autoProgressLines = new[] { false, true, false };
            dlg.endDialogueLines = new[] { false, true, false };

            var choice = new DialogueChoice
            {
                dialogueIndex = 0,
                choices = options,
                nextDialogueIndexes = new int[options.Length],
                isCorrect = new bool[options.Length],
                givesQuest = new bool[options.Length]
            };
            for (int c = 0; c < options.Length; c++)
            {
                bool correct = (c == newCorrect);
                choice.isCorrect[c] = correct;
                choice.nextDialogueIndexes[c] = correct ? 1 : 2;
            }
            dlg.choices = new[] { choice };

            string safeId = string.IsNullOrEmpty(q.id) ? $"q_{i:0000}" : q.id.Replace("/", "_").Replace("\\", "_");
            string assetPath = $"{targetFolder}/{safeId}.asset";
            AssetDatabase.CreateAsset(dlg, assetPath);
            made++;
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Quiz Dialogue", $"Generated {made} NPCDialogue assets in:\n{targetFolder}", "OK");
    }
}
#endif