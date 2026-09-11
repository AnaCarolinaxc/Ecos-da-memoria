using UnityEngine;
using UnityEngine.UI;

public class PuzzlePreviewGrid : MonoBehaviour
{
    public Image previewImage; // a própria Image que mostra a foto (Preserve Aspect = true)
    public Color lineColor = new Color(0, 0, 0, 1.0f);
    public float lineThickness = 4f;
    

    RectTransform linesParent;
    int currentRows, currentCols;

    void Awake()
    {
        var go = new GameObject("GridLines", typeof(RectTransform));
        linesParent = go.GetComponent<RectTransform>();
        linesParent.SetParent(previewImage.rectTransform, false);
    }

    public void Redraw(int rows, int cols)
{
    currentRows = rows;
    currentCols = cols;
    Debug.Log($"[PuzzlePreviewGrid] Redraw rows={rows} cols={cols} sprite={previewImage.sprite}");
    FitLinesParentToImage();
    DrawLines();
    Debug.Log($"[PuzzlePreviewGrid] linesParent ativo={linesParent.gameObject.activeSelf} size={linesParent.sizeDelta} filhos={linesParent.childCount}");
}

    // calcula o tamanho real da foto dentro da caixa, considerando a proporção dela
    void FitLinesParentToImage()
    {
        if (!PuzzleManager.I.HasImage()) { linesParent.gameObject.SetActive(false); return; }        linesParent.gameObject.SetActive(true);
        Rect box = previewImage.rectTransform.rect;
        float boxAspect = box.width / box.height;

        var spriteRect = previewImage.sprite.rect;
        float spriteAspect = spriteRect.width / spriteRect.height;

        float width, height;
        if (spriteAspect > boxAspect)
        {
            // foto mais "larga" que a caixa -> barras em cima/embaixo
            width = box.width;
            height = width / spriteAspect;
        }
        else
        {
            // foto mais "alta" que a caixa -> barras nas laterais
            height = box.height;
            width = height * spriteAspect;
        }


        linesParent.anchorMin = linesParent.anchorMax = new Vector2(0.5f, 0.5f);
        linesParent.pivot = new Vector2(0.5f, 0.5f);
        linesParent.sizeDelta = new Vector2(width, height);
        linesParent.anchoredPosition = Vector2.zero;
    }

    void DrawLines()
    {
        foreach (Transform child in linesParent) Destroy(child.gameObject);

        for (int c = 1; c < currentCols; c++) CreateLine(vertical: true, normalizedPos: (float)c / currentCols);
        for (int r = 1; r < currentRows; r++) CreateLine(vertical: false, normalizedPos: (float)r / currentRows);
    }

    void CreateLine(bool vertical, float normalizedPos)
    {
        var go = new GameObject("Line", typeof(RectTransform), typeof(Image));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(linesParent, false);
        go.GetComponent<Image>().color = lineColor;

        if (vertical)
        {
            rt.anchorMin = new Vector2(normalizedPos, 0);
            rt.anchorMax = new Vector2(normalizedPos, 1);
            rt.sizeDelta = new Vector2(lineThickness, 0);
        }
        else
        {
            rt.anchorMin = new Vector2(0, normalizedPos);
            rt.anchorMax = new Vector2(1, normalizedPos);
            rt.sizeDelta = new Vector2(0, lineThickness);
        }
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
    }
}