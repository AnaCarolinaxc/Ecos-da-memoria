#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Automatiza o "fundo até a borda do celular" em todas as telas dentro do SafeArea,
// SEM mexer na posição do conteúdo.
// Menu: Tools > Ecos > Telas
//   1 - Restaurar tamanho original dos painéis : desfaz o "esticar" (volta para 1080x1920 fixo)
//   2 - Criar fundos até a borda                : cria um filho "Background" + SafeAreaBackground
//   3 - Remover fundos                          : desfaz o passo 2
// Tudo suporta Ctrl+Z.
public static class SafeAreaBackgroundTool
{
    const string BG = "Background";

    // tamanho original dos painéis dos jogos (resolução de referência do Canvas Scaler)
    static readonly Vector2 DesignSize = new Vector2(1080f, 1920f);
    // tamanho original dos containers dos jogos (UneaPalavraGame, PuzzleGame, ...)
    static readonly Vector2 ContainerSize = new Vector2(100f, 100f);

    // ---------- utilidades ----------

    static bool IsStretched(RectTransform r) =>
        r != null && r.anchorMin == Vector2.zero && r.anchorMax == Vector2.one;

    static bool IsStretchedZero(RectTransform r) =>
        IsStretched(r) && r.offsetMin.sqrMagnitude < 0.001f && r.offsetMax.sqrMagnitude < 0.001f;

    // painel "de fundo": esticado (âncoras) e com Image praticamente opaca
    static Image OpaqueStretchedImage(Transform t)
    {
        var r = t as RectTransform;
        if (!IsStretched(r)) return null;
        var img = t.GetComponent<Image>();
        return (img != null && img.color.a >= 0.95f) ? img : null;
    }

    static Transform FindSafeArea()
    {
        var f = Object.FindFirstObjectByType<SafeAreaFitter>(FindObjectsInactive.Include);
        return f != null ? f.transform : null;
    }

    static bool HasBackground(Transform panel)
    {
        var b = panel.Find(BG);
        return b != null && b.GetComponent<SafeAreaBackground>() != null;
    }

    static string PathOf(Transform t, Transform root)
    {
        var parts = new List<string>();
        for (var c = t; c != null && c != root; c = c.parent) parts.Add(c.name);
        parts.Reverse();
        return string.Join("/", parts);
    }

    static bool TryGetSafeArea(out Transform safe)
    {
        safe = FindSafeArea();
        if (safe == null)
            EditorUtility.DisplayDialog("Telas",
                "Não achei o SafeArea (SafeAreaFitter) na cena aberta.\nAbra a cena Main e tente de novo.", "OK");
        return safe != null;
    }

    // ---------- 1) restaurar tamanho original ----------

    [MenuItem("Tools/Ecos/Telas/1 - Restaurar tamanho original dos painéis")]
    static void RestoreSize()
    {
        if (!TryGetSafeArea(out var safe)) return;

        var containers = new List<RectTransform>();
        var panels = new List<RectTransform>();

        foreach (Transform c in safe)
        {
            if (c.name == BG) continue;
            if (c.GetComponent<Image>() != null) continue;      // telas diretas (SelectPanel...) não mudam
            var rc = c as RectTransform;
            if (!IsStretchedZero(rc)) continue;                  // só o que foi esticado antes

            var ps = new List<RectTransform>();
            foreach (Transform p in c)
            {
                if (p.name == BG) continue;
                var rp = p as RectTransform;
                if (IsStretchedZero(rp) && p.GetComponent<Image>() != null) ps.Add(rp);
            }
            if (ps.Count == 0) continue;

            containers.Add(rc);
            panels.AddRange(ps);
        }

        if (containers.Count == 0)
        {
            EditorUtility.DisplayDialog("Telas",
                "Nada para restaurar: os painéis já estão no tamanho original.", "OK");
            return;
        }

        string list = string.Join("\n", containers.Select(c => "• " + c.name + $" ({c.childCount} filhos)"));
        if (!EditorUtility.DisplayDialog("Restaurar tamanho original",
                $"Vou voltar {containers.Count} jogos e {panels.Count} painéis para o tamanho fixo original (1080x1920):\n\n{list}\n\nContinuar? (Ctrl+Z desfaz)",
                "Restaurar", "Cancelar"))
            return;

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Restaurar tamanho dos painéis");
        int group = Undo.GetCurrentGroup();

        foreach (var c in containers)
        {
            Undo.RecordObject(c, "Restaurar container");
            c.anchorMin = new Vector2(0.5f, 0.5f);
            c.anchorMax = new Vector2(0.5f, 0.5f);
            c.anchoredPosition = Vector2.zero;
            c.sizeDelta = ContainerSize;
            EditorUtility.SetDirty(c);
        }

        // painel esticado em um container 100x100 com sizeDelta (980,1820) => 1080x1920 fixo
        Vector2 delta = DesignSize - ContainerSize;
        foreach (var p in panels)
        {
            Undo.RecordObject(p, "Restaurar painel");
            p.anchorMin = Vector2.zero;
            p.anchorMax = Vector2.one;
            p.offsetMin = -delta * 0.5f;
            p.offsetMax = delta * 0.5f;
            EditorUtility.SetDirty(p);
        }

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(safe.gameObject.scene);
        EditorUtility.DisplayDialog("Telas",
            $"Pronto: {containers.Count} jogos e {panels.Count} painéis restaurados.", "OK");
    }

    // ---------- 2) criar fundos ----------

    [MenuItem("Tools/Ecos/Telas/2 - Criar fundos até a borda")]
    static void CreateBackgrounds()
    {
        if (!TryGetSafeArea(out var safe)) return;

        // telas = painéis esticados com Image opaca, diretos no SafeArea ou dentro de containers dos jogos
        var screens = new List<Image>();
        foreach (Transform c in safe)
        {
            if (c.name == BG) continue;
            var img = OpaqueStretchedImage(c);
            if (img != null) { screens.Add(img); continue; }
            foreach (Transform p in c)
            {
                if (p.name == BG) continue;
                var pi = OpaqueStretchedImage(p);
                if (pi != null) screens.Add(pi);
            }
        }

        var targets = screens.Where(i => !HasBackground(i.transform)).ToList();
        if (targets.Count == 0)
        {
            EditorUtility.DisplayDialog("Telas", "Nenhuma tela nova para processar (todas já têm Background).", "OK");
            return;
        }

        string list = string.Join("\n", targets.Select(i => "• " + PathOf(i.transform, safe)));
        if (!EditorUtility.DisplayDialog("Criar fundos até a borda",
                $"Vou criar um Background em {targets.Count} telas:\n\n{list}\n\nContinuar? (Ctrl+Z desfaz)",
                "Criar", "Cancelar"))
            return;

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Criar fundos até a borda");
        int group = Undo.GetCurrentGroup();

        foreach (var src in targets)
        {
            var panel = src.transform;

            var go = new GameObject(BG, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Criar Background");
            go.layer = panel.gameObject.layer;
            go.transform.SetParent(panel, false);
            go.transform.SetAsFirstSibling();          // atrás do conteúdo

            // no editor ele aparece igual ao painel; em Play o script expande até a borda da tela
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var bg = go.AddComponent<Image>();
            EditorUtility.CopySerialized(src, bg);     // copia sprite, cor, tipo, material...
            bg.raycastTarget = false;

            go.AddComponent<SafeAreaBackground>();

            // se o painel usa Layout Group, o fundo não pode entrar no layout
            if (panel.GetComponent<LayoutGroup>() != null)
            {
                var le = go.AddComponent<LayoutElement>();
                le.ignoreLayout = true;
            }

            // painel original fica invisível (mantém o Image para bloquear cliques)
            Undo.RecordObject(src, "Esconder fundo do painel");
            var col = src.color; col.a = 0f; src.color = col;
            EditorUtility.SetDirty(src);
        }

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(safe.gameObject.scene);
        EditorUtility.DisplayDialog("Telas", $"Pronto: {targets.Count} fundos criados.\nSalve a cena (Ctrl+S).", "OK");
    }

    // ---------- 3) remover ----------

    [MenuItem("Tools/Ecos/Telas/3 - Remover fundos até a borda")]
    static void RemoveBackgrounds()
    {
        if (!TryGetSafeArea(out var safe)) return;

        var bgs = safe.GetComponentsInChildren<SafeAreaBackground>(true);
        if (bgs.Length == 0)
        {
            EditorUtility.DisplayDialog("Telas", "Não há Backgrounds para remover.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Remover fundos",
                $"Remover {bgs.Length} Backgrounds e devolver o visual aos painéis originais?", "Remover", "Cancelar"))
            return;

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Remover fundos até a borda");
        int group = Undo.GetCurrentGroup();

        foreach (var b in bgs)
        {
            var parentImg = b.transform.parent != null ? b.transform.parent.GetComponent<Image>() : null;
            var bgImg = b.GetComponent<Image>();
            if (parentImg != null && bgImg != null)
            {
                Undo.RecordObject(parentImg, "Restaurar painel");
                parentImg.color = bgImg.color;
                EditorUtility.SetDirty(parentImg);
            }
            Undo.DestroyObjectImmediate(b.gameObject);
        }

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(safe.gameObject.scene);
    }
}
#endif