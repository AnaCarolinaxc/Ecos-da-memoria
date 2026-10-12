using System;
using TMPro;
using UnityEngine;

public class WordManager : MonoBehaviour
{
    public static WordManager I { get; private set; }

    public const string GameId = "word";

    [Header("Partida atual (preenchido pela tela de config)")]
    public string themeFolder;   
    public int pairCount = 3;

    [Header("UI (TMP)")]
    public TMP_Text timeText;
    public TMP_Text triesText;

    public event Action OnWin;

    float elapsed;
    bool running;

    public int Tries { get; private set; }
    public int MatchedPairs { get; private set; }
    public int ElapsedSeconds => Mathf.FloorToInt(elapsed);

    void Awake()
    {
        if (I != null)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (!running) return;
        elapsed += Time.deltaTime;
        RefreshUI();
    }

    public void SetConfig(string folder, int pairs)
    {
        themeFolder = folder;
        pairCount = pairs;
    }

    public bool HasConfig => !string.IsNullOrEmpty(themeFolder) && pairCount > 0;

    public void StartRun()
    {
        elapsed = 0;
        Tries = 0;
        MatchedPairs = 0;
        running = true;
        RefreshUI();
    }

    public void StopRun()
    {
        running = false;
        RefreshUI();
    }

    public void AddTry()
    {
        Tries++;
        RefreshUI();
    }

    public void AddMatchedPair()
    {
        MatchedPairs++;
        if (MatchedPairs >= pairCount) Win();
    }

    void Win()
    {
        StopRun();
        // RankingSystem calcula a dificuldade por rows*cols/2.
        // Usando rows = nº de pares e cols = 2, "Pairs" = nº de pares.
        RankingSystem.AddScore(Tries, ElapsedSeconds, pairCount, 2, GameId);
        OnWin?.Invoke();
    }

    void RefreshUI()
    {
        if (timeText) timeText.text = $"Tempo: {Mathf.FloorToInt(elapsed)}s";
        if (triesText) triesText.text = $"Tentativas: {Tries}";
    }
}