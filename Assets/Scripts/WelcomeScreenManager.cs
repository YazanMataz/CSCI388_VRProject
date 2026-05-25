using UnityEngine;

public class WelcomeScreenManager : MonoBehaviour
{
    public void OnStartFixed()
    {
        GameManager.Instance.StartFixed();
    }

    public void OnStartRandom()
    {
        GameManager.Instance.StartRandom();
    }
}