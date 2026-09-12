using UnityEngine;
using UnityEditor;

public static class RankingEditorTools
{
    [MenuItem("Tools/Ranking/Limpar Ranking do Puzzle")]
    static void ClearPuzzleRanking()
    {
        RankingSystem.Clear("puzzle");
        Debug.Log("Ranking do Puzzle limpo.");
    }

    [MenuItem("Tools/Ranking/Limpar Ranking da Memória")]
    static void ClearMemoryRanking()
    {
        RankingSystem.Clear("memory");
        Debug.Log("Ranking da Memória limpo.");
    }
}