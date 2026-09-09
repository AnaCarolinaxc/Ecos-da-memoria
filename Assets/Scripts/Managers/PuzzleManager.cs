using System;
using UnityEngine;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager I { get; private set; }

    [Header("Board")]
    public int rows = 3;
    public int cols = 3;

    [Header("Imagem")]
    public Sprite selectedImage;

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

    public bool HasImage() => selectedImage != null;
    public void SetImage(Sprite sprite) => selectedImage = sprite;

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
        RankingSystem.AddScore(Tries, ElapsedSeconds, rows, cols, "puzzle");
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