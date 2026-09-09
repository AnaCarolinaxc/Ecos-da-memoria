using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleBoard : MonoBehaviour
{
    [Header("UI")]
    public RectTransform gridParent; //  GridLayoutGroup
    public GameObject piecePrefab;   // prefab com Image + PuzzlePiece

    GridLayoutGroup grid;
    readonly List<PuzzlePiece> pieces = new();
    PuzzlePiece firstSelected;

    void Awake() => grid = gridParent.GetComponent<GridLayoutGroup>();

    public void StartGame()
    {
        int rows = PuzzleManager.I.rows;
        int cols = PuzzleManager.I.cols;
        Sprite source = PuzzleManager.I.selectedImage;

        BuildGrid(rows, cols);
        SpawnPieces(SliceSprite(source, rows, cols));
        Shuffle();

        PuzzleManager.I.StartRun();
    }

    void BuildGrid(int rows, int cols)
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }
        pieces.Clear();
        firstSelected = null;

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = cols;

        float cellSize = Mathf.Min(gridParent.rect.width / cols, gridParent.rect.height / rows);
        grid.cellSize = new Vector2(cellSize, cellSize);
    }

    List<Sprite> SliceSprite(Sprite source, int rows, int cols)
    {
        var result = new List<Sprite>();
        Texture2D tex = source.texture;
        var srcRect = source.rect;

        float pieceW = srcRect.width / cols;
        float pieceH = srcRect.height / rows;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                // r=0 é a linha do topo -> inverte Y (textura tem origem embaixo)
                float x = srcRect.x + c * pieceW;
                float y = srcRect.y + (rows - 1 - r) * pieceH;
                result.Add(Sprite.Create(tex, new Rect(x, y, pieceW, pieceH), new Vector2(0.5f, 0.5f)));
            }
        }
        return result;
    }

    void SpawnPieces(List<Sprite> slices)
    {
        for (int i = 0; i < slices.Count; i++)
        {
            var go = Instantiate(piecePrefab, gridParent);
            var p = go.GetComponent<PuzzlePiece>();
            p.Setup(this, correctIndex: i, currentIndex: i, sprite: slices[i]);
            pieces.Add(p);
        }
    }

    void Shuffle()
    {
        var order = new List<int>();
        for (int i = 0; i < pieces.Count; i++)
        {
            order.Add(i);
        }

        for (int i = order.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }

        // evita cair já resolvido por acaso
        bool solved = true;
        for (int i = 0; i < order.Count; i++)
        {
            if (order[i] != i)
            {
                solved = false;
                break;
            }
        }

        if (solved && order.Count > 1)
        {
            (order[0], order[1]) = (order[1], order[0]);
        }

        for (int slot = 0; slot < order.Count; slot++)
        {
            var piece = pieces[order[slot]];
            piece.CurrentIndex = slot;
            piece.transform.SetSiblingIndex(slot);
        }
    }

    public void OnPieceTapped(PuzzlePiece piece)
    {
        if (firstSelected == null)
        {
            firstSelected = piece;
            piece.SetSelected(true);
            return;
        }

        if (firstSelected == piece)
        {
            piece.SetSelected(false);
            firstSelected = null;
            return;
        }
        SwapPieces(firstSelected, piece);
        firstSelected.SetSelected(false);
        firstSelected = null;

        PuzzleManager.I.AddTry();
        CheckWin();
    }

    void SwapPieces(PuzzlePiece a, PuzzlePiece b)
    {
        int slotA = a.transform.GetSiblingIndex();
        int slotB = b.transform.GetSiblingIndex();
        a.transform.SetSiblingIndex(slotB);
        b.transform.SetSiblingIndex(slotA);
        (a.CurrentIndex, b.CurrentIndex) = (b.CurrentIndex, a.CurrentIndex);
    }

    void CheckWin()
    {
        foreach (var p in pieces)
        {
            if (!p.IsCorrect)
            {
                return;
            }
        }
        PuzzleManager.I.Win();
    }
}