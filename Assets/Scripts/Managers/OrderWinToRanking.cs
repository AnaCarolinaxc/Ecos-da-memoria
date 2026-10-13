using System.Collections;
using UnityEngine;

// Quando o jogador vence, toca um som e vai para o ranking.
public class OrderWinToRanking : MonoBehaviour
{
    public UIRouter uiRouter;
    public float delay = 0.6f;

    [Header("Victory Sounds")]
    public AudioSource sfxSource;
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
        PlayRandomVictorySound();
        StartCoroutine(GoRanking());
    }

    void PlayRandomVictorySound()
    {
        if (sfxSource == null || victoryClips == null || victoryClips.Length == 0) return;
        sfxSource.PlayOneShot(victoryClips[Random.Range(0, victoryClips.Length)]);
    }

    IEnumerator GoRanking()
    {
        yield return new WaitForSeconds(delay);

        if (uiRouter != null) uiRouter.Show_aogRanking();
        else Debug.LogError("OrderWinToRanking: uiRouter está NULL");
    }
}