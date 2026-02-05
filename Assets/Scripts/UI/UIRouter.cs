using UnityEngine;

public class UIRouter : MonoBehaviour
{
    public GameObject home;
    public GameObject ConfigController;
    public GameObject playing;
    public GameObject ranking;

    void Start()
    {
        ShowHome();
    }

    public void ShowHome()
    {
        ShowOnly(home);
    }

    public void ShowConfig()
    {
        ShowOnly(config);
    }       

    public void ShowPlaying()
    {
        ShowOnly(playing);
    }


    public void ShowRanking()
    {
        ShowOnly(ranking);
    }   

    private void ShowOnly(GameObject target)
    {
        home.SetActive(false);
        config.SetActive(false);
        playing.SetActive(false);
        ranking.SetActive(false);

        target.SetActive(true);
    }   
    
}
