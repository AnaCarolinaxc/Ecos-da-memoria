using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordBoard : MonoBehaviour
{
    [Header("Colunas (com VerticalLayoutGroup)")]
    public Transform imageColumn;   // esquerda
    public Transform wordColumn;    // direita

    [Header("Prefabs (com WordItemView)")]
    public WordItemView imageItemPrefab;
    public WordItemView wordItemPrefab;

    [Header("Áudio (\"Toque em qualquer item para ouvir\")")]
    public AudioSource itemSource;

    [Header("Feedback de erro")]
    public int blinkCount = 3;
    public float blinkInterval = 0.12f;

    readonly List<WordItemView> items = new List<WordItemView>();
    WordItemView selected;
    bool locked;

    public void StartGame()
    {
        var wm = WordManager.I;
        if (wm == null || !wm.HasConfig)
        {
            Debug.LogError("WordBoard: WordManager sem tema/dificuldade configurados.");
            return;
        }

        ClearBoard();
        if (!BuildBoard(wm.themeFolder, wm.pairCount)) return;

        wm.StartRun();
    }

    const string ResourcesRoot = "UnePalavra";

    class Entry
    {
        public Sprite image;
        public string word;
        public AudioClip audio;
    }

    bool BuildBoard(string themeFolder, int pairCount)
    {
        string path = $"{ResourcesRoot}/{themeFolder}";
        var pool = LoadEntries(path);

        if (pool.Count < pairCount)
        {
            Debug.LogError($"WordBoard: a pasta 'Resources/{path}' tem {pool.Count} imagens válidas, mas a dificuldade pede {pairCount}.");
            return false;
        }

        // sorteio novo a cada partida: embaralha tudo e pega só os primeiros N
        Shuffle(pool);

        var imageOrder = new List<int>();
        var wordOrder = new List<int>();
        for (int i = 0; i < pairCount; i++)
        {
            imageOrder.Add(i);
            wordOrder.Add(i);
        }
        // embaralha cada coluna separadamente
        Shuffle(imageOrder);
        Shuffle(wordOrder);

        foreach (int id in imageOrder)
        {
            var item = Instantiate(imageItemPrefab, imageColumn);
            item.Init(this, id, true, pool[id].image, pool[id].word, pool[id].audio);
            items.Add(item);
        }
        foreach (int id in wordOrder)
        {
            var item = Instantiate(wordItemPrefab, wordColumn);
            item.Init(this, id, false, pool[id].image, pool[id].word, pool[id].audio);
            items.Add(item);
        }
        return true;
    }

    // Lê as imagens da pasta. A palavra vem do nome do arquivo.
    List<Entry> LoadEntries(string path)
    {
        var list = new List<Entry>();

        var audios = new Dictionary<string, AudioClip>();
        foreach (var clip in Resources.LoadAll<AudioClip>(path))
            audios[clip.name.ToLowerInvariant()] = clip;

        var seen = new HashSet<string>();
        foreach (var sprite in Resources.LoadAll<Sprite>(path))
        {
            string word = ToDisplayName(sprite.name);
            if (string.IsNullOrEmpty(word)) continue;
            if (!seen.Add(word.ToLowerInvariant())) continue; // evita duas palavras iguais

            audios.TryGetValue(sprite.name.ToLowerInvariant(), out var audio);
            list.Add(new Entry { image = sprite, word = word, audio = audio });
        }
        return list;
    }

    // "micro-ondas" -> "Micro-ondas" | "pe_de_moleque" -> "Pe de moleque"
    static string ToDisplayName(string fileName)
    {
        string s = fileName.Replace('_', ' ').Trim();
        if (s.Length == 0) return s;
        return char.ToUpper(s[0]) + s.Substring(1);
    }

    public void OnItemClicked(WordItemView item)
    {
        if (locked || item.IsLocked) return;

        PlayClip(item.Clip);

        // nada selecionado ainda
        if (selected == null)
        {
            selected = item;
            item.SetState(WordItemState.Selected);
            return;
        }

        // clicou no mesmo: desmarca
        if (selected == item)
        {
            item.SetState(WordItemState.Normal);
            selected = null;
            return;
        }

        // clicou na mesma coluna: troca a seleção
        if (selected.IsImage == item.IsImage)
        {
            selected.SetState(WordItemState.Normal);
            selected = item;
            item.SetState(WordItemState.Selected);
            return;
        }

        // colunas opostas: avalia o par
        var a = selected;
        var b = item;
        selected = null;

        WordManager.I.AddTry();

        if (a.PairId == b.PairId)
        {
            a.SetState(WordItemState.Correct);
            b.SetState(WordItemState.Correct);
            WordManager.I.AddMatchedPair(); // se for o último, dispara a vitória
        }
        else
        {
            StartCoroutine(WrongRoutine(a, b));
        }
    }

    IEnumerator WrongRoutine(WordItemView a, WordItemView b)
    {
        locked = true;
        a.BlinkWrong(blinkCount, blinkInterval);
        b.BlinkWrong(blinkCount, blinkInterval);
        yield return new WaitForSeconds(blinkCount * 2f * blinkInterval);
        locked = false;
    }

    void PlayClip(AudioClip clip)
    {
        if (itemSource == null || clip == null) return;
        itemSource.Stop();
        itemSource.clip = clip;
        itemSource.Play();
    }

    void ClearBoard()
    {
        StopAllCoroutines();
        items.Clear();
        selected = null;
        locked = false;

        if (imageColumn != null)
            for (int i = imageColumn.childCount - 1; i >= 0; i--)
                Destroy(imageColumn.GetChild(i).gameObject);

        if (wordColumn != null)
            for (int i = wordColumn.childCount - 1; i >= 0; i--)
                Destroy(wordColumn.GetChild(i).gameObject);
    }

    static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}