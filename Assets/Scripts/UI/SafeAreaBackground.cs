using UnityEngine;

// Faz o objeto cobrir o Canvas INTEIRO (até as bordas físicas do celular: notch,
// cantos arredondados, barra de gestos), não importa o tamanho do painel pai.
// Use em um filho "Background" (Image) que fica atrás do conteúdo da tela.
// O conteúdo do painel NÃO é alterado de posição.
[RequireComponent(typeof(RectTransform))]
public class SafeAreaBackground : MonoBehaviour
{
    RectTransform rt;
    Canvas rootCanvas;
    readonly Vector3[] corners = new Vector3[4];

    void OnEnable()
    {
        rt = (RectTransform)transform;
        var half = new Vector2(0.5f, 0.5f);
        rt.anchorMin = half;
        rt.anchorMax = half;
        rt.pivot = half;
        Apply();
    }

    void Update() => Apply();

    void Apply()
    {
        if (rt == null) rt = (RectTransform)transform;
        var parent = rt.parent as RectTransform;
        if (parent == null) return;

        if (rootCanvas == null)
        {
            var c = GetComponentInParent<Canvas>();
            rootCanvas = c != null ? c.rootCanvas : null;
        }
        if (rootCanvas == null) return;

        var canvasRT = (RectTransform)rootCanvas.transform;
        if (canvasRT.rect.width <= 0f || canvasRT.rect.height <= 0f) return;

        // cantos do Canvas inteiro, convertidos para o espaço local do painel pai
        canvasRT.GetWorldCorners(corners);
        Vector2 min = parent.InverseTransformPoint(corners[0]);
        Vector2 max = parent.InverseTransformPoint(corners[2]);

        Vector2 size = max - min;
        Vector2 pos = (min + max) * 0.5f - parent.rect.center;

        // só escreve se mudou (evita sujar o layout todo frame)
        if ((rt.sizeDelta - size).sqrMagnitude > 0.0001f) rt.sizeDelta = size;
        if ((rt.anchoredPosition - pos).sqrMagnitude > 0.0001f) rt.anchoredPosition = pos;
    }
}