using System.Collections.Generic;

[System.Serializable]
public class Pergunta
{
    public int id;
    public string texto;
    public List<string> opcoes;
}

[System.Serializable]
public class FormularioData
{
    public List<Pergunta> perguntas;
}

[System.Serializable]
public class Resposta
{
    public int perguntaId;
    public string textoPergunta;
    public string respostaSelecionada;
}

[System.Serializable]
public class RespostasData
{
    public string dataHora;
    public List<Resposta> respostas;
}