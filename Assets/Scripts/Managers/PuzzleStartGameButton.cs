using UnityEngine;

public class PuzzleStartGameButton : MonoBehaviour
{
    public UIRouter uiRouter;
    public PuzzleBoard puzzleBoard;

    public void StartGame()
    {
        if (!PuzzleManager.I.HasImage())
        {
            Debug.LogWarning("PuzzleStartGameButton: nenhuma imagem selecionada ainda");
            return;
        }

        uiRouter.Show_pzgPlaying();
        puzzleBoard.StartGame();
    }
}