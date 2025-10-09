#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class LanguageBankIOEditor
{
    [MenuItem("Tools/Language Quiz/Export Bank To JSON")]
    public static void ExportBank()
    {
        var bank = Selection.activeObject as LanguageQuestionBank;
        if (bank == null) { EditorUtility.DisplayDialog("Export Bank", "Select a LanguageQuestionBank asset first.", "OK"); return; }

        string path = EditorUtility.SaveFilePanel("Export Questions JSON", Application.dataPath, "questions.json", "json");
        if (string.IsNullOrEmpty(path)) return;

        string json = LanguageQuestionBank.ToJson(bank.questions, true);
        File.WriteAllText(path, json);
        EditorUtility.RevealInFinder(path);
    }

    [MenuItem("Tools/Language Quiz/Import JSON Into Bank (Replace)")]
    public static void ImportReplace()
    {
        var bank = Selection.activeObject as LanguageQuestionBank;
        if (bank == null) { EditorUtility.DisplayDialog("Import JSON", "Select a LanguageQuestionBank asset first.", "OK"); return; }

        string path = EditorUtility.OpenFilePanel("Import Questions JSON", Application.dataPath, "json");
        if (string.IsNullOrEmpty(path)) return;

        string json = File.ReadAllText(path);
        bank.questions = LanguageQuestionBank.FromJson(json);
        EditorUtility.SetDirty(bank);
        AssetDatabase.SaveAssets();
        Debug.Log($"Imported {bank.questions.Count} questions (replace).");
    }

    [MenuItem("Tools/Language Quiz/Import JSON Into Bank (Merge)")]
    public static void ImportMerge()
    {
        var bank = Selection.activeObject as LanguageQuestionBank;
        if (bank == null) { EditorUtility.DisplayDialog("Import JSON", "Select a LanguageQuestionBank asset first.", "OK"); return; }

        string path = EditorUtility.OpenFilePanel("Import Questions JSON", Application.dataPath, "json");
        if (string.IsNullOrEmpty(path)) return;

        string json = File.ReadAllText(path);
        var incoming = LanguageQuestionBank.FromJson(json);

        var map = new System.Collections.Generic.Dictionary<string, int>();
        for (int i = 0; i < bank.questions.Count; i++)
            if (!string.IsNullOrEmpty(bank.questions[i].id)) map[bank.questions[i].id] = i;

        int changed = 0;
        foreach (var q in incoming)
        {
            if (q == null || string.IsNullOrEmpty(q.id)) continue;
            if (map.TryGetValue(q.id, out int idx)) { bank.questions[idx] = q; }
            else { bank.questions.Add(q); }
            changed++;
        }
        EditorUtility.SetDirty(bank);
        AssetDatabase.SaveAssets();
        Debug.Log($"Merged {changed} questions.");
    }
}
#endif