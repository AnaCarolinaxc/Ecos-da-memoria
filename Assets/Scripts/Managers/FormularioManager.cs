using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class FormularioManager : MonoBehaviour
{
    [Header("UI - Popup")]
    public GameObject popupFormulario;
    public Transform containerPerguntas;
    public GameObject prefabPergunta;
    public Button btnEnviar;
    public Button btnFechar;

    public GameObject popupFormulario_pzg;
    public Transform containerPerguntas_pzg;
    public GameObject prefabPergunta_pzg;
    public Button btnEnviar_pzg;
    public Button btnFechar_pzg;

    private FormularioData formularioData;
    private List<PerguntaItem> perguntaItems = new List<PerguntaItem>();

    private FormularioData formularioData_pzg;
    private List<PerguntaItem> perguntaItems_pzg = new List<PerguntaItem>();

    void Start()
    {
        popupFormulario.SetActive(false);
        btnEnviar.onClick.AddListener(EnviarFormulario);
        btnFechar.onClick.AddListener(FecharFormulario);

        popupFormulario_pzg.SetActive(false);
        btnFechar_pzg.onClick.AddListener(FecharFormulario_pzg);
    }

    public void AbrirFormulario()
    {
        popupFormulario.SetActive(true);
        StartCoroutine(CarregarEExibir());
    }

     public void AbrirFormulario_pzg()
    {
        popupFormulario_pzg.SetActive(true);
        StartCoroutine(CarregarEExibir_pzg());
    }

    private IEnumerator CarregarEExibir()
    {
        foreach (Transform child in containerPerguntas)
            Destroy(child.gameObject);
        perguntaItems.Clear();

        string path = Path.Combine(Application.streamingAssetsPath, "questions.json");

        #if UNITY_ANDROID && !UNITY_EDITOR
        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();
        string json = www.downloadHandler.text;
        #else
        string json = File.ReadAllText(path);
        yield return null;
        #endif

        formularioData = JsonUtility.FromJson<FormularioData>(json);

        foreach (var pergunta in formularioData.perguntas)
        {
            GameObject obj = Instantiate(prefabPergunta, containerPerguntas);
            PerguntaItem item = obj.GetComponent<PerguntaItem>();
            item.Configurar(pergunta);
            perguntaItems.Add(item);
        }
    }

    private IEnumerator CarregarEExibir_pzg()
    {
        foreach (Transform child in containerPerguntas_pzg)
            Destroy(child.gameObject);
        perguntaItems.Clear();

        string path = Path.Combine(Application.streamingAssetsPath, "questions.json");

        #if UNITY_ANDROID && !UNITY_EDITOR
        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();
        string json = www.downloadHandler.text;
        #else
        string json = File.ReadAllText(path);
        yield return null;
        #endif

        formularioData = JsonUtility.FromJson<FormularioData>(json);

        foreach (var pergunta in formularioData.perguntas)
        {
            GameObject obj = Instantiate(prefabPergunta_pzg, containerPerguntas_pzg);
            PerguntaItem item = obj.GetComponent<PerguntaItem>();
            item.Configurar(pergunta);
            perguntaItems.Add(item);
        }
    }

    public void FecharFormulario()
    {
        popupFormulario.SetActive(false);
    }

     public void FecharFormulario_pzg()
    {
        popupFormulario.SetActive(false);
    }


    void EnviarFormulario()
    {
        RespostasData respostas = new RespostasData();
        respostas.dataHora = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        respostas.respostas = new List<Resposta>();

        foreach (var item in perguntaItems)
        {
            respostas.respostas.Add(new Resposta
            {
                perguntaId = item.pergunta.id,
                textoPergunta = item.pergunta.texto,
                respostaSelecionada = item.RespostaSelecionada()
            });
        }

        SalvarRespostas(respostas);
        FecharFormulario();
    }

    void SalvarRespostas(RespostasData respostas)
{
    string json = JsonUtility.ToJson(respostas, true);

    #if UNITY_ANDROID && !UNITY_EDITOR
    string pasta = "/storage/emulated/0/Download/EcosDaMemoria";
    #else
    string pasta = Path.Combine(Application.persistentDataPath, "Respostas");
    #endif

    if (!Directory.Exists(pasta))
        Directory.CreateDirectory(pasta);

    string nomeArquivo = "resposta_" +
        System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
    string caminhoCompleto = Path.Combine(pasta, nomeArquivo);

    File.WriteAllText(caminhoCompleto, json);
    Debug.Log("Respostas salvas em: " + caminhoCompleto);
}
}