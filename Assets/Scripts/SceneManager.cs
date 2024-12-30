using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomSceneManager : MonoBehaviour
{
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadTitleScene()
    {
        SceneManager.LoadScene("Title");
    }

    public void LoadResultScene()
    {
        SceneManager.LoadScene("Result");
    }

    public void LoadMainScene()
    {
        SceneManager.LoadScene("Main");
    }
}