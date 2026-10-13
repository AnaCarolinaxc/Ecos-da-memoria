using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Uma bolinha do "Acerte a ordem". É criada por código (OrderBall.Create),
// então NÃO precisa de prefab.
public class OrderBall : MonoBehaviour, IPointerDownHandler
{
    public int number;

    // estado da física (usado pelo OrderBoard)
    [HideInInspector] public Vector2 pos;
    [HideInInspector] public Vector2 prev;
    [HideInInspector] public float radius;
    [HideInInspector] public bool touched;
    [HideInInspector] public float vyBefore;

    public bool Blinking { get; private set; }
    public RectTransform RT { get; private set; }

    OrderBoard board;
    Image image;
    Color normalColor;
    Color wrongColor;
    Coroutine blinkRoutine;

    public static OrderBall Create(RectTransform parent, OrderBoard board, int number, float diameter,
                                   Sprite sprite, Color color, Color wrongColor, Color textColor)
    {
        var go = new GameObject("Ball " + number, typeof(RectTransform), typeof(Image), typeof(OrderBall));
        go.transform.SetParent(parent, false);

        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(diameter, diameter);

        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = true;

        var txtGo = new GameObject("Number", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(go.transform, false);
        var txtRt = (RectTransform)txtGo.transform;
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;

        var tmp = txtGo.GetComponent<TextMeshProUGUI>();
        tmp.text = number.ToString();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.fontSize = diameter * 0.45f;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.color = textColor;
        tmp.raycastTarget = false;

        var ball = go.GetComponent<OrderBall>();
        ball.board = board;
        ball.number = number;
        ball.RT = rt;
        ball.image = img;
        ball.normalColor = color;
        ball.wrongColor = wrongColor;
        return ball;
    }

    public void ApplyPosition()
    {
        RT.localPosition = new Vector3(pos.x, pos.y, 0f);
    }

    public void SetClickable(bool value)
    {
        if (image != null) image.raycastTarget = value;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (board != null) board.OnBallPressed(this);
    }

    public void Blink()
    {
        if (blinkRoutine != null) StopCoroutine(blinkRoutine);
        blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkRoutine()
    {
        Blinking = true;
        for (int i = 0; i < 3; i++)
        {
            image.color = wrongColor;
            yield return new WaitForSeconds(0.08f);
            image.color = normalColor;
            yield return new WaitForSeconds(0.08f);
        }
        Blinking = false;
        blinkRoutine = null;
    }
}