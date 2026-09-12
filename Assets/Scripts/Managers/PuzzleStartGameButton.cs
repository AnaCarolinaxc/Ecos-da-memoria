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
        Canvas.ForceUpdateCanvases(); // força o layout do painel de jogo a atualizar antes de gerar as peças
        puzzleBoard.StartGame();
    }
}