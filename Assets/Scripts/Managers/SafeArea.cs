using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    RectTransform rt;
    Rect lastSafe;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        Apply(); // chama quando habilita
    }

    void Update()
    {
        // evita ficar calculando sem necessidade
        if (Screen.safeArea != lastSafe)
            Apply();
    }

    void Apply()
    {
        // evita divisão por zero (muito comum no Device Simulator em alguns estados)
        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        Rect safe = Screen.safeArea;

        // fallback: se safeArea vier zerada/estranha, usa tela inteira
        if (safe.width <= 0 || safe.height <= 0)
            safe = new Rect(0, 0, Screen.width, Screen.height);

        lastSafe = safe;

        Vector2 anchorMin = safe.position;
        Vector2 anchorMax = safe.position + safe.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // proteção extra contra NaN
        if (float.IsNaN(anchorMin.x) || float.IsNaN(anchorMin.y) ||
            float.IsNaN(anchorMax.x) || float.IsNaN(anchorMax.y))
            return;

        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}