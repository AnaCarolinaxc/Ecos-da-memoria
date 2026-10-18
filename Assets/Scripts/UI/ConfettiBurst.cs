using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Confete que "explode" na tela quando o jogador vence.

public class ConfettiBurst : MonoBehaviour
{
    static ConfettiBurst instance;

    [Header("Onde aparece (opcional)")]
    public RectTransform container;          // vazio = usa o Canvas principal

    [Header("Quantidade / duração")]
    public int piecesPerWave = 60;           // peças por explosão
    public int waves = 3;                    // quantas explosões seguidas
    public float waveInterval = 0.35f;       // tempo entre as explosões
    public float lifetime = 3f;              // quanto tempo cada peça fica na tela

    [Header("Explosão")]
    [Range(0f, 1f)] public float originX = 0.5f;   // 0 = esquerda, 1 = direita
    [Range(0f, 1f)] public float originY = 0.4f;   // 0 = embaixo, 1 = em cima
    public float minSpeed = 700f;
    public float maxSpeed = 1800f;
    public float gravity = 900f;             // puxa as peças para baixo
    public float drag = 2f;                  // resistência do ar (maior = freia mais rápido)

    [Header("Visual")]
    public Vector2 sizeRange = new Vector2(18f, 38f);
    public Color[] colors =
    {
        new Color(0.95f, 0.26f, 0.21f),
        new Color(1.00f, 0.60f, 0.00f),
        new Color(1.00f, 0.92f, 0.23f),
        new Color(0.30f, 0.80f, 0.30f),
        new Color(0.13f, 0.59f, 0.95f),
        new Color(0.61f, 0.15f, 0.69f),
        new Color(0.91f, 0.12f, 0.39f),
    };

    class Piece
    {
        public GameObject go;
        public RectTransform rt;
        public Image img;
        public Color color;
        public Vector2 vel;
        public float rot, rotSpeed, flipSpeed, flipPhase, age, life;
    }

    readonly List<Piece> active = new List<Piece>();
    readonly Stack<Piece> pool = new Stack<Piece>();
    RectTransform layer;

    // ------------------------------------------------------------ API estática
    public static void Fire()
    {
        if (instance == null) instance = FindFirstObjectByType<ConfettiBurst>();
        if (instance == null)
        {
            var go = new GameObject("Confetti");
            instance = go.AddComponent<ConfettiBurst>();
        }
        instance.Play();
    }

    public static void ClearAll()
    {
        if (instance != null) instance.Clear();
    }

    void Awake()
    {
        if (instance == null) instance = this;
    }

    // ------------------------------------------------------------ uso
    public void Play()
    {
        if (!EnsureLayer()) return;
        StartCoroutine(Waves());
    }

    public void Clear()
    {
        StopAllCoroutines();
        for (int i = active.Count - 1; i >= 0; i--) Release(i);
    }

    IEnumerator Waves()
    {
        for (int w = 0; w < Mathf.Max(1, waves); w++)
        {
            Spawn(piecesPerWave);
            if (w < waves - 1) yield return new WaitForSeconds(waveInterval);
        }
    }

    // ------------------------------------------------------------ camada
    bool EnsureLayer()
    {
        if (layer != null) return true;

        // camada nova: descarta peças antigas (podem ter sido destruídas junto com a camada)
        active.Clear();
        pool.Clear();

        RectTransform parent = container;
        if (parent == null)
        {
            Canvas best = null;
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (!c.isRootCanvas || c.renderMode == RenderMode.WorldSpace) continue;
                if (best == null || c.sortingOrder > best.sortingOrder) best = c;
            }
            if (best == null)
            {
                Debug.LogWarning("[ConfettiBurst] Nenhum Canvas encontrado.");
                return false;
            }
            parent = (RectTransform)best.transform;
        }

        var go = new GameObject("ConfettiLayer", typeof(RectTransform), typeof(Canvas));
        layer = (RectTransform)go.transform;
        layer.SetParent(parent, false);
        layer.anchorMin = Vector2.zero;
        layer.anchorMax = Vector2.one;
        layer.offsetMin = Vector2.zero;
        layer.offsetMax = Vector2.zero;

        // Canvas próprio: desenha por cima de tudo e não obriga o Canvas principal a se refazer a cada frame
        var cv = go.GetComponent<Canvas>();
        cv.overrideSorting = true;
        cv.sortingOrder = 1000;

        return true;
    }

    // ------------------------------------------------------------ peças
    Piece Get()
    {
        if (pool.Count > 0) return pool.Pop();

        var go = new GameObject("c", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var p = new Piece
        {
            go = go,
            rt = (RectTransform)go.transform,
            img = go.GetComponent<Image>()
        };
        p.rt.SetParent(layer, false);
        p.rt.anchorMin = new Vector2(0.5f, 0.5f);
        p.rt.anchorMax = new Vector2(0.5f, 0.5f);
        p.img.raycastTarget = false;      // não bloqueia cliques
        return p;
    }

    void Release(int index)
    {
        var p = active[index];
        active.RemoveAt(index);
        p.go.SetActive(false);
        pool.Push(p);
    }

    void Spawn(int count)
    {
        if (layer == null) return;

        Rect r = layer.rect;
        Vector2 origin = new Vector2((originX - 0.5f) * r.width, (originY - 0.5f) * r.height);

        for (int i = 0; i < count; i++)
        {
            Piece p = Get();

            float size = Random.Range(sizeRange.x, sizeRange.y);
            p.rt.sizeDelta = new Vector2(size, size * Random.Range(0.45f, 0.8f));
            p.rt.anchoredPosition = origin + Random.insideUnitCircle * 30f;

            p.color = (colors != null && colors.Length > 0) ? colors[Random.Range(0, colors.Length)] : Color.white;
            p.img.color = p.color;

            float ang = Random.Range(0f, Mathf.PI * 2f);
            float speed = Random.Range(minSpeed, maxSpeed);
            p.vel = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * speed;

            p.rot = Random.Range(0f, 360f);
            p.rotSpeed = Random.Range(-540f, 540f);
            p.flipSpeed = Random.Range(4f, 10f);
            p.flipPhase = Random.Range(0f, Mathf.PI * 2f);

            p.age = 0f;
            p.life = lifetime * Random.Range(0.8f, 1.1f);

            p.go.SetActive(true);
            active.Add(p);
        }
    }

    void Update()
    {
        if (active.Count == 0) return;

        float dt = Time.deltaTime;

        for (int i = active.Count - 1; i >= 0; i--)
        {
            Piece p = active[i];
            p.age += dt;

            if (p.age >= p.life) { Release(i); continue; }

            // movimento: explode, freia pelo ar e cai
            p.vel *= Mathf.Max(0f, 1f - drag * dt);
            p.vel.y -= gravity * dt;
            p.rt.anchoredPosition += p.vel * dt;

            // gira e "vira" (efeito de papelzinho girando no ar)
            p.rot += p.rotSpeed * dt;
            p.rt.localRotation = Quaternion.Euler(0f, 0f, p.rot);
            p.rt.localScale = new Vector3(1f, Mathf.Cos(p.age * p.flipSpeed + p.flipPhase), 1f);

            // some no final
            float fade = Mathf.Clamp01((p.life - p.age) / (p.life * 0.3f));
            if (fade < 1f) p.img.color = new Color(p.color.r, p.color.g, p.color.b, fade);
        }
    }
}