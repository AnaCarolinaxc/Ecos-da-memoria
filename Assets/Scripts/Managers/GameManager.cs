using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    public int rows = 2;
    public int cols = 2;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public int PairCount => (rows * cols) / 2;
    public bool IsValidBoard() => (rows * cols) % 2 == 0;
}