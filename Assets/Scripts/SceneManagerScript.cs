using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    //просто скрипт для смены сцены в юнити
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
