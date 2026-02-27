using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    [Header("Board")]
    public int rows = 2;
    public int cols = 2;

    [Header("UI (TMP)")]
    public TMP_Text timeText;
    public TMP_Text triesText;
    public TMP_Text pairsText;
    public event Action OnWin;
    // ✅ banco de imagens do jogo (sprites escolhidos)
    private List<Sprite> selectedSprites = new();

    float elapsed;
    bool running;

    public int Tries { get; private set; }
    public int MatchedPairs { get; private set; }

    public int PairCount => (rows * cols) / 2;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (!running) return;
        elapsed += Time.deltaTime;
        RefreshUI();
    }

    // ====== tempo/tentativas/pares ======

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
        RefreshUI();
        if (MatchedPairs >= PairCount)
        {
            StopRun();
            RankingSystem.AddScore(Tries, ElapsedSeconds, rows, cols);
            OnWin?.Invoke();
        }
    }

    void RefreshUI()
    {
        if (timeText) timeText.text = $"Tempo: {Mathf.FloorToInt(elapsed)}";
        if (triesText) triesText.text = $"Tentativas: {Tries}";
        if (pairsText) pairsText.text = $"Pares: {MatchedPairs}/{PairCount}";
    }
    
   

    public void SetSelectedSprites(List<Sprite> sprites)
    {
        selectedSprites = sprites ?? new List<Sprite>();
    }

    public IReadOnlyList<Sprite> GetSprites()
    {
        return selectedSprites;
    }
    public int ElapsedSeconds => Mathf.FloorToInt(elapsed);
    
    public bool HasEnoughImages()
    {
        return selectedSprites != null && selectedSprites.Count >= PairCount;
    }
}
