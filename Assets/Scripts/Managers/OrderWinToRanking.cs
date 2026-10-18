using System.Collections;
using UnityEngine;

// Quando o jogador vence, toca um som e vai para o ranking.
public class OrderWinToRanking : MonoBehaviour
{
    public UIRouter uiRouter;
    public float delay = 0.6f;      // pausa DEPOIS do áudio de parabéns terminar

    [Header("Victory Sounds")]
    public AudioSource sfxSource;   // não é mais usado (o parabéns toca pelo UIRouter)
    public AudioClip[] victoryClips;

    bool subscribed;

    void OnEnable() => Subscribe();
    void Start() => Subscribe();
    void OnDisable() => Unsubscribe();

    void Subscribe()
    {
        if (subscribed || OrderManager.I == null) return;
        OrderManager.I.OnWin += HandleWin;
        subscribed = true;
    }

    void Unsubscribe()
    {
        if (!subscribed || OrderManager.I == null) return;
        OrderManager.I.OnWin -= HandleWin;
        subscribed = false;
    }

    void HandleWin()
    {
        StartCoroutine(GoRanking());
    }

    // Toca o parabéns, espera ele terminar (+ "delay") e só então abre a classificação.
    IEnumerator GoRanking()
    {
        float wait = delay;
        ConfettiBurst.Fire(); 
        AudioClip clip = PickVictoryClip();
        if (clip != null && uiRouter != null)
        {
            uiRouter.PlayUI(clip);
            wait += clip.length;
        }

        yield return new WaitForSeconds(wait);

        if (uiRouter != null) uiRouter.Show_aogRanking();
        else Debug.LogError("OrderWinToRanking: uiRouter está NULL");
    }

    AudioClip PickVictoryClip()
    {
        if (victoryClips == null || victoryClips.Length == 0) return null;
        return victoryClips[Random.Range(0, victoryClips.Length)];
    }
}