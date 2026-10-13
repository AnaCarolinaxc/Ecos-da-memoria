using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Gameplay do "Acerte a ordem".
// As bolinhas caem na "arena" com uma física simples (gravidade + colisão entre bolas e paredes).
// Clicar na bola da vez faz ela subir para a fileira de cima; as outras caem para ocupar o espaço.
// Clicar na bola errada faz ela piscar em vermelho e conta uma tentativa.
// Colocar no PlayingPanel. O jogo começa sozinho quando o painel é ativado.
public class OrderBoard : MonoBehaviour
{
    [Header("Áreas (objetos vazios dentro do PlayingPanel)")]
    public RectTransform ballsParent;   // cobre a área de jogo inteira (topo + arena)
    public RectTransform topArea;       // onde as bolas certas ficam enfileiradas
    public RectTransform arena;         // onde as bolas caem

    [Header("Visual")]
    public float maxDiameter = 130f;     // tamanho máximo das bolas na arena
    public float arenaDivisor = 5f;      // bola <= largura da arena / este valor (menor = bolas maiores)
    public bool shrinkOnTop = false;     // desmarcado = bolas de cima do MESMO tamanho das da arena
    public Sprite circleSprite;         // opcional: se vazio, gera um círculo sozinho
    public Color ballColor = new Color(1f, 0.82f, 0.12f);
    public Color wrongColor = new Color(0.93f, 0.18f, 0.18f);
    public Color numberColor = new Color(0.2f, 0.12f, 0.02f);
    public float flyTime = 0.35f;

    [Header("Física (unidades do Canvas)")]
    public float gravity = 3500f;
    [Range(0.9f, 1f)] public float damping = 0.997f;
    public int iterations = 4;
    [Range(0f, 0.8f)] public float restitution = 0.3f;
    [Range(0f, 0.5f)] public float contactDamp = 0.08f;

    [Header("Som (opcional)")]
    public AudioSource sfxSource;
    public AudioClip correctClip;
    public AudioClip wrongClip;

    const float FixedDt = 1f / 120f;
    const float BounceThreshold = 2.5f;   // px por passo

    static Sprite generatedCircle;

    readonly List<OrderBall> active = new List<OrderBall>();
    readonly List<OrderBall> all = new List<OrderBall>();

    Rect arenaRect;
    Rect topRect;
    float diameter;       // tamanho da bola na arena
    float topDiameter;    // tamanho da bola na fileira de cima
    float topScale;       // topDiameter / diameter
    float cell;
    int count;
    int placed;
    bool ascending;
    bool simulate;
    float acc;

    void OnEnable()
    {
        StartCoroutine(Begin());
    }

    void OnDisable()
    {
        simulate = false;
        StopAllCoroutines();
        Clear();
        if (OrderManager.I != null) OrderManager.I.StopRun();
    }

    IEnumerator Begin()
    {
        Clear();
        yield return null;                 // espera o layout do painel ficar pronto
        Canvas.ForceUpdateCanvases();

        var m = OrderManager.I;
        if (m == null || ballsParent == null || topArea == null || arena == null)
        {
            Debug.LogError("OrderBoard: falta o OrderManager ou as referências (Balls Parent / Top Area / Arena).");
            yield break;
        }

        count = Mathf.Max(1, m.ballCount);
        ascending = m.ascending;
        placed = 0;

        arenaRect = LocalRect(ballsParent, arena);
        topRect = LocalRect(ballsParent, topArea);

        diameter = ComputeDiameter();
        topDiameter = ComputeTopDiameter();
        topScale = topDiameter / diameter;
        cell = topDiameter * 1.12f;
        float r = diameter * 0.5f;

        // números embaralhados
        var nums = new List<int>();
        for (int i = 1; i <= count; i++) nums.Add(i);
        for (int i = nums.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = nums[i];
            nums[i] = nums[j];
            nums[j] = tmp;
        }

        Sprite sprite = circleSprite != null ? circleSprite : GetCircle();

        // as bolas nascem em grade na parte de cima da arena (sem sobrepor) e caem
        int cols = Mathf.Max(1, Mathf.FloorToInt(arenaRect.width / (diameter * 1.15f)));
        float cellW = arenaRect.width / cols;
        float slackX = Mathf.Max(0f, (cellW - diameter) * 0.45f);

        for (int i = 0; i < count; i++)
        {
            var b = OrderBall.Create(ballsParent, this, nums[i], diameter, sprite, ballColor, wrongColor, numberColor);

            int col = i % cols;
            int row = i / cols;
            float x = arenaRect.xMin + (col + 0.5f) * cellW + Random.Range(-slackX, slackX);
            float y = arenaRect.yMax - r - row * diameter * 1.1f;
            y = Mathf.Max(y, arenaRect.yMin + r);

            b.radius = r;
            b.pos = new Vector2(x, y);
            b.prev = b.pos - new Vector2(Random.Range(-1.2f, 1.2f), 0f);
            b.ApplyPosition();

            active.Add(b);
            all.Add(b);
        }

        m.StartRun();
        acc = 0f;
        simulate = true;
    }

    // tamanho na arena: o maior possível, independente da fileira de cima
    float ComputeDiameter()
    {
        return Mathf.Min(maxDiameter, arenaRect.width / Mathf.Max(1f, arenaDivisor));
    }

    // tamanho na fileira de cima: encolhe só o necessário para todas as bolas caberem
    float ComputeTopDiameter()
    {
        if (!shrinkOnTop) return diameter;   // mesmo tamanho; as fileiras descem quanto precisar

        float d = diameter;
        while (d > 30f)
        {
            float c = d * 1.12f;
            int perRow = Mathf.Max(1, Mathf.FloorToInt(topRect.width / c));
            int rows = Mathf.CeilToInt(count / (float)perRow);
            if (rows * c <= topRect.height) break;
            d -= 4f;
        }
        return d;
    }

    void Clear()
    {
        foreach (var b in all)
            if (b != null) Destroy(b.gameObject);
        all.Clear();
        active.Clear();
    }

    // ---------------- clique ----------------

    public void OnBallPressed(OrderBall ball)
    {
        var m = OrderManager.I;
        if (!simulate || m == null) return;
        if (!active.Contains(ball) || ball.Blinking) return;

        int expected = ascending ? placed + 1 : count - placed;

        if (ball.number == expected)
        {
            Place(ball);
            m.AddPlaced();
        }
        else
        {
            m.AddTry();
            ball.Blink();
            PlaySfx(wrongClip);
        }
    }

    void Place(OrderBall ball)
    {
        active.Remove(ball);          // sai da física: as outras caem no lugar dela
        ball.SetClickable(false);
        ball.transform.SetAsLastSibling();

        Vector2 slot = SlotPosition(placed);
        placed++;

        PlaySfx(correctClip);
        StartCoroutine(Fly(ball, slot, topScale));
    }

    Vector2 SlotPosition(int index)
    {
        int perRow = Mathf.Max(1, Mathf.FloorToInt(topRect.width / cell));
        int row = index / perRow;
        int col = index % perRow;

        float startX = topRect.xMin + (topRect.width - perRow * cell) * 0.5f;
        float x = startX + (col + 0.5f) * cell;
        float y = topRect.yMax - (row + 0.5f) * cell;
        return new Vector2(x, y);
    }

    IEnumerator Fly(OrderBall b, Vector2 target, float endScale)
    {
        var rt = b.RT;
        Vector3 from = rt.localPosition;
        Vector3 to = new Vector3(target.x, target.y, 0f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, flyTime);
            float e = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            rt.localPosition = Vector3.Lerp(from, to, e);
            float s = Mathf.Lerp(1f, endScale, e) + 0.18f * Mathf.Sin(e * Mathf.PI);
            rt.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        rt.localPosition = to;
        rt.localScale = new Vector3(endScale, endScale, 1f);
    }

    void PlaySfx(AudioClip clip)
    {
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
    }

    // ---------------- física ----------------

    void Update()
    {
        if (!simulate) return;

        acc += Mathf.Min(Time.deltaTime, 0.05f);
        while (acc >= FixedDt)
        {
            Step(FixedDt);
            acc -= FixedDt;
        }

        for (int i = 0; i < active.Count; i++)
            active[i].ApplyPosition();
    }

    // Verlet + projeção de posição: simples e estável para pilhas de bolas.
    void Step(float dt)
    {
        int n = active.Count;
        float g = gravity * dt * dt;

        for (int i = 0; i < n; i++)
        {
            var b = active[i];
            Vector2 vel = (b.pos - b.prev) * damping;
            b.prev = b.pos;
            b.pos += vel + new Vector2(0f, -g);
            b.vyBefore = b.pos.y - b.prev.y;
            b.touched = false;
        }

        for (int it = 0; it < iterations; it++)
        {
            // bola x bola
            for (int i = 0; i < n; i++)
            {
                var a = active[i];
                for (int j = i + 1; j < n; j++)
                {
                    var c = active[j];
                    Vector2 d = c.pos - a.pos;
                    float minD = a.radius + c.radius;
                    float sq = d.sqrMagnitude;
                    if (sq >= minD * minD) continue;

                    float dist = Mathf.Sqrt(sq);
                    Vector2 nrm = dist > 0.0001f ? d / dist : Vector2.up;
                    float push = (minD - dist) * 0.5f;
                    a.pos -= nrm * push;
                    c.pos += nrm * push;
                    a.touched = true;
                    c.touched = true;
                }
            }

            // paredes e chão
            for (int i = 0; i < n; i++)
            {
                var b = active[i];
                float minX = arenaRect.xMin + b.radius;
                float maxX = arenaRect.xMax - b.radius;
                float minY = arenaRect.yMin + b.radius;

                if (b.pos.x < minX) { b.pos.x = minX; b.touched = true; }
                else if (b.pos.x > maxX) { b.pos.x = maxX; b.touched = true; }

                if (b.pos.y < minY) { b.pos.y = minY; b.touched = true; }
            }
        }

        for (int i = 0; i < n; i++)
        {
            var b = active[i];
            float minY = arenaRect.yMin + b.radius;

            // quicar no chão (só se a batida for forte o bastante)
            if (b.pos.y <= minY + 0.01f && b.vyBefore < -BounceThreshold)
                b.prev.y = b.pos.y + b.vyBefore * restitution;

            if (b.touched)
            {
                // atrito: tira um pouco da velocidade de quem está encostado
                b.prev = Vector2.Lerp(b.prev, b.pos, contactDamp);

                // evita ficar "deslizando" bem devagar
                if ((b.pos - b.prev).sqrMagnitude < 0.0009f) b.prev = b.pos;
            }
        }
    }

    // ---------------- utilidades ----------------

    static Rect LocalRect(RectTransform parent, RectTransform child)
    {
        var c = new Vector3[4];
        child.GetWorldCorners(c);
        Vector3 min = parent.InverseTransformPoint(c[0]);
        Vector3 max = parent.InverseTransformPoint(c[2]);
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    // círculo branco com borda cinza; a cor da bola vem do Image.color
    static Sprite GetCircle()
    {
        if (generatedCircle != null) return generatedCircle;

        const int S = 128;
        float r = S * 0.5f;
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        var px = new Color32[S * S];
        for (int y = 0; y < S; y++)
        {
            for (int x = 0; x < S; x++)
            {
                float dx = x + 0.5f - r;
                float dy = y + 0.5f - r;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(r - d);                 // borda suave
                byte gray = (byte)(d > r - 7f ? 165 : 255);     // borda mais escura
                px[y * S + x] = new Color32(gray, gray, gray, (byte)(a * 255f));
            }
        }
        tex.SetPixels32(px);
        tex.Apply();

        generatedCircle = Sprite.Create(tex, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), 100f);
        return generatedCircle;
    }
}