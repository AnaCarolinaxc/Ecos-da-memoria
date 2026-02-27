using UnityEngine;

public class StartGameButton : MonoBehaviour
{
    public GalleryUI gallery;
    public UIRouter uiRouter;

    [Header("Popup")]
    public NotEnoughImagesPopup notEnoughPopup;

    public void StartGame()
    {
        if (gallery == null)
        {
            Debug.LogError("StartGameButton: gallery está NULL");
            return;
        }

        gallery.CommitToGameManager();

        int available = GameManager.I.GetSprites() == null ? 0 : GameManager.I.GetSprites().Count;
        Debug.Log($"[StartGameButton] after commit, selectedSprites={available}");

        if (!GameManager.I.HasEnoughImages())
        {
            int needed = GameManager.I.PairCount; // pares necessários
            string msg = $"Você precisa selecionar pelo menos {needed} imagens.\n" +
                         $"Selecionadas: {available}.";

            if (notEnoughPopup != null) notEnoughPopup.Show(msg);
            else Debug.LogError("Popup não configurado (notEnoughPopup == null)");

            return;
        }

        if (uiRouter == null)
        {
            Debug.LogError("StartGameButton: uiRouter está NULL");
            return;
        }

        uiRouter.ShowPlaying();

        var gb = FindObjectOfType<GameBoard>(true);
        if (gb == null) { Debug.LogError("GameBoard não encontrado na cena!"); return; }

        gb.StartGame();
    }
}
