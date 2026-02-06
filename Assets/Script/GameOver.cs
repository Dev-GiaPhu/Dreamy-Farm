using UnityEngine;

public class GameOver : MonoBehaviour
{
    public void PlayAgain()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GamePlay");
    }
    public void Menu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
}
