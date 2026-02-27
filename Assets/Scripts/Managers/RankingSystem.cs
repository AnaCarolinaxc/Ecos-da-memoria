using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ScoreEntry
{
    public int tries;
    public int seconds;

    // novo: tamanho/dificuldade
    public int rows;
    public int cols;

    // opcional: cache do multiplicador (não precisa salvar, pode calcular)
    public int Pairs => (rows * cols) / 2;

    public float DifficultyMult
    {
        get
        {
            // base: 2x2 (2 pares) = 1.0
            int pairs = Mathf.Max(1, Pairs);
            return pairs / 2f;
        }
    }

    public float AdjustedSeconds => seconds / DifficultyMult;
    public float AdjustedTries => tries / DifficultyMult;
}

[Serializable]
class ScoreListWrapper
{
    public List<ScoreEntry> list = new();
}

public static class RankingSystem
{
    const string KEY = "RANKING_V3";
    const int MAX = 8;

    public static List<ScoreEntry> Load()
    {
        var json = PlayerPrefs.GetString(KEY, "");
        if (string.IsNullOrEmpty(json)) return new List<ScoreEntry>();

        var w = JsonUtility.FromJson<ScoreListWrapper>(json);
        return w?.list ?? new List<ScoreEntry>();
    }

    // novo: recebe rows/cols
    public static void AddScore(int tries, int seconds, int rows, int cols)
    {
        var list = Load();
        list.Add(new ScoreEntry
        {
            tries = tries,
            seconds = seconds,
            rows = rows,
            cols = cols
        });

        // ordena por "score ajustado": menor tempo ajustado primeiro,
        // desempata por menos tentativas ajustadas,
        // depois por maior tabuleiro (premia dificuldade se ainda empatar),
        // e por fim por menor tempo real.
        list.Sort((a, b) =>
        {
            int c = a.AdjustedSeconds.CompareTo(b.AdjustedSeconds);
            if (c != 0) return c;

            c = a.AdjustedTries.CompareTo(b.AdjustedTries);
            if (c != 0) return c;

            // se empatar, prefere quem jogou tabuleiro maior
            c = b.Pairs.CompareTo(a.Pairs);
            if (c != 0) return c;

            // último desempate: tempo real
            c = a.seconds.CompareTo(b.seconds);
            if (c != 0) return c;

            return a.tries.CompareTo(b.tries);
        });

        if (list.Count > MAX) list.RemoveRange(MAX, list.Count - MAX);

        var wrapper = new ScoreListWrapper { list = list };
        PlayerPrefs.SetString(KEY, JsonUtility.ToJson(wrapper));
        PlayerPrefs.Save();
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(KEY);
    }
}
