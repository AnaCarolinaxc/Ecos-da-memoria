using UnityEngine;

public class PuzzleWinToRanking : MonoBehaviour
{
    public UIRouter uiRouter;

    void Start()
    {
        PuzzleManager.I.OnWin += HandleWin;
    }

    void OnDisable()
    {
        PuzzleManager.I.OnWin -= HandleWin;
        Debug.Log("[PuzzleWinToRanking] Inscrito no evento OnWin");
    }

    void HandleWin()
    {
        Debug.Log("[PuzzleWinToRanking] HandleWin chamado! Chamando ShowPuzzleRanking()");
        uiRouter.Show_pzgRanking();
    }
}