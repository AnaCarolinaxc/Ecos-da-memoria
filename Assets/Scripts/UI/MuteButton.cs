using UnityEngine;
using UnityEngine.UI;

// Muta TODO o áudio do jogo (AudioListener.volume = 0). Os sons continuam "rodando" em silêncio.
// Ao desmutar, o áudio da tela que estava tocando recomeça do início.
[RequireComponent(typeof(Button))]
public class MuteButton : MonoBehaviour
{
    public UIRouter router;       
    public GameObject slash;      // risco que aparece sobre o ícone quando está mutado

    // static: o estado vale para o jogo inteiro e sobrevive a reativações do painel
    static bool muted;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Toggle);
    }

    void OnEnable() => Apply();

    public void Toggle()
    {
        muted = !muted;
        Apply();

        if (!muted && router != null)
            router.RestartCurrentUI();   // desmutou: toca de novo, do começo, o que estava tocando
    }

    void Apply()
    {
        AudioListener.volume = muted ? 0f : 1f;
        if (slash != null) slash.SetActive(muted);
    }
}