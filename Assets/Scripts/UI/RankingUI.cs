using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RankingUI : MonoBehaviour
{
    public Transform content;          // ListContainer
    public GameObject rowPrefab;       // RankingRow prefab

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        // limpa
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        List<ScoreEntry> list = RankingSystem.Load();

        if (list.Count == 0)
        {
            SpawnRow(1, 0, 0, true);
            return;
        }

        for (int i = 0; i < list.Count; i++)
            SpawnRow(i + 1, list[i].tries, list[i].seconds, false);
    }

    void SpawnRow(int pos, int tries, int seconds, bool empty)
    {
        var go = Instantiate(rowPrefab, content);

        var tmps = go.GetComponentsInChildren<TMP_Text>();
        // esperado: 2 textos (pos e info)
        TMP_Text posText = tmps[0];
        TMP_Text infoText = tmps[1];

        posText.text = $"{pos}º";

        if (empty) infoText.text = "Sem recordes ainda";
        else infoText.text = $"{tries} tentativas | {seconds}s";
    }
}
