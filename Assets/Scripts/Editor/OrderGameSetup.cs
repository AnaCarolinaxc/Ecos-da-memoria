using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Monta (ou refaz) toda a ligação do "Acerte a ordem" na cena aberta.
// Menu: Tools > Ecos da Memória > Montar Acerte a Ordem
//
// Pode rodar de novo quando quiser: ele reaproveita o que já existe e só refaz as ligações.
// Colocar este arquivo em Assets/Scripts/Editor/.
public static class OrderGameSetup
{
    // Medidas (Canvas de referência 1080 x 1920)
    const float SideMargin = 40f;
    const float TopAreaHeight = 450f;       // 3 fileiras de bolas de ~130
    const float Gap = 30f;
    const float BottomMargin = 60f;
    const float FallbackTopMargin = 480f;   // usado se não achar o texto do tempo

    static readonly List<string> log = new List<string>();
    static int warnings;

    [MenuItem("Tools/Ecos da Memória/Montar Acerte a Ordem")]
    public static void Run()
    {
        log.Clear();
        warnings = 0;

        Scene scene = SceneManager.GetActiveScene();
        Transform game = Path(scene, "Canvas/SafeArea/AcerteaOrdemGame");
        if (game == null)
        {
            EditorUtility.DisplayDialog("Acerte a Ordem",
                "Não achei Canvas/SafeArea/AcerteaOrdemGame na cena aberta.\nAbra a cena Main e rode de novo.", "OK");
            return;
        }

        UIRouter router = Object.FindFirstObjectByType<UIRouter>(FindObjectsInactive.Include);
        if (router == null)
        {
            EditorUtility.DisplayDialog("Acerte a Ordem", "Não achei o objeto UIRouter na cena.", "OK");
            return;
        }

        Transform home = game.Find("HomePanel");
        Transform config = game.Find("ConfigPanel");
        Transform playing = game.Find("PlayingPanel");
        Transform ranking = game.Find("RankingPanel");
        if (home == null || config == null || playing == null || ranking == null)
        {
            EditorUtility.DisplayDialog("Acerte a Ordem",
                "Faltam painéis dentro de AcerteaOrdemGame (HomePanel, ConfigPanel, PlayingPanel, RankingPanel).", "OK");
            return;
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Montar Acerte a Ordem");
        int undoGroup = Undo.GetCurrentGroup();

        // 1) objeto raiz do jogo precisa estar ativo
        if (!game.gameObject.activeSelf)
        {
            Undo.RecordObject(game.gameObject, "Ativar AcerteaOrdemGame");
            game.gameObject.SetActive(true);
            Log("AcerteaOrdemGame ativado");
        }

        // 2) UIRouter
        SetupRouter(router, home, config, playing, ranking);

        // 3) OrderManager
        SetupManager(playing);

        // 4) telas
        SetupHome(home, router);
        SetupConfig(config, router);
        SetupPlaying(scene, playing, router);
        SetupRanking(scene, ranking, router);

        // 5) FormularioManager
        SetupForm(ranking);

        // 6) botão do jogo na tela de seleção
        SetupSelect(scene, router);

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene);

        Selection.activeTransform = game;
        Debug.Log("=== Montar Acerte a Ordem ===\n" + string.Join("\n", log));

        EditorUtility.DisplayDialog("Acerte a Ordem",
            "Pronto! " + log.Count + " ajustes feitos"
            + (warnings > 0 ? " (" + warnings + " avisos, veja o Console)" : "") + ".\n\n"
            + (saved ? "A cena foi salva." : "ATENÇÃO: não consegui salvar a cena, aperte Ctrl+S.")
            + "\n\nAgora commite o Main.unity junto com os arquivos .meta.", "OK");
    }

    // ------------------------------------------------------------------ UIRouter

    static void SetupRouter(UIRouter router, Transform home, Transform config, Transform playing, Transform ranking)
    {
        var so = new SerializedObject(router);
        SetRef(so, "aog_homePanel", home.gameObject);
        SetRef(so, "aog_configPanel", config.gameObject);
        SetRef(so, "aog_playingPanel", playing.gameObject);
        SetRef(so, "aog_rankingPanel", ranking.gameObject);
        so.ApplyModifiedProperties();
        Log("UIRouter: painéis aog_ ligados");
    }

    // ------------------------------------------------------------------ OrderManager

    static void SetupManager(Transform playing)
    {
        var mgr = Object.FindFirstObjectByType<OrderManager>(FindObjectsInactive.Include);
        if (mgr == null)
        {
            var go = new GameObject("OrderManager");
            Undo.RegisterCreatedObjectUndo(go, "Criar OrderManager");
            mgr = Undo.AddComponent<OrderManager>(go);
            Log("OrderManager criado na raiz da cena");
        }

        Undo.RecordObject(mgr, "OrderManager");
        Transform time = playing.Find("Time");
        if (time != null) mgr.timeText = time.GetComponent<TMP_Text>();
        else Warn("PlayingPanel/Time não encontrado: o tempo não vai aparecer");

        Transform tries = playing.Find("Tentativas");
        mgr.triesText = tries != null ? tries.GetComponent<TMP_Text>() : null;

        EditorUtility.SetDirty(mgr);
        Log("OrderManager: texto do tempo ligado");
    }

    // ------------------------------------------------------------------ HomePanel

    static void SetupHome(Transform home, UIRouter router)
    {
        var buttons = new List<Button>();
        foreach (Transform child in home)
        {
            var b = child.GetComponent<Button>();
            if (b != null && child.name == "Button") buttons.Add(b);
        }

        Button playBtn = null, rankBtn = null;
        foreach (var b in buttons)
        {
            var label = b.GetComponentInChildren<TMP_Text>(true);
            string t = label != null ? label.text.ToLowerInvariant() : "";
            if (t.Contains("jog")) playBtn = b;
            else if (t.Contains("rank")) rankBtn = b;
        }
        // se os textos não ajudarem, usa a ordem: primeiro = jogar, segundo = ranking
        if (playBtn == null && buttons.Count > 0) playBtn = buttons[0];
        if (rankBtn == null && buttons.Count > 1) rankBtn = buttons[1];

        if (playBtn != null) { SetOnClick(playBtn, router.Show_aogConfig); Log("HomePanel: botão Jogar -> Show_aogConfig"); }
        else Warn("HomePanel: botão de jogar não encontrado");

        if (rankBtn != null) { SetOnClick(rankBtn, router.Show_aogRanking); Log("HomePanel: botão Ranking -> Show_aogRanking"); }
        else Warn("HomePanel: botão de ranking não encontrado");

        var arrow = ButtonOf(home.Find("Arrow"), "HomePanel/Arrow");
        if (arrow != null) SetOnClick(arrow, router.ShowSelect);
    }

    // ------------------------------------------------------------------ ConfigPanel

    static void SetupConfig(Transform config, UIRouter router)
    {
        Transform cresce = config.Find("CresceBTN");
        Transform decresce = config.Find("DecresceBTN");
        Transform play = config.Find("PlayBTN");
        Transform arrow = config.Find("Arrow");

        Transform b5 = FindDeep(config, "5BTN");
        Transform b10 = FindDeep(config, "10BTN") ?? FindDeep(config, "BTN (1)");
        Transform b15 = FindDeep(config, "15BTN") ?? FindDeep(config, "2X2BTN (2)");
        Transform dtxt = FindDeep(config, "dificuldadeTXT");

        if (cresce == null || decresce == null || play == null || b5 == null || b10 == null || b15 == null)
        {
            Warn("ConfigPanel: faltam botões (CresceBTN, DecresceBTN, PlayBTN, 5BTN, BTN (1), 2X2BTN (2))");
            return;
        }

        // renomeia e põe o texto dos botões de dificuldade
        Rename(b10, "10BTN");
        Rename(b15, "15BTN");
        SetLabel(b5, "5");
        SetLabel(b10, "10");
        SetLabel(b15, "15");

        // grupo da dificuldade (some até escolher a ordem)
        Transform group = config.Find("DificuldadeGroup");
        if (group == null)
        {
            var go = new GameObject("DificuldadeGroup", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Criar DificuldadeGroup");
            go.transform.SetParent(config, false);
            Stretch((RectTransform)go.transform);
            group = go.transform;
            Log("ConfigPanel: DificuldadeGroup criado");
        }
        foreach (var t in new[] { dtxt, b5, b10, b15 })
        {
            if (t != null && t.parent != group)
                Undo.SetTransformParent(t, group, "Agrupar dificuldade");
        }

        // controlador
        var ctrl = GetOrAdd<OrderConfigController>(config.gameObject);
        Undo.RecordObject(ctrl, "OrderConfigController");
        ctrl.uiRouter = router;
        ctrl.orderButtonImages = new[] { cresce.GetComponent<Image>(), decresce.GetComponent<Image>() };
        ctrl.difficultyGroup = group.gameObject;
        ctrl.difficultyButtonImages = new[] { b5.GetComponent<Image>(), b10.GetComponent<Image>(), b15.GetComponent<Image>() };
        ctrl.playButton = play.gameObject;
        if (ctrl.difficultyValues == null || ctrl.difficultyValues.Length != 3)
            ctrl.difficultyValues = new[] { 5, 10, 15 };
        EditorUtility.SetDirty(ctrl);

        // OnClicks (apaga os herdados de outros jogos)
        SetOnClickInt(ButtonOf(cresce, "CresceBTN"), ctrl.SelectOrder, 0);
        SetOnClickInt(ButtonOf(decresce, "DecresceBTN"), ctrl.SelectOrder, 1);
        SetOnClickInt(ButtonOf(b5, "5BTN"), ctrl.SelectDifficulty, 0);
        SetOnClickInt(ButtonOf(b10, "10BTN"), ctrl.SelectDifficulty, 1);
        SetOnClickInt(ButtonOf(b15, "15BTN"), ctrl.SelectDifficulty, 2);
        SetOnClick(ButtonOf(play, "PlayBTN"), ctrl.Play);
        if (arrow != null) SetOnClick(ButtonOf(arrow, "ConfigPanel/Arrow"), router.Show_aogHome);

        Log("ConfigPanel: OrderConfigController e OnClicks ligados");
    }

    // ------------------------------------------------------------------ PlayingPanel

    static void SetupPlaying(Scene scene, Transform playing, UIRouter router)
    {
        var panel = (RectTransform)playing;
        float topMargin = ComputeTopMargin(panel, playing.Find("Time"));

        var ballsLayer = GetOrCreateRect(playing, "BallsLayer");
        Stretch(ballsLayer);
        ballsLayer.SetSiblingIndex(0);   // fica atrás de Quit e do texto

        var topArea = GetOrCreateRect(playing, "TopArea");
        topArea.anchorMin = new Vector2(0f, 1f);
        topArea.anchorMax = new Vector2(1f, 1f);
        topArea.pivot = new Vector2(0.5f, 1f);
        topArea.sizeDelta = new Vector2(-2f * SideMargin, TopAreaHeight);
        topArea.anchoredPosition = new Vector2(0f, -topMargin);

        var arena = GetOrCreateRect(playing, "Arena");
        arena.anchorMin = Vector2.zero;
        arena.anchorMax = Vector2.one;
        arena.pivot = new Vector2(0.5f, 0.5f);
        arena.offsetMin = new Vector2(SideMargin, BottomMargin);
        arena.offsetMax = new Vector2(-SideMargin, -(topMargin + TopAreaHeight + Gap));

        EditorUtility.SetDirty(ballsLayer);
        EditorUtility.SetDirty(topArea);
        EditorUtility.SetDirty(arena);

        var src = GetOrAdd<AudioSource>(playing.gameObject);
        Undo.RecordObject(src, "AudioSource");
        src.playOnAwake = false;

        var board = GetOrAdd<OrderBoard>(playing.gameObject);
        Undo.RecordObject(board, "OrderBoard");
        board.ballsParent = ballsLayer;
        board.topArea = topArea;
        board.arena = arena;
        board.sfxSource = src;
        EditorUtility.SetDirty(board);

        var win = GetOrAdd<OrderWinToRanking>(playing.gameObject);
        Undo.RecordObject(win, "OrderWinToRanking");
        win.uiRouter = router;
        win.sfxSource = src;

        // copia os sons de vitória do Une Palavra (se existirem)
        Transform wordPlaying = Path(scene, "Canvas/SafeArea/UneaPalavraGame/PlayingPanel");
        var wordWin = wordPlaying != null ? wordPlaying.GetComponent<WordWinToRanking>() : null;
        if (wordWin != null)
        {
            if (wordWin.sfxSource != null) win.sfxSource = wordWin.sfxSource;
            win.victoryClips = wordWin.victoryClips;
        }
        EditorUtility.SetDirty(win);

        var quit = ButtonOf(playing.Find("Quit"), "PlayingPanel/Quit");
        if (quit != null) SetOnClick(quit, router.Show_aogHome);

        Log("PlayingPanel: BallsLayer, TopArea, Arena, OrderBoard e OrderWinToRanking prontos (topo a "
            + Mathf.RoundToInt(topMargin) + " px)");
    }

    static float ComputeTopMargin(RectTransform panel, Transform time)
    {
        if (time == null) return FallbackTopMargin;
        var t = (RectTransform)time;
        bool centered = t.anchorMin == new Vector2(0.5f, 0.5f) && t.anchorMax == new Vector2(0.5f, 0.5f)
                        && Mathf.Approximately(t.pivot.y, 0.5f);
        if (!centered) return FallbackTopMargin;

        float h = panel.rect.height > 0f ? panel.rect.height : 1920f;
        float bottomFromTop = h * 0.5f - t.anchoredPosition.y + t.rect.height * 0.5f;
        return Mathf.Round(bottomFromTop + Gap);
    }

    // ------------------------------------------------------------------ RankingPanel

    static void SetupRanking(Scene scene, Transform ranking, UIRouter router)
    {
        var ui = GetOrAdd<RankingUI>(ranking.gameObject);
        Undo.RecordObject(ui, "RankingUI");
        ui.content = ranking.Find("ListContainer");
        ui.gameId = "order";

        Transform wordRanking = Path(scene, "Canvas/SafeArea/UneaPalavraGame/RankingPanel");
        var wordUI = wordRanking != null ? wordRanking.GetComponent<RankingUI>() : null;
        if (wordUI != null && wordUI.rowPrefab != null) ui.rowPrefab = wordUI.rowPrefab;
        else if (ui.rowPrefab == null) Warn("RankingPanel: não achei o Row Prefab (arraste manualmente no RankingUI)");
        EditorUtility.SetDirty(ui);

        var form = Object.FindFirstObjectByType<FormularioManager>(FindObjectsInactive.Include);

        var open = ButtonOf(ranking.Find("Button"), "RankingPanel/Button");
        if (open != null && form != null) SetOnClick(open, form.AbrirFormulario_aog);

        var again = ButtonOf(ranking.Find("Button (1)"), "RankingPanel/Button (1)");
        if (again != null) SetOnClick(again, router.Show_aogConfig);

        var arrow = ButtonOf(ranking.Find("Arrow"), "RankingPanel/Arrow");
        if (arrow != null) SetOnClick(arrow, router.Show_aogHome);

        // o FormularioManager liga o "fechar" por código; tira o OnClick herdado
        var close = ButtonOf(ranking.Find("PopupFormulario/Panel/Header/BrnFechar"), "PopupFormulario/BrnFechar");
        if (close != null) ClearOnClick(close);

        Log("RankingPanel: RankingUI (gameId = order) e botões ligados");
    }

    // ------------------------------------------------------------------ FormularioManager

    static void SetupForm(Transform ranking)
    {
        var form = Object.FindFirstObjectByType<FormularioManager>(FindObjectsInactive.Include);
        if (form == null) { Warn("FormularioManager não encontrado na cena"); return; }

        Transform popup = ranking.Find("PopupFormulario");
        Transform content = ranking.Find("PopupFormulario/Panel/ScrollView/Viewport/Content");
        Transform send = ranking.Find("PopupFormulario/Panel/BtnEnviar");
        Transform close = ranking.Find("PopupFormulario/Panel/Header/BrnFechar");
        if (popup == null || content == null || send == null || close == null)
        {
            Warn("PopupFormulario: faltam objetos (Panel/ScrollView/Viewport/Content, BtnEnviar, Header/BrnFechar)");
            return;
        }

        var so = new SerializedObject(form);
        SetRef(so, "popupFormulario_aog", popup.gameObject);
        SetRef(so, "containerPerguntas_aog", content);
        SetRef(so, "btnEnviar_aog", send.GetComponent<Button>());
        SetRef(so, "btnFechar_aog", close.GetComponent<Button>());

        // o prefab de pergunta é o mesmo dos outros jogos
        var prefabUpg = so.FindProperty("prefabPergunta_upg");
        var prefabAog = so.FindProperty("prefabPergunta_aog");
        if (prefabUpg != null && prefabAog != null && prefabUpg.objectReferenceValue != null)
            prefabAog.objectReferenceValue = prefabUpg.objectReferenceValue;
        else if (prefabAog != null && prefabAog.objectReferenceValue == null)
            Warn("FormularioManager: arraste o PrefabPergunta em 'Prefab Pergunta Aog'");

        so.ApplyModifiedProperties();
        Log("FormularioManager: campos _aog ligados");
    }

    // ------------------------------------------------------------------ SelectPanel

    static void SetupSelect(Scene scene, UIRouter router)
    {
        Transform t = Path(scene, "Canvas/SafeArea/SelectPanel/aog_Button");
        var b = ButtonOf(t, "SelectPanel/aog_Button");
        if (b != null)
        {
            SetOnClick(b, router.Show_aogHome);
            Log("SelectPanel: aog_Button -> Show_aogHome");
        }
    }

    // ------------------------------------------------------------------ utilidades

    static Transform Path(Scene scene, string path)
    {
        int i = path.IndexOf('/');
        string rootName = i < 0 ? path : path.Substring(0, i);
        foreach (var r in scene.GetRootGameObjects())
        {
            if (r.name != rootName) continue;
            return i < 0 ? r.transform : r.transform.Find(path.Substring(i + 1));
        }
        return null;
    }

    static Transform FindDeep(Transform root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }

    static T GetOrAdd<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        if (c == null) c = Undo.AddComponent<T>(go);
        return c;
    }

    static RectTransform GetOrCreateRect(Transform parent, string name)
    {
        Transform t = parent.Find(name);
        if (t == null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Criar " + name);
            go.transform.SetParent(parent, false);
            t = go.transform;
        }
        else
        {
            Undo.RecordObject(t, "Ajustar " + name);
        }
        return (RectTransform)t;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void Rename(Transform t, string newName)
    {
        if (t.name == newName) return;
        Undo.RecordObject(t.gameObject, "Renomear");
        t.name = newName;
    }

    static void SetLabel(Transform button, string text)
    {
        var label = button.GetComponentInChildren<TMP_Text>(true);
        if (label == null) return;
        Undo.RecordObject(label, "Texto do botão");
        label.text = text;
        EditorUtility.SetDirty(label);
    }

    static void SetRef(SerializedObject so, string field, Object value)
    {
        var p = so.FindProperty(field);
        if (p == null) { Warn("Campo não encontrado: " + field); return; }
        p.objectReferenceValue = value;
    }

    static Button ButtonOf(Transform t, string what)
    {
        if (t == null) { Warn("Não achei: " + what); return null; }
        var b = t.GetComponent<Button>();
        if (b == null) Warn("Sem componente Button: " + what);
        return b;
    }

    static void ClearOnClick(Button b)
    {
        Undo.RecordObject(b, "Limpar OnClick");
        for (int i = b.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(b.onClick, i);
        EditorUtility.SetDirty(b);
    }

    static void SetOnClick(Button b, UnityAction call)
    {
        if (b == null) return;
        ClearOnClick(b);
        UnityEventTools.AddPersistentListener(b.onClick, call);
        EditorUtility.SetDirty(b);
    }

    static void SetOnClickInt(Button b, UnityAction<int> call, int arg)
    {
        if (b == null) return;
        ClearOnClick(b);
        UnityEventTools.AddIntPersistentListener(b.onClick, call, arg);
        EditorUtility.SetDirty(b);
    }

    static void Log(string msg) => log.Add(msg);

    static void Warn(string msg)
    {
        warnings++;
        Debug.LogWarning("[Acerte a Ordem] " + msg);
    }
}
