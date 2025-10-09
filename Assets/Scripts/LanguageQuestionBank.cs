using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "LanguageQuestionBank", menuName = "Language/Question Bank")]
public class LanguageQuestionBank : ScriptableObject
{
    public List<LanguageQuestion> questions = new List<LanguageQuestion>();

    [Header("External Source (optional)")]
    [Tooltip("If set, will try to load this JSON from StreamingAssets at runtime and merge/replace.")]
    public string streamingAssetsJsonFile = "questions.json";
    public bool replaceWithExternalAtRuntime = false;

    // --- JSON IO (Editor & Runtime PC) ---
    [System.Serializable] private class Wrapper { public List<LanguageQuestion> questions; }

    public static List<LanguageQuestion> FromJson(string json)
    {
        var wrap = JsonUtility.FromJson<Wrapper>(json);
        return wrap != null && wrap.questions != null ? wrap.questions : new List<LanguageQuestion>();
    }

    public static string ToJson(List<LanguageQuestion> list, bool pretty = true)
    {
        var wrap = new Wrapper { questions = list };
        return JsonUtility.ToJson(wrap, pretty);
    }

    public bool TryLoadFromStreamingAssets(out int addedOrReplaced)
    {
        addedOrReplaced = 0;
        try
        {
            string path = Path.Combine(Application.streamingAssetsPath, streamingAssetsJsonFile);
            if (!File.Exists(path)) return false;
            string json = File.ReadAllText(path);
            var external = FromJson(json);
            if (replaceWithExternalAtRuntime)
            {
                questions = new List<LanguageQuestion>(external);
                addedOrReplaced = questions.Count;
            }
            else
            {
                // merge by id (replace if exists)
                var map = new Dictionary<string, int>();
                for (int i = 0; i < questions.Count; i++)
                    if (!string.IsNullOrEmpty(questions[i].id)) map[questions[i].id] = i;

                foreach (var q in external)
                {
                    if (q == null || string.IsNullOrEmpty(q.id)) continue;
                    if (map.TryGetValue(q.id, out int idx)) { questions[idx] = q; }
                    else { questions.Add(q); }
                    addedOrReplaced++;
                }
            }
            return true;
        }
        catch { return false; }
    }

    public LanguageQuestion GetRandom(LqDifficulty minDiff, LqDifficulty maxDiff, LqType? typeFilter = null)
    {
        var pool = new List<LanguageQuestion>();
        foreach (var q in questions)
        {
            if (q == null || q.options == null || q.options.Length < 2) continue;
            if (q.difficulty < minDiff || q.difficulty > maxDiff) continue;
            if (typeFilter.HasValue && q.type != typeFilter.Value) continue;
            pool.Add(q);
        }
        if (pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }
}
