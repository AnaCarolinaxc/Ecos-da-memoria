using System;
using UnityEngine;
using TMPro;

public class UnePalavraGameManager : MonoBehaviour
{
    public static UnePalavraGameManager I { get; private set; }

    [Header("Configuração vinda da tela de config")]
    public int wordLength = 4;          // dificuldade
    public string category = "Animais"; // categoria escolhida

    [Header("UI (TMP)")]
    public TMP_Text timeText;
    public TMP_Text triesText;

    public event Action OnWin;

    float elapsed;
    bool running;

    public int Tries { get; private set; }
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
        if (!running)
        {
            return;
        }
        elapsed += Time.deltaTime;
        RefreshUI();
    }


    public void SetDifficulty(int letters)
    {
        wordLength = letters;
    }

    public void SetCategory(string cat)
    {
        category = cat;
    }

    // ====== rodada ======

    public void StartRun()
    {
        elapsed = 0;
        Tries = 0;
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

    public void Win()
    {
        StopRun();
        // usa wordLength como "rows" pra reaproveitar o RankingSystem existente (cols = 1)
        RankingSystem.AddScore(Tries, ElapsedSeconds, wordLength, 1, "word");
        OnWin?.Invoke();
    }

    void RefreshUI()
    {
        if (timeText)
        {
            timeText.text = $"Tempo: {Mathf.FloorToInt(elapsed)}s";
        }
        if (triesText)
        {
            triesText.text = $"Tentativas: {Tries}";
        }
    }
}