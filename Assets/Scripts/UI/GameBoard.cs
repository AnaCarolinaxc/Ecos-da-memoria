using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameBoard : MonoBehaviour
{
    [Header("Refs")]
    public Transform boardParent;
    public GridLayoutGroup grid;
    public GameObject cardPrefab;

    [Header("Layout")]
    public Vector2 spacing = new Vector2(20, 20);
    public RectOffset padding; // <<< NÃO inicialize aqui

    [Header("Gameplay")]
    public float mismatchHideDelay = 0.7f;

    private List<CardView> cards; // <<< inicializa no Awake
    private CardView first;
    private CardView second;
    private bool locked;

    void Awake()
    {
        // garante lista
        cards = new List<CardView>();

        // garante padding
        if (padding == null) padding = new RectOffset(20, 20, 20, 20);

        // (opcional) auto-pega refs se esquecer no Inspector
        if (boardParent == null) boardParent = transform;
        if (grid == null) grid = GetComponent<GridLayoutGroup>();
    }

    public void StartGame()
    {
        Debug.Log("StartGame() entrou");
        BuildBoard();
        GameManager.I.StartRun();
    }
    public void BuildBoard()
    {
        ClearBoard();
        
        int rows = GameManager.I.rows;
        int cols = GameManager.I.cols;
        int totalCards = rows * cols;
        int neededPairs = totalCards / 2;


        var spritesPool = GameManager.I.GetSprites(); 

        if (spritesPool == null || spritesPool.Count < neededPairs)
        {
            Debug.LogError($"Imagens insuficientes: precisa de {neededPairs}, mas tem {(spritesPool==null?0:spritesPool.Count)}");
            return;
        }

        grid.spacing = spacing;
        grid.padding = padding;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = cols;
        FitCellSize(rows, cols);

        // monta deck (pares duplicados) e embaralha
        var deck = BuildDeck(spritesPool, neededPairs);
        Shuffle(deck);
Debug.Log($"BuildBoard: totalCards={totalCards} parent={boardParent.name}");
        for (int i = 0; i < totalCards; i++)
        {
            Debug.Log("Instanciando card...");
            var go = Instantiate(cardPrefab, boardParent);
            var cv = go.GetComponent<CardView>();
            cards.Add(cv);

            int pairId = deck[i].pairId;
            Sprite spr = deck[i].sprite;
            cv.Init(this, pairId, spr);
        }
    }

    public void OnCardClicked(CardView c)
    {
        if (locked) return;
        if (c.isMatched) return;
        if (c == first) return;

        c.SetFaceUp(true);

        if (first == null)
        {
            first = c;
            return;
        }

        second = c;
        locked = true;
        GameManager.I.AddTry();

        if (first.pairId == second.pairId)
        {
            first.isMatched = true;
            second.isMatched = true;
            first.SetInteractable(false);
            second.SetInteractable(false);

            first = null;
            second = null;
            locked = false;

            GameManager.I.AddMatchedPair();
        }
        else
        {
            StartCoroutine(HideMismatch());
        }
    }

    IEnumerator HideMismatch()
    {
        yield return new WaitForSeconds(mismatchHideDelay);
        first.SetFaceUp(false);
        second.SetFaceUp(false);
        first = null;
        second = null;
        locked = false;
    }

    struct DeckItem { public int pairId; public Sprite sprite; }

    List<DeckItem> BuildDeck(IReadOnlyList<Sprite> pool, int neededPairs)
    {
        var deck = new List<DeckItem>(neededPairs * 2);
        // pega N sprites aleatórias sem repetir
        var idx = new List<int>();
        for (int i = 0; i < pool.Count; i++) idx.Add(i);
        Shuffle(idx);

        for (int p = 0; p < neededPairs; p++)
        {
            var spr = pool[idx[p]];
            deck.Add(new DeckItem { pairId = p, sprite = spr });
            deck.Add(new DeckItem { pairId = p, sprite = spr });
        }
        return deck;
    }

    void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    void FitCellSize(int rows, int cols)
    {
        var rt = grid.GetComponent<RectTransform>();
        float availableW = rt.rect.width - grid.padding.left - grid.padding.right - grid.spacing.x * (cols - 1);
        float availableH = rt.rect.height - grid.padding.top - grid.padding.bottom - grid.spacing.y * (rows - 1);
        float cell = Mathf.Floor(Mathf.Min(availableW / cols, availableH / rows));
        grid.cellSize = new Vector2(cell, cell);
    }

    void ClearBoard()
    {
        cards.Clear();
        first = null;
        second = null;
        locked = false;

        for (int i = boardParent.childCount - 1; i >= 0; i--)
            Destroy(boardParent.GetChild(i).gameObject);
    }
}
