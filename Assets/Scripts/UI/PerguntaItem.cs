using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerguntaItem : MonoBehaviour
{
    [HideInInspector] public Pergunta pergunta;
    private List<Toggle> toggles = new List<Toggle>();
    private ToggleGroup toggleGroup;

    public void Configurar(Pergunta p)
    {
        pergunta = p;
        toggleGroup = gameObject.AddComponent<ToggleGroup>();
        toggleGroup.allowSwitchOff = true;

        VerticalLayoutGroup vlg = gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.padding = new RectOffset(12, 12, 12, 12);
        vlg.childControlWidth = true;
        vlg.childForceExpandWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Image bg = gameObject.AddComponent<Image>();
        bg.sprite = Resources.Load<Sprite>("Rectangle 2");
        bg.color = Color.white;
        bg.type = Image.Type.Sliced;

        // Texto da pergunta
        GameObject txtObj = new GameObject("TxtPergunta");
        txtObj.transform.SetParent(transform, false);
        TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
        txt.text = p.texto;
        txt.fontSize = 40;
        txt.color = new Color(0.4f, 0.3f, 0.6f);
        txt.fontStyle = FontStyles.Bold;
        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.sizeDelta = new Vector2(0, 30);
        ContentSizeFitter txtCsf = txtObj.AddComponent<ContentSizeFitter>();
        txtCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Container de opções em Grid
        GameObject gridObj = new GameObject("GridOpcoes");
        gridObj.transform.SetParent(transform, false);

        RectTransform gridRect = gridObj.AddComponent<RectTransform>();
        gridRect.sizeDelta = new Vector2(0, 100);

        GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(300, 60);
        grid.spacing = new Vector2(9, 9);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 1;

        ContentSizeFitter gridCsf = gridObj.AddComponent<ContentSizeFitter>();
        gridCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        foreach (string opcao in p.opcoes)
        {
            GameObject opcaoObj = new GameObject("Opcao_" + opcao);
            opcaoObj.transform.SetParent(gridObj.transform, false);

            HorizontalLayoutGroup hlg = opcaoObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6;
            hlg.padding = new RectOffset(4, 4, 4, 4);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlHeight = false;
            hlg.childControlWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childForceExpandWidth = false;

            // Toggle
            GameObject toggleObj = new GameObject("Toggle");
            toggleObj.transform.SetParent(opcaoObj.transform, false);

            RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
            toggleRect.sizeDelta = new Vector2(50, 50);

            LayoutElement toggleLE = toggleObj.AddComponent<LayoutElement>();
            toggleLE.minWidth = 50;
            toggleLE.minHeight = 50;
            toggleLE.preferredWidth = 50;
            toggleLE.preferredHeight = 50;

            Image toggleImg = toggleObj.AddComponent<Image>();
            toggleImg.color = new Color(0.9f, 0.9f, 0.9f);

            Toggle toggle = toggleObj.AddComponent<Toggle>();

            // Checkmark
            GameObject checkObj = new GameObject("Checkmark");
            checkObj.transform.SetParent(toggleObj.transform, false);
            RectTransform checkRect = checkObj.AddComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.sizeDelta = Vector2.zero;
            Image checkImg = checkObj.AddComponent<Image>();
            checkImg.color = Color.black;

            toggle.graphic = checkImg;
            toggle.group = toggleGroup;
            toggles.Add(toggle);

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(opcaoObj.transform, false);

            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(300, 50);

            TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
            label.text = opcao;
            label.fontSize = 34;
            label.color = Color.black;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.overflowMode = TextOverflowModes.Overflow;
            label.enableWordWrapping = false;
        }

        foreach (var t in toggles)
            t.isOn = false;
    }

    public string RespostaSelecionada()
    {
        for (int i = 0; i < toggles.Count; i++)
        {
            if (toggles[i].isOn)
                return pergunta.opcoes[i];
        }
        return "Sem resposta";
    }
}