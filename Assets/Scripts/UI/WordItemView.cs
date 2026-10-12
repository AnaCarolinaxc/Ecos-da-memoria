using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum WordItemState { Normal, Selected, Correct }

// Um item do jogo: pode ser o card de IMAGEM ou o card de PALAVRA.
// No prefab: Button com Transition = None (as cores são controladas aqui).
[RequireComponent(typeof(Button))]
public class WordItemView : MonoBehaviour
{
    [Header("Refs")]
    public Image background;   // fundo que muda de cor
    public Image icon;         // só no card de imagem
    public TMP_Text label;     // só no card de palavra

    [Header("Cores")]
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.95f, 0.85f, 0.55f);
    public Color correctColor = new Color(0.80f, 0.96f, 0.80f);
    public Color wrongColor = new Color(1f, 0.55f, 0.55f);
    public Color wrongIconColor = new Color(1f, 0.3f, 0.3f);

    public int PairId { get; private set; }
    public bool IsImage { get; private set; }
    public bool IsLocked { get; private set; }
    public AudioClip Clip { get; private set; }

    Button button;
    Coroutine blinkRoutine;

    public void Init(WordBoard board, int pairId, bool isImage, Sprite sprite, string word, AudioClip clip)
    {
        PairId = pairId;
        IsImage = isImage;
        Clip = clip;
        IsLocked = false;

        if (icon != null)
        {
            icon.gameObject.SetActive(isImage);
            if (isImage) { icon.sprite = sprite; icon.preserveAspect = true; }
        }
        if (label != null)
        {
            label.gameObject.SetActive(!isImage);
            if (!isImage) label.text = word;
        }

        button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => board.OnItemClicked(this));

        SetState(WordItemState.Normal);
    }

    public void SetState(WordItemState state)
    {
        if (blinkRoutine != null) { StopCoroutine(blinkRoutine); blinkRoutine = null; }

        switch (state)
        {
            case WordItemState.Normal:
                IsLocked = false;
                SetColor(normalColor);
                break;
            case WordItemState.Selected:
                IsLocked = false;
                SetColor(selectedColor);
                break;
            case WordItemState.Correct:
                IsLocked = true; // acertou: não clica mais
                SetColor(correctColor);
                break;
        }
    }

    // Pisca em vermelho e volta ao normal.
    public void BlinkWrong(int times, float interval)
    {
        if (!gameObject.activeInHierarchy) { SetState(WordItemState.Normal); return; }
        if (blinkRoutine != null) StopCoroutine(blinkRoutine);
        blinkRoutine = StartCoroutine(BlinkRoutine(times, interval));
    }

    IEnumerator BlinkRoutine(int times, float interval)
    {
        for (int i = 0; i < times; i++)
        {
            SetColor(wrongColor, true);   // fundo e imagem vermelhos
            yield return new WaitForSeconds(interval);
            SetColor(normalColor);        // volta ao normal (imagem branca)
            yield return new WaitForSeconds(interval);
        }
        blinkRoutine = null;
        SetState(WordItemState.Normal);
    }

    void SetColor(Color c, bool tintIcon = false)
    {
        if (background != null) background.color = c;
        if (icon != null) icon.color = tintIcon ? wrongIconColor : Color.white;
    }
}