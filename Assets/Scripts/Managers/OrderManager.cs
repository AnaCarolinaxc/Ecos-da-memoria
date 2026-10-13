using System;
using TMPro;
using UnityEngine;

// Estado da partida do "Acerte a ordem": config, tempo, tentativas e vitória.
// Colocar em um GameObject vazio NA RAIZ da cena (fora do Canvas), sempre ativo.
public class OrderManager : MonoBehaviour
{
    public static OrderManager I { get; private set; }

    public const string GameId = "order";

    [Header("Partida atual (preenchido pela tela de config)")]
    public bool ascending = true;   // true = 1,2,3...  false = N...3,2,1
    public int ballCount = 5;

    [Header("UI (TMP)")]
    public TMP_Text timeText;
    public TMP_Text triesText;      // opcional (pode ficar vazio)

    public event Action OnWin;

    float elapsed;
    bool running;

    public int Tries { get; private set; }
    public int Placed { get; private set; }
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

    public void SetConfig(bool asc, int count)
    {
        ascending = asc;
        ballCount = count;
    }

    public void StartRun()
    {
        elapsed = 0f;
        Tries = 0;
        Placed = 0;
        running = true;
        RefreshUI();
    }

    public void StopRun()
    {
        running = false;
        RefreshUI();
    }

    // clique errado
    public void AddTry()
    {
        Tries++;
        RefreshUI();
    }

    // bola certa colocada em cima
    public void AddPlaced()
    {
        Placed++;
        if (Placed >= ballCount) Win();
    }

    void Win()
    {
        StopRun();
        // RankingSystem calcula a dificuldade por rows*cols/2.
        // Usando rows = nº de bolas e cols = 2, "Pairs" = nº de bolas.
        RankingSystem.AddScore(Tries, ElapsedSeconds, ballCount, 2, GameId);
        OnWin?.Invoke();
    }

    void RefreshUI()
    {
        if (timeText) timeText.text = $"Tempo: {Mathf.FloorToInt(elapsed)}s";
        if (triesText) triesText.text = $"Tentativas: {Tries}";
    }
}