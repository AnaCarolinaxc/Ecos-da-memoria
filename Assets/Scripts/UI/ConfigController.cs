using UnityEngine;

public class ConfigController : MonoBehaviour
{
    public void SetBoard2x2() { GameManager.I.rows = 2; GameManager.I.cols = 2; }
    public void SetBoard3x2() { GameManager.I.rows = 3; GameManager.I.cols = 2; }
    public void SetBoard4x2() { GameManager.I.rows = 4; GameManager.I.cols = 2; }
    public void SetBoard4x3() { GameManager.I.rows = 4; GameManager.I.cols = 3; }
}