using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalleryUI : MonoBehaviour
{
    [Header("UI")]
    public Transform contentParent;
    public GameObject thumbPrefab;

    // guarda sprites (não Texture2D)
    private readonly List<Sprite> sprites = new();

    // miniaturas criadas, pra poder apagar no reset
    private readonly List<GameObject> thumbs = new();

    public void AddImage(Texture2D tex)
    {
        var sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );

        sprites.Add(sprite);

        var go = Instantiate(thumbPrefab, contentParent);
        thumbs.Add(go);

        var img = go.GetComponentInChildren<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
    }

    public IReadOnlyList<Sprite> GetAllSprites() => sprites;

    // Apaga todas as imagens escolhidas (usado quando a config recomeça do zero)
    public void Clear()
    {
        foreach (var t in thumbs)
            if (t != null) Destroy(t);
        thumbs.Clear();

        foreach (var s in sprites)
        {
            if (s == null) continue;
            if (s.texture != null) Destroy(s.texture);
            Destroy(s);
        }
        sprites.Clear();

        if (GameManager.I != null)
            GameManager.I.SetSelectedSprites(new List<Sprite>());
    }

    public void CommitToGameManager()
    {
        GameManager.I.SetSelectedSprites(new List<Sprite>(sprites));

        int have = sprites.Count;
        int need = GameManager.I.PairCount;

        if (!GameManager.I.HasEnoughImages())
            Debug.LogWarning($"Poucas imagens! Precisa de {need}, tem {have}");
        else
            Debug.Log($"[GalleryUI] Commit OK: {have} sprites");
    }
}